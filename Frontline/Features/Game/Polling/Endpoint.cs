using FastEndpoints;
using Frontline.Extensions;
using Frontline.Services;
using Microsoft.AspNetCore.HttpLogging;

namespace Frontline.Features.Game.Polling;

public class Endpoint : Endpoint<PollingRequest, PollingResponse>
{
    private readonly IGameService _gameService;

    public Endpoint(IGameService gameService)
    {
        _gameService = gameService;
    }

    public override void Configure()
    {
        Get("/gameserver/polling/{GameId}");
        Options(b =>
        {
            b.WithHttpLogging(HttpLoggingFields.None);
        });
    }

    public override async Task HandleAsync(PollingRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        
        var game = _gameService.GetGame(req.GameId);
        if (game is null || !game.IsPlayerInGame(userId))
        {
            await SendNotFoundAsync();
            return;
        }
        
        var response = new PollingResponse
        {
            ChangeCounter = game.GameChangeCounter
        };

        await SendAsync(response);
    }
}