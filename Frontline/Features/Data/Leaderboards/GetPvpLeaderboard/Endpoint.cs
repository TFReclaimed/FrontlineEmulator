using FastEndpoints;
using Frontline.Data.Repositories;

namespace Frontline.Features.Data.Leaderboards.GetPvpLeaderboard;

public class Endpoint : EndpointWithoutRequest<LeaderboardPvpResponse, Mapper>
{
    private readonly IPlayerRepository _playerRepository;

    public Endpoint(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public override void Configure()
    {
        Get("pvp/leaderboard.json");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var players = await _playerRepository.GetTopPlayersAsync(50);
        
        var response = new LeaderboardPvpResponse
        {
            TournamentName = "Some tournament",
            BeginDate = new DateTime(2016, 8, 3),
            EndDate = new DateTime(2016, 8, 10),
            Entries = Map.FromEntity(players)
        };
        
        await SendAsync(response);
    }
}