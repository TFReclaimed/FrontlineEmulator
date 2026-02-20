using FastEndpoints;
using Frontline.Battle.Matchmaking;
using Frontline.Extensions;

namespace Frontline.Endpoints.Session.Game.CancelRequest;

public class CancelRequestEndpoint : EndpointWithoutRequest
{
    private readonly IMatchmakingService _matchmakingService;

    public CancelRequestEndpoint(IMatchmakingService matchmakingService)
    {
        _matchmakingService = matchmakingService;
    }

    public override void Configure()
    {
        Delete("/session/game");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = this.GetUserId();

        Logger.LogInformation("User {UserId} cancelled the game request.", userId);

        _matchmakingService.Cancel(userId);
        await Send.OkAsync();
    }
}