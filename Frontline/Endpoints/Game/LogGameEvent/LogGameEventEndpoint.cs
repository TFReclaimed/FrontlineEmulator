using FastEndpoints;
using Frontline.Battle;
using Frontline.Extensions;

namespace Frontline.Endpoints.Game.LogGameEvent;

public class LogGameEventEndpoint : Endpoint<LogGameEventRequest>
{
    private readonly IBattleService _battleService;

    public LogGameEventEndpoint(IBattleService battleService)
    {
        _battleService = battleService;
    }

    public override void Configure()
    {
        Post("/gameserver/event/{GameId}");
        AllowFormData(true);
    }

    public override async Task HandleAsync(LogGameEventRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();

        var game = _battleService.GetBattle(req.GameId);
        if (game is null || !game.IsPlayerInGame(userId))
        {
            await Send.NotFoundAsync();
            return;
        }

        game.PlayGameEvent(req.Param);
        await Send.OkAsync();
    }
}