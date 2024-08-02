using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Repositories;

namespace Frontline.Features.Guilds.GetGiftStatus;

public class Endpoint : EndpointWithoutRequest<GuildGiftStatusResponse>
{
    private readonly IPlayerRepository _playerRepository;

    public Endpoint(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public override void Configure()
    {
        Get("/Dealership/v1/guild");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = int.Parse(User.ClaimValue("UserId")!);
        var player = await _playerRepository.GetPlayerAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to get gift status but was not found", userId);
            await SendNotFoundAsync();
            return;
        }
        
        var response = new GuildGiftStatusResponse
        {
            Time = player.LastGiftSent + TimeSpan.FromHours(1)
        };
        
        await SendAsync(response);
    }
}