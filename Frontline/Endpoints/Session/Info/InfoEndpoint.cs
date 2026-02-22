using FastEndpoints;
using Frontline.Battle;
using Frontline.Battle.Matchmaking;
using Frontline.Extensions;
using Frontline.Services;

namespace Frontline.Endpoints.Session.Info;

public class InfoEndpoint : EndpointWithoutRequest<SessionInfoResponse>
{
    private readonly IBattleService _battleService;

    private readonly IMatchmakingService _matchmakingService;

    private readonly IUserService _userService;

    public InfoEndpoint(IBattleService battleService, IMatchmakingService matchmakingService, IUserService userService)
    {
        _battleService = battleService;
        _matchmakingService = matchmakingService;
        _userService = userService;
    }

    public override void Configure()
    {
        Get("/session/info");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = this.GetUserId();

        const string notInGame = "0";
        const string matchmaking = "-1";

        var gameId = notInGame;
        if (_matchmakingService.IsUserInQueue(userId))
        {
            gameId = matchmaking;
        }
        else if (_battleService.IsPlayerInGame(userId, out var battle) && !battle.GameState.IsGameOver())
        {
            gameId = battle.Id.ToString();
        }

        var response = new SessionInfoResponse
        {
            CurrentGameInstance = gameId,
            UserChangeCounter = _userService.GetChangeCounter(userId)
        };

        await Send.OkAsync(response);
    }
}