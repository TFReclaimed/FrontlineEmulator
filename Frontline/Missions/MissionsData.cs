using System.Text.Json.Serialization;
using Frontline.Game;
using Frontline.Missions.Json;

namespace Frontline.Missions;

public class MissionsData
{
    [JsonPropertyName("DT_MissionStage")]
    public Dictionary<string, MissionStage> MissionStages { get; set; }
    [JsonPropertyName("DT_MissionSet")]
    public Dictionary<string, MissionSet> MissionSets { get; set; }
    [JsonPropertyName("DT_Slots")]
    public Dictionary<string, MissionSlot> Slots { get; set; }
    [JsonPropertyName("DT_SlotXP")]
    public Dictionary<string, MissionSlotXp> SlotXp { get; set; }
    [JsonPropertyName("DT_RewardSets")]
    public Dictionary<string, MissionRewardSet> RewardSets { get; set; }
    [JsonPropertyName("DT_Rewards")]
    public Dictionary<string, MissionReward> Rewards { get; set; }
    [JsonPropertyName("DT_Synergies")]
    public Dictionary<string, MissionSynergy> Synergies { get; set; }
    [JsonPropertyName("DT_Intel")]
    public Dictionary<string, MissionIntel> Intel { get; set; }
    [JsonPropertyName("DT_Regions")]
    public Dictionary<string, MissionRegion> Regions { get; set; }
    [JsonPropertyName("DT_SubRegions")]
    public Dictionary<string, MissionSubRegion> SubRegions { get; set; }
    [JsonPropertyName("DT_Conditionals")]
    public Dictionary<string, MissionConditional> Conditionals { get; set; }
    [JsonPropertyName("DT_Reputation")]
    public Dictionary<string, MissionReputation> Reputation { get; set; }
    [JsonPropertyName("DT_NameMap")]
    public Dictionary<string, MissionNameMapping> NameMap { get; set; }
}

public class MissionStage
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("MissionID:I")]
    public int MissionId { get; set; }
    [JsonPropertyName("Req1:S")]
    public string Requirement1 { get; set; }
    [JsonPropertyName("Req2:S")]
    public string Requirement2 { get; set; }
    [JsonPropertyName("Req3:S")]
    public string Requirement3 { get; set; }
    [JsonPropertyName("Req4:S")]
    public string Requirement4 { get; set; }
    [JsonPropertyName("Conj:S")]
    public string RequirementConjunction { get; set; } // TODO: make into enum
    [JsonPropertyName("NotReq:S")]
    public string NotReq { get; set; }
    [JsonPropertyName("Region:S")]
    public PveRegion Region { get; set; }
    [JsonPropertyName("SubRegion:S")]
    public string SubRegion { get; set; }
    [JsonPropertyName("Faction:S")]
    public Faction Faction { get; set; }
    [JsonPropertyName("Guild:B")]
    public bool IsGuild { get; set; }
    [JsonPropertyName("Set:X")]
    public string MissionSet { get; set; }
    [JsonPropertyName("Type:S")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MissionType MissionType { get; set; }
    [JsonPropertyName("Weight:F")]
    public float ElectiveSelectionWeight { get; set; }
    [JsonPropertyName("Cooldown:I=72000")]
    [StringIntConverter(72000)]
    public int Cooldown { get; set; }
    [JsonPropertyName("RepType:X")]
    public string ReputationType { get; set; }
    [JsonPropertyName("RepPct:X")]
    public string ReputationPercentile { get; set; }
    [JsonPropertyName("Supply:I")]
    public int SupplyCost { get; set; }
    [JsonPropertyName("Credit:I")]
    public int CreditCost { get; set; }
    [JsonPropertyName("Token:I")]
    public int TokenCost { get; set; }
    [JsonPropertyName("Duration:I")]
    public int Duration { get; set; }
    [JsonPropertyName("SuccessChance:F=0.7")]
    [StringFloatConverter(0.7f)]
    public float SuccessChance { get; set; }
    [JsonPropertyName("SuccessReward:X")]
    public string SuccessReward { get; set; }
    [JsonPropertyName("NumReqSlots:I=1")]
    [StringIntConverter(1)]
    public int RequiredSlotCount { get; set; }
    [JsonPropertyName("ReqSlot:X")]
    public string RequiredSlotCondition { get; set; }
    [JsonPropertyName("ReqConsume:B")]
    public bool RequiredSlotConsume { get; set; }
    [JsonPropertyName("ReqMinCmd:I")]
    public int RequiredSlotMinCommand { get; set; }
    [JsonPropertyName("ReqMaxCmd:I=10")]
    [StringIntConverter(10)]
    public int RequiredSlotMaxCommand { get; set; }
    [JsonPropertyName("ReqMinRarity:S")]
    public CardRarity RequiredSlotMinRarity { get; set; }
    [JsonPropertyName("ReqMaxRarity:S")]
    public CardRarity RequiredSlotMaxRarity { get; set; }
    [JsonPropertyName("ReqMinRank:I")]
    public int RequiredSlotMinRank { get; set; }
    [JsonPropertyName("Bonus1:X")]
    public string Bonus1SlotCondition { get; set; }
    [JsonPropertyName("B1Consume:B")]
    public bool Bonus1SlotConsume { get; set; }
    [JsonPropertyName("B1MinCmd:I")]
    public int Bonus1SlotMinCommand { get; set; }
    [JsonPropertyName("B1MaxCmd:I=10")]
    [StringIntConverter(10)]
    public int Bonus1SlotMaxCommand { get; set; }
    [JsonPropertyName("B1MinRarity:S")]
    public CardRarity Bonus1SlotMinRarity { get; set; }
    [JsonPropertyName("B1MinRank:I")]
    public int Bonus1SlotMinRank { get; set; }
    [JsonPropertyName("Bonus2:X")]
    public string Bonus2SlotCondition { get; set; }
    [JsonPropertyName("B2Consume:B")]
    public bool Bonus2SlotConsume { get; set; }
    [JsonPropertyName("B2MinCmd:I")]
    public int Bonus2SlotMinCommand { get; set; }
    [JsonPropertyName("B2MaxCmd:I=10")]
    [StringIntConverter(10)]
    public int Bonus2SlotMaxCommand { get; set; }
    [JsonPropertyName("B2MinRarity:S")]
    public CardRarity Bonus2SlotMinRarity { get; set; }
    [JsonPropertyName("B2MinRank:I")]
    public int Bonus2SlotMinRank { get; set; }
}

