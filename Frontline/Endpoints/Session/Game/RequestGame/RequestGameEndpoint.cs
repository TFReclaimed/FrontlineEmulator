using FastEndpoints;
using Frontline.Battle.Matchmaking;
using Frontline.Extensions;

namespace Frontline.Endpoints.Session.Game.RequestGame;

public class RequestGameEndpoint : Endpoint<RequestGameRequest>
{
    private readonly IMatchmakingService _matchmakingService;

    public RequestGameEndpoint(IMatchmakingService matchmakingService)
    {
        _matchmakingService = matchmakingService;
    }

    public override void Configure()
    {
        Post("/session/game");
        AllowFormData(true);
    }

    public override async Task HandleAsync(RequestGameRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();

        Logger.LogInformation("User {UserId} requested a game of {GameType} against {OpponentId}.",
            userId, req.Param.GameType, req.Param.OpponentId);

        _matchmakingService.Enqueue(userId, req.Param.GameType, req.Param.OpponentId);
        await Send.OkAsync();
    }
}