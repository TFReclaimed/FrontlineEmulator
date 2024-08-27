using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;

namespace Frontline.Features.Guilds.SendGift;

public class Endpoint : Endpoint<SendGiftRequest, SendGiftResponse>
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
        Post("/Dealership/v1/guild");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(SendGiftRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.ClaimValue("UserId")!);
        var player = await _playerRepository.GetPlayerAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to send a gift but was not found", userId);
            await SendNotFoundAsync();
            return;
        }
        
        if (DateTime.UtcNow - player.LastGiftSent < TimeSpan.FromHours(1))
        {
            Logger.LogWarning("Player {UserId} tried to send a gift too soon", userId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var receiver = await _playerRepository.GetPlayerAsync(req.ReceiverId);
        if (receiver is null)
        {
            Logger.LogWarning("Player {UserId} tried to send a gift to {ReceiverId} but the receiver was not found",
                userId, req.ReceiverId);
            await SendNotFoundAsync();
            return;
        }
        
        Logger.LogInformation("Player {UserId} sent a gift to {ReceiverId}", userId, req.ReceiverId);
        
        player.LastGiftSent = DateTime.UtcNow;
        await _playerRepository.UpdatePlayerAsync(player);

        var gift = new ItemEntity
        {
            TemplateId = 654,
            Rank = 0
        };
        
        await _inventoryRepository.AddItemAsync(receiver.Id, gift);
        
        var result = new SendGiftResponse
        {
            Fulfillment = true
        };

        await SendAsync(result);
    }
}