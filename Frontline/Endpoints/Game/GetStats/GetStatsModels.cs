namespace Frontline.Endpoints.Game.GetStats;

public class GetStatsResponse
{
    public int OnlinePlayers { get; set; }
    public int PvpBattles { get; set; }
    public int AiBattles { get; set; }
}