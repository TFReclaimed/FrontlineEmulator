using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;

namespace Frontline.Endpoints.Guilds.GetGiftStatus;

public class GetGiftStatusEndpoint : EndpointWithoutRequest<GuildGiftStatusResponse>
{
    private readonly IPlayerRepository _playerRepository;

    public GetGiftStatusEndpoint(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public override void Configure()
    {
        Get("/Dealership/v1/guild");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = this.GetUserId();
        var player = await _playerRepository.GetByIdAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to get gift status but was not found", userId);
            await Send.NotFoundAsync();
            return;
        }
        
        var response = new GuildGiftStatusResponse
        {
            Time = player.LastGiftSent + TimeSpan.FromHours(1)
        };
        
        await Send.OkAsync(response);
    }
}