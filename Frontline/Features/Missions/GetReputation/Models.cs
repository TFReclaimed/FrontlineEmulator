using Frontline.Missions;

namespace Frontline.Features.Missions.GetReputation;

public class ReputationInfo
{
    public Faction Faction { get; set; }
    public PveRegion Region { get; set; }
    public int Reputation { get; set; }
}