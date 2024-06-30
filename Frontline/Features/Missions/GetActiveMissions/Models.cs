namespace Frontline.Features.Missions.GetActiveMissions;

public class MissionStageStatus
{
    public PveRegion Region { get; set; }
    public Faction Faction { get; set; }
    public int MissionId { get; set; }
    public MissionStageState CurrentState { get; set; }
}

public enum MissionStageState
{
    Unavailable = 0,
    Available = 1,
    InProgress = 2,
    Finished = 3,
    Rewarded = 4,
    Finalized = 5
}