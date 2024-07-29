using System.Text.Json.Serialization;
using Frontline.Features.Session.Inventory.GetInventory;
using Frontline.Missions;

namespace Frontline.Features.Missions;

public class MissionKey
{
    [JsonPropertyName("Region")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PveRegion Region { get; set; }
    [JsonPropertyName("MyFaction")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Faction Faction { get; set; }
    [JsonPropertyName("MissionID")]
    public int MissionId { get; set; }
}

public class MissionStageStatus
{
    public PveRegion Region { get; set; }
    public Faction Faction { get; set; }
    public int MissionId { get; set; }
    public MissionStageState CurrentState { get; set; }
    public string MissionStageStart { get; set; }
    public int Card0TemplateId { get; set; }
    public int Card0InstanceId { get; set; }
    public CardSlotState Card0Success { get; set; }
    public string Card0Reward0 { get; set; }
    public string Card0Reward1 { get; set; }
    public string Card0Reward2 { get; set; }
    public string Card0Reward3 { get; set; }
    public string Card0Reward4 { get; set; }
    public CardState Card0State { get; set; }
    public int Card1TemplateId { get; set; }
    public int Card1InstanceId { get; set; }
    public CardSlotState Card1Success { get; set; }
    public string Card1Reward0 { get; set; }
    public string Card1Reward1 { get; set; }
    public string Card1Reward2 { get; set; }
    public string Card1Reward3 { get; set; }
    public string Card1Reward4 { get; set; }
    public CardState Card1State { get; set; }
    public int Card2TemplateId { get; set; }
    public int Card2InstanceId { get; set; }
    public CardSlotState Card2Success { get; set; }
    public string Card2Reward0 { get; set; }
    public string Card2Reward1 { get; set; }
    public string Card2Reward2 { get; set; }
    public string Card2Reward3 { get; set; }
    public string Card2Reward4 { get; set; }
    public CardState Card2State { get; set; }
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

public enum CardSlotState
{
    Open = 0,
    Success = 2,
    Fail = 3
}