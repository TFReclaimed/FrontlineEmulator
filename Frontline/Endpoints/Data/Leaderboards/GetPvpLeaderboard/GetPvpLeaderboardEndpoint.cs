using FastEndpoints;
using Frontline.Data.Repositories;

namespace Frontline.Endpoints.Data.Leaderboards.GetPvpLeaderboard;

public class GetPvpLeaderboardEndpoint : EndpointWithoutRequest<LeaderboardPvpResponse>
{
    private readonly IPlayerRepository _playerRepository;

    public GetPvpLeaderboardEndpoint(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public override void Configure()
    {
        Get("pvp/leaderboard.json");
        AllowAnonymous();
        Options(b =>
        {
            b.CacheOutput(p => p.Expire(TimeSpan.FromMinutes(1)));
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var players = await _playerRepository.GetTopPlayersAsync(50);

        var response = new LeaderboardPvpResponse
        {
            TournamentName = "Some tournament",
            BeginDate = new DateTime(2016, 8, 3),
            EndDate = new DateTime(2016, 8, 10),
            Entries = players.Select(LeaderboardPvpEntry.FromEntity).ToList()
        };

        await Send.OkAsync(response);
    }
}