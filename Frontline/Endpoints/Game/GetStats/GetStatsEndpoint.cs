using FastEndpoints;
using Frontline.Battle;
using Frontline.Xmpp;

namespace Frontline.Endpoints.Game.GetStats;

public class GetStatsEndpoint : EndpointWithoutRequest<GetStatsResponse>
{
    private readonly IXmppServer _xmppServer;

    private readonly IBattleService _battleService;

    public GetStatsEndpoint(IXmppServer xmppServer, IBattleService battleService)
    {
        _xmppServer = xmppServer;
        _battleService = battleService;
    }

    public override void Configure()
    {
        Get("stats.json");
        AllowAnonymous();
        Options(b =>
        {
            b.CacheOutput(p => p.Expire(TimeSpan.FromSeconds(10)));
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new GetStatsResponse
        {
            OnlinePlayers = _xmppServer.GetClientCount(),
            ActiveBattles = _battleService.GetBattleCount()
        };

        await Send.OkAsync(response);
    }
}