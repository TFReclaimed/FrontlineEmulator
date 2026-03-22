using FastEndpoints;
using Frontline.Battle.Data;
using Frontline.Battle.Data.Card;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Frontline.Services;

namespace Frontline.Endpoints.Session.Inventory.Booster.OpenBooster;

public class OpenBoosterEndpoint : Endpoint<OpenBoosterPackRequest, BoosterPackResponse>
{
    private readonly IPlayerRepository _playerRepository;

    private readonly IInventoryRepository _inventoryRepository;

    private readonly IUserService _userService;

    public OpenBoosterEndpoint(IPlayerRepository playerRepository, IInventoryRepository inventoryRepository, IUserService userService)
    {
        _playerRepository = playerRepository;
        _inventoryRepository = inventoryRepository;
        _userService = userService;
    }

    public override void Configure()
    {
        Post("/session/booster/{BoosterId}");
        AllowFormData(true);
    }

    public override async Task HandleAsync(OpenBoosterPackRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var player = await _playerRepository.GetByIdAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to open a booster pack but the profile wasn't found!", userId);
            await Send.NotFoundAsync();
            return;
        }

        if (player.BoosterPackCount <= 0)
        {
            Logger.LogWarning("Player {UserId} tried to open a booster pack but they don't have any!", userId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        if (RulesetParser.Ruleset is null)
        {
            Logger.LogWarning("Player {UserId} tried to open a booster pack but card ruleset is null!", userId);
            await Send.ResultAsync(TypedResults.InternalServerError());
            return;
        }

        player.BoosterPackCount--;
        await _playerRepository.UpdateAsync(player);

        var cardSet = RulesetParser.GetCardSetEntry(1);
        if (cardSet is null)
        {
            Logger.LogWarning("Player {UserId} tried to open a booster pack but card set 1 is null!", userId);
            await Send.ResultAsync(TypedResults.InternalServerError());
            return;
        }

        var isUltraRare = Random.Shared.Next(0, 2) == 0;
        var potentialRareCards = RulesetParser.Ruleset.CardsRuleset.Cards.Values
            .Where(x => cardSet.CardIds.Contains(x.CardId))
            .Where(x => x.Type != CardType.Resource &&
                        x.Rarity == (isUltraRare ? CardRarity.UltraRare : CardRarity.Rare))
            .ToList();

        var potentialResourceCards = RulesetParser.Ruleset.CardsRuleset.Cards.Values
            .Where(x =>
            {
                if (x.Type != CardType.Resource)
                {
                    return false;
                }

                if (x is ResourceCardTemplate resourceCardTemplate)
                {
                    if (resourceCardTemplate.ResourceType is ResourceType.IntelTypeOperational
                        or ResourceType.IntelTypeTechnical
                        or ResourceType.IntelTypePersonnel)
                    {
                        return true;
                    }

                    if (resourceCardTemplate.ResourceType is ResourceType.Xp
                        or ResourceType.Credit
                        or ResourceType.Supply)
                    {
                        return resourceCardTemplate.ResourceValue > 0;
                    }
                }
                
                return false;
            })
            .ToList();

        var potentialCommonCards = RulesetParser.Ruleset.CardsRuleset.Cards.Values
            .Where(x => cardSet.CardIds.Contains(x.CardId))
            .Where(x => x.Type != CardType.Resource && x.Rarity == CardRarity.Common)
            .ToList();

        var rareCardTemplate = potentialRareCards[Random.Shared.Next(0, potentialRareCards.Count)];
        var resourceCardTemplate = (ResourceCardTemplate) potentialResourceCards[Random.Shared.Next(0, potentialResourceCards.Count)];
        var commonCards = potentialCommonCards
            .OrderBy(_ => Random.Shared.Next())
            .Take(3)
            .ToList();

        var addResourceCard = true;
        if (resourceCardTemplate.ResourceType == ResourceType.Credit)
        {
            player.Credits += resourceCardTemplate.ResourceValue;
            await _playerRepository.UpdateAsync(player);
            _userService.IncrementChangeCounter(userId);
            addResourceCard = false;

            Logger.LogInformation("Player {UserId} got {ResourceValue} credits from a booster pack.",
                userId, resourceCardTemplate.ResourceValue);
        }
        else if (resourceCardTemplate.ResourceType == ResourceType.Supply)
        {
            player.Supply += resourceCardTemplate.ResourceValue;
            await _playerRepository.UpdateAsync(player);
            _userService.IncrementChangeCounter(userId);
            addResourceCard = false;

            Logger.LogInformation("Player {UserId} got {ResourceValue} supply from a booster pack.",
                userId, resourceCardTemplate.ResourceValue);
        }

        List<CardTemplate> cardTemplates = [
            rareCardTemplate,
            ..commonCards
        ];

        if (addResourceCard)
        {
            cardTemplates.Add(resourceCardTemplate);
        }

        List<ItemEntity> cardEntities = [];
        foreach (var template in cardTemplates)
        {
            cardEntities.Add(ItemEntity.FromTemplate(template));
        }

        await _inventoryRepository.AddItemsAsync(userId, cardEntities);

        if (!addResourceCard)
        {
            cardEntities.Add(ItemEntity.FromTemplate(resourceCardTemplate));
        }

        var response = new BoosterPackResponse
        {
            Cards = cardEntities
                .Where(x => x.TemplateId != resourceCardTemplate.CardId)
                .Select(CardDto.FromEntity)
                .ToList(),
            Resources = cardEntities
                .Where(x => x.TemplateId == resourceCardTemplate.CardId)
                .Select(CardDto.FromEntity)
                .ToList()
        };

        await Send.OkAsync(response);
    }
}