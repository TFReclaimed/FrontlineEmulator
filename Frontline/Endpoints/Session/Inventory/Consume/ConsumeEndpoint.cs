using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Frontline.Game;

namespace Frontline.Endpoints.Session.Inventory.Consume;

public class ConsumeEndpoint : Endpoint<ConsumeRequest>
{
    private readonly IInventoryRepository _inventoryRepository;

    private readonly IPlayerRepository _playerRepository;

    public ConsumeEndpoint(IInventoryRepository inventoryRepository, IPlayerRepository playerRepository)
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
        var userId = this.GetUserId();

        var item = await _inventoryRepository.GetItemAsync(userId, req.ItemId);
        if (item is null)
        {
            Logger.LogWarning("Player {UserId} tried to consume item {ItemId} but it wasn't found!",
                userId, req.ItemId);
            await Send.ResultAsync(Results.NotFound("Item not found!"));
            return;
        }

        var cardTemplate = RulesetParser.GetCardTemplate(item.TemplateId);
        if (cardTemplate is null)
        {
            Logger.LogWarning("Player {UserId} tried to consume item {ItemId} but card template {TemplateId} wasn't found!",
                userId, req.ItemId, item.TemplateId);
            await Send.ResultAsync(Results.NotFound("Card template not found!"));
            return;
        }

        if (item.IsInDropship)
        {
            Logger.LogWarning("Player {UserId} tried to consume item {ItemId} but it's in a dropship!",
                userId, req.ItemId);
            await Send.ResultAsync(Results.BadRequest("Item is in dropship!"));
            return;
        }

        if (item.CurrentMission is not null)
        {
            Logger.LogWarning("Player {UserId} tried to consume item {ItemId} but it's on a mission!",
                userId, req.ItemId);
            await Send.ResultAsync(Results.BadRequest("Item is on a mission!"));
            return;
        }

        if (item.Casualty)
        {
            Logger.LogWarning("Player {UserId} tried to consume item {ItemId} but it's injured/damaged!",
                userId, req.ItemId);
            await Send.ResultAsync(Results.BadRequest("Item is injured/damaged!"));
            return;
        }

        if (req.RetireFor is null)
        {
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
            await Send.ResultAsync(Results.BadRequest("Item is not an XP card!"));
            return;
        }

        var targetItem = await _inventoryRepository.GetItemAsync(userId, req.TargetId!.Value);
        if (targetItem is null)
        {
            Logger.LogWarning("Player {UserId} tried to use XP card {ItemId} for target {TargetId} but target wasn't found!",
                userId, req.ItemId, req.TargetId);
            await Send.ResultAsync(Results.NotFound("Target not found!"));
            return;
        }

        await _inventoryRepository.DeleteAsync(item);

        targetItem.Xp += resourceCardTemplate.ResourceValue;
        await _inventoryRepository.UpdateAsync(targetItem);

        Logger.LogInformation("Player {UserId} used XP card {ItemId} on target {TargetId} for {Xp} XP",
            userId, req.ItemId, req.TargetId, resourceCardTemplate.ResourceValue);

        await Send.OkAsync();
    }

    private async Task RetireForCredits(int userId, ConsumeRequest req, ItemEntity item, CardTemplate cardTemplate)
    {
        if (cardTemplate is ResourceCardTemplate)
        {
            Logger.LogWarning("Player {UserId} tried to retire a resource card {ItemId} for credits!",
                userId, req.ItemId);
            await Send.ResultAsync(Results.BadRequest("Item is a resource card!"));
            return;
        }

        var player = await _playerRepository.GetByIdAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to retire card {ItemId} but player wasn't found!",
                userId, req.ItemId);
            await Send.ResultAsync(Results.NotFound("Player not found!"));
            return;
        }

        var credits = cardTemplate.CreditsIfRetired(item.Xp);

        player.Credits += credits;

        await _inventoryRepository.DeleteAsync(item);
        await _playerRepository.UpdateAsync(player);

        Logger.LogInformation("Player {UserId} retired card {ItemId} for {Credits} credits",
            userId, req.ItemId, credits);

        var result = new RetireForCreditsResponse
        {
            Credits = credits.ToString()
        };

        await Send.OkAsync(result);
    }

    private async Task RetireForXp(int userId, ConsumeRequest req, ItemEntity item, CardTemplate cardTemplate)
    {
        var targetItem = await _inventoryRepository.GetItemAsync(userId, req.TargetId!.Value);
        if (targetItem is null)
        {
            Logger.LogWarning("Player {UserId} tried retire card {ItemId} for XP but target {TargetId} wasn't found!",
                userId, req.ItemId, req.TargetId);
            await Send.ResultAsync(Results.NotFound("Target not found!"));
            return;
        }

        var xp = cardTemplate.XpIfRetired(item.Xp);

        targetItem.Xp += xp;

        await _inventoryRepository.DeleteAsync(item);
        await _inventoryRepository.UpdateAsync(targetItem);

        Logger.LogInformation("Player {UserId} retired card {ItemId} to target {TargetId} for {Xp} XP",
            userId, req.ItemId, req.TargetId, xp);

        var result = new RetireForXpResponse
        {
            Xp = xp.ToString()
        };

        await Send.OkAsync(result);
    }
}