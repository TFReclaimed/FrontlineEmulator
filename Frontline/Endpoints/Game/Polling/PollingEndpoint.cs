using FastEndpoints;
using Frontline.Battle;
using Frontline.Extensions;
using Microsoft.AspNetCore.HttpLogging;

namespace Frontline.Endpoints.Game.Polling;

public class PollingEndpoint : Endpoint<PollingRequest, PollingResponse>
{
    private readonly IBattleService _battleService;

    public PollingEndpoint(IBattleService battleService)
    {
        _battleService = battleService;
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

        var battle = _battleService.GetBattle(req.GameId);
        if (battle is null || !battle.IsPlayerInGame(userId))
        {
            await Send.NotFoundAsync();
            return;
        }

        var response = new PollingResponse
        {
            ChangeCounter = battle.GameChangeCounter
        };

        await Send.OkAsync(response);
    }
}