public enum MissionType
{
    Progression = 0,
    Elective = 1,
    Persistent = 2,
    Once = 3
}

public class MissionSet
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("Period:I")]
    public int Period { get; set; }
    [JsonPropertyName("ElectiveCount:I")]
    public int ElectiveCount { get; set; }
}

public class MissionSlot
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("ReqCasualtyOverride:F=-1")]
    [StringFloatConverter(-1f)]
    public float ReqCasualtyOverride { get; set; }
    [JsonPropertyName("BonusCasualtyOverride:F=-1")]
    [StringFloatConverter(-1f)]
    public float BonusCasualtyOverride { get; set; }
    [JsonPropertyName("BonusSuccessOverride:F=-1")]
    [StringFloatConverter(-1f)]
    public float BonusSuccessOverride { get; set; }
    [JsonPropertyName("BonusSuccessReward:X")]
    public string BonusSuccessReward { get; set; }
}

public class MissionSlotXp
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("Base:F")]
    public float Base { get; set; }
    [JsonPropertyName("Uncommon:F")]
    public float Uncommon { get; set; }
    [JsonPropertyName("Rare:F")]
    public float Rare { get; set; }
    [JsonPropertyName("VeryRare:F")]
    public float VeryRare { get; set; }
}

public class MissionRewardSet
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("Type:S")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RewardSetType Type { get; set; }
    [JsonPropertyName("Reward1:X")]
    public string Reward1 { get; set; }
    [JsonPropertyName("Reward2:X")]
    public string Reward2 { get; set; }
    [JsonPropertyName("Reward3:X")]
    public string Reward3 { get; set; }
    [JsonPropertyName("Reward4:X")]
    public string Reward4 { get; set; }
    [JsonPropertyName("Reward5:X")]
    public string Reward5 { get; set; }
}

public enum RewardSetType
{
    All = 0,
    Pick = 1
}

public class MissionReward
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("Element:S")]
    public string Element { get; set; }
    [JsonPropertyName("Qty:I")]
    public int Quantity { get; set; }
    [JsonPropertyName("Once:B")]
    public bool Once { get; set; }
    [JsonPropertyName("Weight:F")]
    public float Weight { get; set; }
}

public class MissionSynergy
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("DisplayName:S")]
    public string DisplayName { get; set; }
    [JsonPropertyName("Grouping:S")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MissionSynergyGrouping Grouping { get; set; }
    [JsonPropertyName("Effect:X")]
    public string Effect { get; set; }
    [JsonPropertyName("Reward:X")]
    public string Reward { get; set; }
}

public enum MissionSynergyGrouping
{
    One = 0,
    All = 1
}

public class MissionIntel
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("DisplayName:S")]
    public string DisplayName { get; set; }
}

