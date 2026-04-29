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

    public OpenBoosterEndpoint(IPlayerRepository playerRepository, IInventoryRepository inventoryRepository,
        IUserService userService)
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

        var userObtainedItems = (await _inventoryRepository.GetUserItemsAsync(userId))
            .Where(x => cardSet.CardIds.Contains(x.TemplateId))
            .GroupBy(item => item.TemplateId)
            .Select(items => items.First())
            .ToList()
            .ConvertAll(cardEntity => RulesetParser.GetCardTemplate(cardEntity.TemplateId))
            .FindAll(template => template is not null);
        
        var userObtainedItemIds = userObtainedItems.Select(obtainedItem => obtainedItem?.CardId);

        var userUnObtainedItems = cardSet.CardIds
            .FindAll(cardId => !userObtainedItemIds.Contains(cardId))
            .ConvertAll(RulesetParser.GetCardTemplate)
            .FindAll(template => template is not null);

        List<CardRarity> pickedCardRarities = [
            GetCardRarity(13),
            GetCardRarity(),
            GetCardRarity(),
            GetCardRarity()
        ];

        var pickedCardTemplates = pickedCardRarities
            .ConvertAll(rarity => GetCardTemplate(userUnObtainedItems!, userObtainedItems!, rarity));

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

        var resourceCardTemplate = (ResourceCardTemplate) potentialResourceCards[Random.Shared.Next(0, potentialResourceCards.Count)];

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

        if (addResourceCard)
        {
            pickedCardTemplates.Add(resourceCardTemplate);
        }

        List<ItemEntity> cardEntities = [];
        foreach (var template in pickedCardTemplates)
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

    private static CardTemplate GetCardTemplate(List<CardTemplate> unObtainedCards, List<CardTemplate> obtainedCards,
        CardRarity rarity)
    {
        var unObtainedCardsWithRarity = unObtainedCards
            .Where(card => card.Rarity == rarity)
            .ToList();
        
        var fromUnObtained = Random.Shared.Next(0, 2) == 0;

        if (unObtainedCardsWithRarity.Count != 0 && fromUnObtained)
        {
            return unObtainedCardsWithRarity.OrderBy(_ => Random.Shared.Next()).First();
        }
        
        return obtainedCards
            .Where(card => card.Rarity == rarity)
            .OrderBy(_ => Random.Shared.Next()).First();
    }

    private static CardRarity GetCardRarity(int lowerBoundary = 1)
    {
        var cardRarityRng = Random.Shared.Next(lowerBoundary, 16);

        return cardRarityRng switch
        {
            15 => CardRarity.UltraRare,
            > 12 => CardRarity.Rare,
            > 8 => CardRarity.Uncommon,
            _ => CardRarity.Common
        };
    }
}