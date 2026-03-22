using Frontline.Battle.Data.Card;
using Frontline.Missions;

namespace Frontline.Endpoints.Missions.GetReputation;

public class ReputationInfo
{
    public CardFaction Faction { get; set; }
    public PveRegion Region { get; set; }
    public int Reputation { get; set; }
}