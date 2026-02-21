using FastEndpoints;
using Frontline.Battle;
using Frontline.Extensions;

namespace Frontline.Endpoints.Game.GetGame;

public class GetGameEndpoint : Endpoint<GetGameRequest, CcgGame>
{
    private readonly IBattleService _battleService;

    public GetGameEndpoint(IBattleService battleService)
    {
        _battleService = battleService;
    }

    public override void Configure()
    {
        Get("/gameserver/{GameId}");
    }

    public override async Task HandleAsync(GetGameRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();

        var game = _battleService.GetBattle(req.GameId);
        if (game is null || !game.IsPlayerInGame(userId))
        {
            await Send.NotFoundAsync();
            return;
        }

        await Send.OkAsync(game);
    }
}