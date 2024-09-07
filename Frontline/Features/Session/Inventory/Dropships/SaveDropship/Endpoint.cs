using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Frontline.Game;

namespace Frontline.Features.Session.Inventory.Dropships.SaveDropship;

public class Endpoint : Endpoint<SaveDropshipRequest>
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
        Post("/session/dropship/{DropshipId}");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(SaveDropshipRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var player = await _playerRepository.GetPlayerAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} not found", userId);
            await SendNotFoundAsync();
            return;
        }
        
        if (req.DropshipId != 10 && req.DropshipId != 11 && player.Level < 3)
        {
            Logger.LogWarning("Player {UserId} attempted to save invalid dropship {DropshipId}",
                userId, req.DropshipId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var itemIds = req.Param.InstanceIds;
        
        var usedItems = itemIds.Where(x => x != 0).ToList();
        if (!await _inventoryRepository.HasItemsAsync(userId, usedItems))
        {
            Logger.LogWarning("Player {UserId} attempted to save dropship with invalid items", userId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        if (usedItems.GroupBy(x => x).Any(x => x.Count() > 1))
        {
            Logger.LogWarning("Player {UserId} attempted to save dropship with duplicate items", userId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var itemEntities = _inventoryRepository.GetItems(userId, usedItems);
        
        for (var i = 0; i < itemIds.Length; i++)
        {
            var itemId = itemIds[i];
            if (itemId == 0)
            {
                continue;
            }
            
            var item = itemEntities.First(x => x.ItemId == itemId);
            if (item.CurrentMission is not null)
            {
                Logger.LogWarning("Player {UserId} attempted to save dropship with item on mission {ItemId}",
                    userId, itemId);
                await SendResultAsync(TypedResults.BadRequest());
                return;
            }

            if (item.Casualty)
            {
                Logger.LogWarning("Player {UserId} attempted to save dropship with injured item {ItemId}",
                    userId, itemId);
                await SendResultAsync(TypedResults.BadRequest());
                return;
            }
            
            var cardTemplate = RulesetParser.GetCardTemplate(item.TemplateId);
            if (cardTemplate is null)
            {
                Logger.LogWarning("Could not find card template for item {ItemId}", itemId);
                await SendResultAsync(TypedResults.BadRequest());
                return;
            }

            if (i < 30)
            {
                if (!RulesetParser.IsCommandDeckCard(item.TemplateId))
                {
                    continue;
                }
                
                Logger.LogWarning("Player {UserId} attempted to save dropship with invalid deck card {ItemId}",
                    userId, itemId);
                await SendResultAsync(TypedResults.BadRequest());
                return;
            }

            if (i == 30)
            {
                if (cardTemplate.Type == CardType.Commander)
                {
                    continue;
                }
                
                Logger.LogWarning("Player {UserId} attempted to save dropship with invalid commander card {ItemId}",
                    userId, itemId);
                await SendResultAsync(TypedResults.BadRequest());
                return;
            }

            if (RulesetParser.IsCommandDeckCard(item.TemplateId))
            {
                continue;
            }
            
            Logger.LogWarning("Player {UserId} attempted to save dropship with invalid support card {ItemId}",
                userId, itemId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        await _inventoryRepository.ClearDropshipItemsAsync(userId, req.DropshipId);
        
        var dropshipItems = new List<DropshipEntity>();
        
        for (var i = 0; i < itemIds.Length; i++)
        {
            var itemId = itemIds[i];
            if (itemId == 0)
            {
                continue;
            }
            
            dropshipItems.Add(new DropshipEntity
            {
                UserId = userId,
                DropshipId = req.DropshipId,
                SlotIndex = i,
                ItemId = itemId
            });
        }
        
        await _inventoryRepository.AddDropshipItemsAsync(userId, dropshipItems);
        
        Logger.LogInformation("Player {UserId} updated dropship {DropshipId}", userId, req.DropshipId);
        
        await SendOkAsync();
    }
}