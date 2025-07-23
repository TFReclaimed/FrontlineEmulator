using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Frontline.Game;

namespace Frontline.Features.Session.Inventory.Upgrade;

public class Endpoint : Endpoint<UpgradeRequest, UpgradedCard, Mapper>
{
    private readonly IInventoryRepository _inventoryRepository;

    public Endpoint(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public override void Configure()
    {
        Post("/session/upgrade/{ItemId}");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(UpgradeRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        
        var item = await _inventoryRepository.GetItemAsync(userId, req.ItemId);
        if (item is null)
        {
            Logger.LogWarning("Player {UserId} tried to upgrade item {ItemId} but it wasn't found!",
                userId, req.ItemId);
            await Send.NotFoundAsync();
            return;
        }
        
        var cardTemplate = RulesetParser.GetCardTemplate(item.TemplateId);
        if (cardTemplate is null)
        {
            Logger.LogWarning("Player {UserId} tried to upgrade item {ItemId} but card template {TemplateId} wasn't found!",
                userId, req.ItemId, item.TemplateId);
            await Send.NotFoundAsync();
            return;
        }

        if (!cardTemplate.IsCombatUnit())
        {
            Logger.LogWarning("Player {UserId} tried to upgrade item {ItemId} but it's not a combat unit!",
                userId, req.ItemId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var nextRank = item.Rank + 1;
        
        var xpEntry = RulesetParser.GetXpEntry(cardTemplate.Type, nextRank);
        if (xpEntry is null)
        {
            Logger.LogWarning("Player {UserId} tried to upgrade item {ItemId} but xp entry for rank {Rank} wasn't found!",
                userId, req.ItemId, nextRank);
            await Send.NotFoundAsync();
            return;
        }
        
        var upgradeCost = CalculateUpgradeCost(cardTemplate.Type, nextRank);
        if (item.Xp < upgradeCost)
        {
            Logger.LogWarning("Player {UserId} tried to upgrade item {ItemId} but it doesn't have enough xp!",
                userId, req.ItemId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }
        
        item.Rank = (sbyte) nextRank;
        
        await _inventoryRepository.UpdateItemAsync(item);
        
        Logger.LogInformation("Player {UserId} upgraded item {ItemId} to rank {Rank}",
            userId, req.ItemId, item.Rank);
        
        var result = Map.FromEntity(item);
        await Send.OkAsync(result);
    }

    private int CalculateUpgradeCost(CardType type, int rank)
    {
        var totalCost = 0;
        
        for (var i = 0; i < rank; i++)
        {
            var xpEntry = RulesetParser.GetXpEntry(type, i);
            if (xpEntry is not null)
            {
                totalCost += xpEntry.XpRequired;
            }
        }
        
        return totalCost;
    }
}