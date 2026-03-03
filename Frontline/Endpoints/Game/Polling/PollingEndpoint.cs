using FastEndpoints;
using Frontline.Battle;
using Frontline.Extensions;
using Microsoft.AspNetCore.HttpLogging;

namespace Frontline.Endpoints.Game.Polling;

public class PollingEndpoint : EndpointWithoutRequest<PollingResponse>
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

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = this.GetUserId();

        var gameId = Route<Guid>("GameId");
        var battle = _battleService.GetBattle(gameId);
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