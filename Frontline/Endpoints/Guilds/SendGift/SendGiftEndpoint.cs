using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Extensions;

namespace Frontline.Endpoints.Guilds.SendGift;

public class SendGiftEndpoint : Endpoint<SendGiftRequest, SendGiftResponse>
{
    private readonly IPlayerRepository _playerRepository;

    private readonly IInventoryRepository _inventoryRepository;

    public SendGiftEndpoint(IPlayerRepository playerRepository, IInventoryRepository inventoryRepository)
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
        var userId = this.GetUserId();
        var player = await _playerRepository.GetByIdAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to send a gift but was not found", userId);
            await Send.NotFoundAsync();
            return;
        }
        
        if (DateTime.UtcNow - player.LastGiftSent < TimeSpan.FromHours(1))
        {
            Logger.LogWarning("Player {UserId} tried to send a gift too soon", userId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }
        
        if (player.Id == req.ReceiverId)
        {
            Logger.LogWarning("Player {UserId} tried to send a gift to themselves", userId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var receiver = await _playerRepository.GetByIdAsync(req.ReceiverId);
        if (receiver is null)
        {
            Logger.LogWarning("Player {UserId} tried to send a gift to {ReceiverId} but the receiver was not found",
                userId, req.ReceiverId);
            await Send.NotFoundAsync();
            return;
        }
        
        Logger.LogInformation("Player {UserId} sent a gift to {ReceiverId}", userId, req.ReceiverId);
        
        player.LastGiftSent = DateTime.UtcNow;
        await _playerRepository.UpdateAsync(player);

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

        await Send.OkAsync(result);
    }
}