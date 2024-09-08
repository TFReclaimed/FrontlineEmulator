using FastEndpoints;
using Frontline.Extensions;
using Frontline.Game;
using Frontline.Services;

namespace Frontline.Features.Game.GetGameEvents;

public class Endpoint : Endpoint<GameEventsRequest, GameEventsResponse>
{
    private readonly IGameService _gameService;

    public Endpoint(IGameService gameService)
    {
        _gameService = gameService;
    }

    public override void Configure()
    {
        Get("/gameserver/event/{GameId}");
    }

    public override async Task HandleAsync(GameEventsRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        
        var game = _gameService.GetGame(req.GameId);
        if (game is null || !game.IsPlayerInGame(userId))
        {
            await SendNotFoundAsync();
            return;
        }

        var events = new List<GameEventParams>();
        if (req.Param.StartIndex < game.GameEvents.Count)
        {
            events = game.GameEvents.Skip(req.Param.StartIndex).ToList();
        }
        
        var response = new GameEventsResponse
        {
            Events = events
        };

        await SendAsync(response);
    }
}