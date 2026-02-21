using FastEndpoints;
using Frontline.Battle;
using Frontline.Battle.GameEvents;
using Frontline.Extensions;

namespace Frontline.Endpoints.Game.GetGameEvents;

public class GetGameEventsEndpoint : Endpoint<GetGameEventsRequest, GetGameEventsResponse>
{
    private readonly IBattleService _battleService;

    public GetGameEventsEndpoint(IBattleService battleService)
    {
        _battleService = battleService;
    }

    public override void Configure()
    {
        Get("/gameserver/event/{GameId}");
    }

    public override async Task HandleAsync(GetGameEventsRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();

        var game = _battleService.GetBattle(req.GameId);
        if (game is null || !game.IsPlayerInGame(userId))
        {
            await Send.NotFoundAsync();
            return;
        }

        var events = new List<GameEventParams>();
        /*if (req.Param.StartIndex < game.GameEvents.Count)
        {
            events = game.GameEvents.Skip(req.Param.StartIndex).ToList();
        }*/

        var response = new GetGameEventsResponse
        {
            Events = events
        };

        await Send.OkAsync(response);
    }
}