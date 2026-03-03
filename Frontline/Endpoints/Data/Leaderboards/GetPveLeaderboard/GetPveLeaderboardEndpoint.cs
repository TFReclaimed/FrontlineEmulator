using FastEndpoints;

namespace Frontline.Endpoints.Data.Leaderboards.GetPveLeaderboard;

public class GetPveLeaderboardEndpoint : EndpointWithoutRequest<LeaderboardPveResponse>
{
    public override void Configure()
    {
        Get("pve/leaderboard.json");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // TODO: I have no idea how you're supposed to access this.
        // The LeaderboardsUI class does have a TabController with multiple tabs connected to it,
        // but they're not visible in the UI.
        var response = new LeaderboardPveResponse
        {
            TournamentName = "New time",
            BeginDate = new DateTime(2016, 8, 3),
            EndDate = new DateTime(2016, 8, 10),
            Entries =
            [
                new LeaderboardPveEntry
                {
                    Territory = Territory.Harmony,
                    Faction = LeaderboardFaction.Imc,
                    Rank = 20
                },
                new LeaderboardPveEntry
                {
                    Territory = Territory.Harmony,
                    Faction = LeaderboardFaction.Militia,
                    Rank = 40
                },
                new LeaderboardPveEntry
                {
                    Territory = Territory.Kraken,
                    Faction = LeaderboardFaction.Imc,
                    Rank = 3
                },
                new LeaderboardPveEntry
                {
                    Territory = Territory.Kraken,
                    Faction = LeaderboardFaction.Militia,
                    Rank = 45
                }
            ]
        };
        
        await Send.OkAsync(response);
    }
}