using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Game;

namespace Frontline.Features.Session.Inventory.Consume;

public class Endpoint : Endpoint<ConsumeRequest>
{
    private readonly IInventoryRepository _inventoryRepository;
    
    private readonly IPlayerRepository _playerRepository;

    public Endpoint(IInventoryRepository inventoryRepository, IPlayerRepository playerRepository)
    {
        _inventoryRepository = inventoryRepository;
        _playerRepository = playerRepository;
    }

    public override void Configure()
    {
        Post("/session/consume/{ItemId}");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(ConsumeRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.ClaimValue("UserId")!);

        var item = await _inventoryRepository.GetItemAsync(userId, req.ItemId);
        if (item is null)
        {
            Logger.LogWarning("Player {UserId} tried to consume item {ItemId} but it wasn't found!",
                userId, req.ItemId);
            await SendNotFoundAsync();
            return;
        }
        
        var cardTemplate = RulesetParser.GetCardTemplate(item.TemplateId);
        if (cardTemplate is null)
        {
            Logger.LogWarning("Player {UserId} tried to consume item {ItemId} but card template {TemplateId} wasn't found!",
                userId, req.ItemId, item.TemplateId);
            await SendNotFoundAsync();
            return;
        }

        if (item.IsInDropship)
        {
            Logger.LogWarning("Player {UserId} tried to consume item {ItemId} but it's in a dropship!",
                userId, req.ItemId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        if (item.CurrentMission is not null)
        {
            Logger.LogWarning("Player {UserId} tried to consume item {ItemId} but it's on a mission!",
                userId, req.ItemId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }

        if (item.Casualty)
        {
            Logger.LogWarning("Player {UserId} tried to consume item {ItemId} but it's injured/damaged!",
                userId, req.ItemId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }

        if (req.RetireFor is null)
        {
            // TODO: Doesn't work. Game has all xp resource card instance ids as 0 for some reason
            await UseXpCard(userId, req, item, cardTemplate);
        }
        else if (req.RetireFor == RetireFor.CREDITS)
        {
            await RetireForCredits(userId, req, item, cardTemplate);
        }
        else if (req.RetireFor == RetireFor.XP)
        {
            await RetireForXp(userId, req, item, cardTemplate);
        }
    }

    private async Task UseXpCard(int userId, ConsumeRequest req, ItemEntity item, CardTemplate cardTemplate)
    {
        if (cardTemplate is not ResourceCardTemplate { ResourceType: ResourceType.Xp } resourceCardTemplate)
        {
            Logger.LogWarning("Player {UserId} tried to use XP card {ItemId} despite it not being one!",
                userId, req.ItemId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var targetItem = await _inventoryRepository.GetItemAsync(userId, req.TargetId!.Value);
        if (targetItem is null)
        {
            Logger.LogWarning("Player {UserId} tried to use XP card {ItemId} for target {TargetId} but target wasn't found!",
                userId, req.ItemId, req.TargetId);
            await SendNotFoundAsync();
            return;
        }

        await _inventoryRepository.RemoveItemAsync(item);
        
        targetItem.Xp += resourceCardTemplate.ResourceValue;
        await _inventoryRepository.UpdateItemAsync(targetItem);
        
        Logger.LogInformation("Player {UserId} used XP card {ItemId} on target {TargetId} for {Xp} XP",
            userId, req.ItemId, req.TargetId, resourceCardTemplate.ResourceValue);
        
        await SendOkAsync();
    }

    private async Task RetireForCredits(int userId, ConsumeRequest req, ItemEntity item, CardTemplate cardTemplate)
    {
        if (cardTemplate is ResourceCardTemplate)
        {
            Logger.LogWarning("Player {UserId} tried to retire a resource card {ItemId} for credits!",
                userId, req.ItemId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var player = await _playerRepository.GetPlayerAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to retire card {ItemId} but player wasn't found!",
                userId, req.ItemId);
            await SendNotFoundAsync();
            return;
        }
        
        var credits = cardTemplate.CreditsIfRetired(item.Xp);
        
        player.Credits += credits;
        
        await _inventoryRepository.RemoveItemAsync(item);
        await _playerRepository.UpdatePlayerAsync(player);
        
        Logger.LogInformation("Player {UserId} retired card {ItemId} for {Credits} credits",
            userId, req.ItemId, credits);
        
        var result = new RetireForCreditsResponse
        {
            Credits = credits.ToString()
        };
        
        await SendAsync(result);
    }

    private async Task RetireForXp(int userId, ConsumeRequest req, ItemEntity item, CardTemplate cardTemplate)
    {
        var targetItem = await _inventoryRepository.GetItemAsync(userId, req.TargetId!.Value);
        if (targetItem is null)
        {
            Logger.LogWarning("Player {UserId} tried retire card {ItemId} for XP but target {TargetId} wasn't found!",
                userId, req.ItemId, req.TargetId);
            await SendNotFoundAsync();
            return;
        }

        var xp = cardTemplate.XpIfRetired(item.Xp);
        
        targetItem.Xp += xp;
        
        await _inventoryRepository.RemoveItemAsync(item);
        await _inventoryRepository.UpdateItemAsync(targetItem);
        
        Logger.LogInformation("Player {UserId} retired card {ItemId} to target {TargetId} for {Xp} XP",
            userId, req.ItemId, req.TargetId, xp);
        
        var result = new RetireForXpResponse
        {
            Xp = xp.ToString()
        };
        
        await SendAsync(result);
    }
}