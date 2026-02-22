using FastEndpoints;
using Frontline.Battle;
using Frontline.Battle.Matchmaking;
using Frontline.Extensions;

namespace Frontline.Endpoints.Session.Game.RequestGame;

public class RequestGameEndpoint : Endpoint<RequestGameRequest>
{
    private readonly IMatchmakingService _matchmakingService;

    private readonly IBattleService _battleService;

    public RequestGameEndpoint(IMatchmakingService matchmakingService, IBattleService battleService)
    {
        _matchmakingService = matchmakingService;
        _battleService = battleService;
    }

    public override void Configure()
    {
        Post("/session/game");
        AllowFormData(true);
    }

    public override async Task HandleAsync(RequestGameRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();

        if (_battleService.IsPlayerInGame(userId, out var game) && !game.GameState.IsGameOver())
        {
            Logger.LogWarning("User {UserId} requested a game but is already in one.", userId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        Logger.LogInformation("User {UserId} requested a game of {GameType} against {OpponentId}.",
            userId, req.Param.GameType, req.Param.OpponentId);

        _matchmakingService.Enqueue(userId, req.Param.GameType, req.Param.OpponentId);
        await Send.OkAsync();
    }
}