public class MissionRegion
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("DisplayName:S")]
    public string DisplayName { get; set; }
    [JsonPropertyName("Locked:B")]
    public bool Locked { get; set; }
    [JsonPropertyName("ShowEmpty:B")]
    public bool ShowEmpty { get; set; }
}

public class MissionSubRegion
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("Name:S")]
    public string Name { get; set; }
}

public class MissionConditional
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("RefId:S")]
    public string RefId { get; set; }
    [JsonPropertyName("NameId:X")]
    public string NameId { get; set; }
    [JsonPropertyName("GroupPriority:F")]
    public float GroupPriority { get; set; }
    [JsonPropertyName("Attribute:S")]
    public ConditionalAttribute Attribute { get; set; }
    [JsonPropertyName("Operator:S")]
    public Operator Operator { get; set; }
    [JsonPropertyName("Comparison:S")]
    public string Comparison { get; set; }
    [JsonPropertyName("Conjunction:S")]
    public Conjunction Conjunction { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConditionalAttribute
{
    Invalid = -1,
    IsType = 0,
    IsUnitType = 1,
    HasTrait = 2,
    HasFlag = 3,
    IsName = 4,
    Command = 5,
    IsRarity = 6,
    Rank = 7
}

public enum Operator
{
    Invalid = -1,
    IsEqual = 0,
    IsNotEqual = 1,
    IsGreaterThan = 2,
    IsLessThan = 3,
    IsGreaterThanOrEqual = 4,
    IsLessThanOrEqual = 5
}

public enum Conjunction
{
    Invalid = -1,
    And = 0,
    Or = 1,
    None = 2
}

public class MissionReputation
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("Region:S")]
    public PveRegion Region { get; set; }
    [JsonPropertyName("Faction:S")]
    public Faction Faction { get; set; }
    [JsonPropertyName("ResetPeriod:I")]
    public int ResetPeriod { get; set; }
    [JsonPropertyName("Tier1Pct:F")]
    public float Tier1Pct { get; set; }
    [JsonPropertyName("Tier1Success:F")]
    public float Tier1Success { get; set; }
    [JsonPropertyName("Tier1BonusSuccess:F")]
    public float Tier1BonusSuccess { get; set; }
    [JsonPropertyName("Tier1Reward:X")]
    public string Tier1Reward { get; set; }
    [JsonPropertyName("Tier2Pct:F")]
    public float Tier2Pct { get; set; }
    [JsonPropertyName("Tier2Success:F")]
    public float Tier2Success { get; set; }
    [JsonPropertyName("Tier2BonusSuccess:F")]
    public float Tier2BonusSuccess { get; set; }
    [JsonPropertyName("Tier2Reward:X")]
    public string Tier2Reward { get; set; }
    [JsonPropertyName("Tier3Pct:F")]
    public float Tier3Pct { get; set; }
    [JsonPropertyName("Tier3Success:F")]
    public float Tier3Success { get; set; }
    [JsonPropertyName("Tier3BonusSuccess:F")]
    public float Tier3BonusSuccess { get; set; }
    [JsonPropertyName("Tier3Reward:X")]
    public string Tier3Reward { get; set; }
    [JsonPropertyName("Tier4Pct:F")]
    public float Tier4Pct { get; set; }
    [JsonPropertyName("Tier4Success:F")]
    public float Tier4Success { get; set; }
    [JsonPropertyName("Tier4BonusSuccess:F")]
    public float Tier4BonusSuccess { get; set; }
    [JsonPropertyName("Tier4Reward:X")]
    public string Tier4Reward { get; set; }
    [JsonPropertyName("Tier5Pct:F")]
    public float Tier5Pct { get; set; }
    [JsonPropertyName("Tier5Success:F")]
    public float Tier5Success { get; set; }
    [JsonPropertyName("Tier5BonusSuccess:F")]
    public float Tier5BonusSuccess { get; set; }
    [JsonPropertyName("Tier5Reward:X")]
    public string Tier5Reward { get; set; }
}

public enum Faction
{
    Neutral = 0,
    IMC = 1,
    Militia = 2,
    NumFactions = 3
}

public enum PveRegion
{
    Demeter = 0,
    Swampland = 1,
    DeepSpace = 2,
    Harmony = 3,
    Kraken = 4,
    Coliseum = 5,
    Badlands = 6,
    Epic = 7,
    Ops = 8,
    TEST = 9,
    NumRegions = 10
}

public class MissionNameMapping
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; }
    [JsonPropertyName("TemplateId:I")]
    public int TemplateId { get; set; }
}