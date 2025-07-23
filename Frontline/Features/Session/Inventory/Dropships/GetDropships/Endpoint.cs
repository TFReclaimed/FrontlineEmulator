using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Frontline.Features.Session.Inventory.GetInventory;
using Frontline.Game;

namespace Frontline.Features.Session.Inventory.Dropships.GetDropships;

public class Endpoint : Endpoint<GetInventoryRequest, List<DropshipInfo>>
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
        Get("/session/dropships");
    }

    public override async Task HandleAsync(GetInventoryRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var player = await _playerRepository.GetPlayerAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player not found: {UserId}", userId);
            await Send.NotFoundAsync();
            return;
        }

        var dropshipItems = _inventoryRepository.GetDropshipItems(userId);

        var response = new List<DropshipInfo>();
        
        foreach (var dropship in dropshipItems.GroupBy(x => x.DropshipId).ToList())
        {
            var slottedCards = new InventoryCard?[41];
            
            var dropshipEntities = dropship.ToList();
            
            foreach (var dropshipEntity in dropshipEntities)
            {
                var item = dropshipEntity.Item!;
                
                var cardTemplate = RulesetParser.GetCardTemplate(item.TemplateId);
                var isCommander = cardTemplate!.Type == CardType.Commander;
                
                slottedCards[dropshipEntity.SlotIndex] = new InventoryCard
                {
                    Type = isCommander ? "CommanderCard" : "Card",
                    InstanceId = item.ItemId,
                    TemplateId = item.TemplateId,
                    Rank = item.Rank,
                    Xp = item.Xp
                };
            }
            
            for (var i = 0; i < 41; i++)
            {
                if (slottedCards[i] != null)
                {
                    continue;
                }
                
                slottedCards[i] = new InventoryCard
                {
                    Type = "Card",
                    InstanceId = 0,
                    TemplateId = 0
                };
            }
            
            response.Add(new DropshipInfo
            {
                Index = dropship.Key,
                SlottedCards = slottedCards!,
                InstanceId = dropship.Key
            });
        }
        
        await Send.OkAsync(response);
    }
}