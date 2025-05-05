using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Frontline.Features.Session.Inventory.GetInventory;
using Frontline.Game;

namespace Frontline.Features.Session.Inventory.Booster.OpenBooster;

public class Endpoint : Endpoint<OpenBoosterPackRequest, BoosterPackResponse>
{
    private readonly IPlayerRepository _playerRepository;

    private readonly IInventoryRepository _inventoryRepository;

    public Endpoint(IPlayerRepository playerRepository, IInventoryRepository inventoryRepository)
    {
        _playerRepository = playerRepository;
        _inventoryRepository = inventoryRepository;
    }

    public override void Configure()
    {
        Post("/session/booster/{BoosterId}");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(OpenBoosterPackRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var player = await _playerRepository.GetPlayerAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to open a booster pack but the profile wasn't found!", userId);
            await SendNotFoundAsync();
            return;
        }
        
        if (player.BoosterPackCount <= 0)
        {
            Logger.LogWarning("Player {UserId} tried to open a booster pack but they don't have any!", userId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }

        if (RulesetParser.Ruleset is null)
        {
            Logger.LogWarning("Player {UserId} tried to open a booster pack but card ruleset is null!", userId);
            await SendResultAsync(TypedResults.InternalServerError());
            return;
        }

        player.BoosterPackCount--;
        await _playerRepository.UpdatePlayerAsync(player);

        var isUltraRare = Random.Shared.Next(0, 2) == 0;
        var potentialRareCards = RulesetParser.Ruleset.CardsRuleset.Cards.Values
            .Where(x => x.Type != CardType.Resource &&
                        x.Rarity == (isUltraRare ? CardRarity.UltraRare : CardRarity.Rare))
            .ToList();

        var potentialResourceCards = RulesetParser.Ruleset.CardsRuleset.Cards.Values
            .Where(x => x.Type == CardType.Resource && x is ResourceCardTemplate
            {
                ResourceType: ResourceType.Xp or ResourceType.Credit or ResourceType.Supply or ResourceType.Intel
            })
            .ToList();

        var potentialCommonCards = RulesetParser.Ruleset.CardsRuleset.Cards.Values
            .Where(x => x.Type != CardType.Resource && x.Rarity == CardRarity.Common)
            .ToList();
        
        var rareCardTemplate = potentialRareCards[Random.Shared.Next(0, potentialRareCards.Count)];
        var resourceCardTemplate = potentialResourceCards[Random.Shared.Next(0, potentialResourceCards.Count)];
        var commonCards = potentialCommonCards
            .OrderBy(_ => Random.Shared.Next())
            .Take(3)
            .ToList();

        List<CardTemplate> cardTemplates = [
            rareCardTemplate,
            resourceCardTemplate,
            ..commonCards
        ];
        
        List<ItemEntity> cardEntities = [];
        foreach (var template in cardTemplates)
        {
            cardEntities.Add(ItemEntity.FromTemplate(template));
        }

        await _inventoryRepository.AddItemsAsync(userId, cardEntities);
        // TODO: handle credit and supply separately

        var response = new BoosterPackResponse
        {
            Cards = cardEntities
                .Where(x => x.TemplateId != resourceCardTemplate.CardId)
                .Select(x => InventoryCard.FromItemEntity(x))
                .ToList(),
            Resources = cardEntities
                .Where(x => x.TemplateId == resourceCardTemplate.CardId)
                .Select(x => InventoryCard.FromItemEntity(x, resourceCardTemplate))
                .ToList()
        };
        
        await SendAsync(response);
    }
}