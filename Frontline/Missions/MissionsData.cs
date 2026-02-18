using System.Text.Json.Serialization;
using Frontline.Game;
using Frontline.Missions.Json;

namespace Frontline.Missions;

public class MissionsData
{
    [JsonPropertyName("DT_MissionStage")]
    public required Dictionary<string, MissionStage> MissionStages { get; set; }
    [JsonPropertyName("DT_MissionSet")]
    public required Dictionary<string, MissionSet> MissionSets { get; set; }
    [JsonPropertyName("DT_Slots")]
    public required Dictionary<string, MissionSlot> Slots { get; set; }
    [JsonPropertyName("DT_SlotXP")]
    public required Dictionary<string, MissionSlotXp> SlotXp { get; set; }
    [JsonPropertyName("DT_RewardSets")]
    public required Dictionary<string, MissionRewardSet> RewardSets { get; set; }
    [JsonPropertyName("DT_Rewards")]
    public required Dictionary<string, MissionReward> Rewards { get; set; }
    [JsonPropertyName("DT_Synergies")]
    public required Dictionary<string, MissionSynergy> Synergies { get; set; }
    [JsonPropertyName("DT_Intel")]
    public required Dictionary<string, MissionIntel> Intel { get; set; }
    [JsonPropertyName("DT_Regions")]
    public required Dictionary<string, MissionRegion> Regions { get; set; }
    [JsonPropertyName("DT_SubRegions")]
    public required Dictionary<string, MissionSubRegion> SubRegions { get; set; }
    [JsonPropertyName("DT_Conditionals")]
    public required Dictionary<string, MissionConditional> Conditionals { get; set; }
    [JsonPropertyName("DT_Reputation")]
    public required Dictionary<string, MissionReputation> Reputation { get; set; }
    [JsonPropertyName("DT_NameMap")]
    public required Dictionary<string, MissionNameMapping> NameMap { get; set; }
}

public class MissionStage
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("MissionID:I")]
    public int MissionId { get; set; }
    [JsonPropertyName("Req1:S")]
    public string Requirement1 { get; set; } = string.Empty;
    [JsonPropertyName("Req2:S")]
    public string Requirement2 { get; set; } = string.Empty;
    [JsonPropertyName("Req3:S")]
    public string Requirement3 { get; set; } = string.Empty;
    [JsonPropertyName("Req4:S")]
    public string Requirement4 { get; set; } = string.Empty;
    [JsonPropertyName("Conj:S")]
    public string RequirementConjunction { get; set; } = string.Empty; // TODO: make into enum
    [JsonPropertyName("NotReq:S")]
    public string NotReq { get; set; } = string.Empty;
    [JsonPropertyName("Region:S")]
    public PveRegion Region { get; set; }
    [JsonPropertyName("SubRegion:S")]
    public string SubRegion { get; set; } = string.Empty;
    [JsonPropertyName("Faction:S")]
    public Faction Faction { get; set; }
    [JsonPropertyName("Guild:B")]
    public bool IsGuild { get; set; }
    [JsonPropertyName("Set:X")]
    public string MissionSet { get; set; } = string.Empty;
    [JsonPropertyName("Type:S")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MissionType MissionType { get; set; }
    [JsonPropertyName("Weight:F")]
    public float ElectiveSelectionWeight { get; set; }
    [JsonPropertyName("Cooldown:I=72000")]
    [StringIntConverter(72000)]
    public int Cooldown { get; set; }
    [JsonPropertyName("RepType:X")]
    public string ReputationType { get; set; } = string.Empty;
    [JsonPropertyName("RepPct:X")]
    public string ReputationPercentile { get; set; } = string.Empty;
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
    public string SuccessReward { get; set; } = string.Empty;
    [JsonPropertyName("NumReqSlots:I=1")]
    [StringIntConverter(1)]
    public int RequiredSlotCount { get; set; }
    [JsonPropertyName("ReqSlot:X")]
    public string RequiredSlotCondition { get; set; } = string.Empty;
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
    // Yes, this is a typo in the original data.
    [JsonPropertyName("ReqMaxRank:I")]
    public int RequiredSlotMinRank { get; set; }
    [JsonPropertyName("Bonus1:X")]
    public string Bonus1SlotCondition { get; set; } = string.Empty;
    [JsonPropertyName("B1Consume:B")]
    public bool Bonus1SlotConsume { get; set; }
    [JsonPropertyName("B1MinCmd:I")]
    public int Bonus1SlotMinCommand { get; set; }
    [JsonPropertyName("B1MaxCmd:I=10")]
    [StringIntConverter(10)]
    public int Bonus1SlotMaxCommand { get; set; }
    [JsonPropertyName("B1MinRarity:S")]
    public CardRarity Bonus1SlotMinRarity { get; set; }
    // Yes, this is a typo in the original data.
    [JsonPropertyName("B1MaxRank:I")]
    public int Bonus1SlotMinRank { get; set; }
    [JsonPropertyName("Bonus2:X")]
    public string Bonus2SlotCondition { get; set; } = string.Empty;
    [JsonPropertyName("B2Consume:B")]
    public bool Bonus2SlotConsume { get; set; }
    [JsonPropertyName("B2MinCmd:I")]
    public int Bonus2SlotMinCommand { get; set; }
    [JsonPropertyName("B2MaxCmd:I=10")]
    [StringIntConverter(10)]
    public int Bonus2SlotMaxCommand { get; set; }
    [JsonPropertyName("B2MinRarity:S")]
    public CardRarity Bonus2SlotMinRarity { get; set; }
    // Yes, this is a typo in the original data.
    [JsonPropertyName("B2MaxRank:I")]
    public int Bonus2SlotMinRank { get; set; }
    
    public bool IsCalculated => MissionType is MissionType.Elective or MissionType.Once;
    
    public VisibilityRarity VisibilityRarity
    {
        get
        {
            if (!IsCalculated)
            {
                return VisibilityRarity.None;
            }

            return ElectiveSelectionWeight switch
            {
                -1f or >= 1f => VisibilityRarity.Common,
                >= 0.1f => VisibilityRarity.Uncommon,
                >= 0.01f => VisibilityRarity.Rare,
                _ => VisibilityRarity.VeryRare
            };
        }
    }
}

public enum MissionType
{
    Progression,
    Elective,
    Persistent,
    Once
}

public enum VisibilityRarity
{
    None = -1,
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    VeryRare = 3
}

public class MissionSet
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("Period:I")]
    public int Period { get; set; }
    [JsonPropertyName("ElectiveCount:I")]
    public int ElectiveCount { get; set; }
}

public class MissionSlot
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; } = string.Empty;
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
    public string BonusSuccessReward { get; set; } = string.Empty;
}

public class MissionSlotXp
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; } = string.Empty;
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
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("Type:S")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RewardSetType Type { get; set; }
    [JsonPropertyName("Reward1:X")]
    public string Reward1 { get; set; } = string.Empty;
    [JsonPropertyName("Reward2:X")]
    public string Reward2 { get; set; } = string.Empty;
    [JsonPropertyName("Reward3:X")]
    public string Reward3 { get; set; } = string.Empty;
    [JsonPropertyName("Reward4:X")]
    public string Reward4 { get; set; } = string.Empty;
    [JsonPropertyName("Reward5:X")]
    public string Reward5 { get; set; } = string.Empty;
}

public enum RewardSetType
{
    All,
    Pick
}

public class MissionReward
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("Element:S")]
    public string Element { get; set; } = string.Empty;
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
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("DisplayName:S")]
    public string DisplayName { get; set; } = string.Empty;
    [JsonPropertyName("Grouping:S")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MissionSynergyGrouping Grouping { get; set; }
    [JsonPropertyName("Effect:X")]
    public string Effect { get; set; } = string.Empty;
    [JsonPropertyName("Reward:X")]
    public string Reward { get; set; } = string.Empty;
}

public enum MissionSynergyGrouping
{
    One,
    All
}

public class MissionIntel
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("DisplayName:S")]
    public string DisplayName { get; set; } = string.Empty;
}

public class MissionRegion
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("DisplayName:S")]
    public string DisplayName { get; set; } = string.Empty;
    [JsonPropertyName("Locked:B")]
    public bool Locked { get; set; }
    [JsonPropertyName("ShowEmpty:B")]
    public bool ShowEmpty { get; set; }
}

public class MissionSubRegion
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("Name:S")]
    public string Name { get; set; } = string.Empty;
}

public class MissionConditional
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("RefId:S")]
    public string RefId { get; set; } = string.Empty;
    [JsonPropertyName("NameId:X")]
    public string NameId { get; set; } = string.Empty;
    [JsonPropertyName("GroupPriority:F")]
    public float GroupPriority { get; set; }
    [JsonPropertyName("Attribute:S")]
    public ConditionalAttribute Attribute { get; set; }
    [JsonPropertyName("Operator:S")]
    public Operator Operator { get; set; }
    [JsonPropertyName("Comparison:S")]
    public string Comparison { get; set; } = string.Empty;
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
    public string Id { get; set; } = string.Empty;
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
    public string Tier1Reward { get; set; } = string.Empty;
    [JsonPropertyName("Tier2Pct:F")]
    public float Tier2Pct { get; set; }
    [JsonPropertyName("Tier2Success:F")]
    public float Tier2Success { get; set; }
    [JsonPropertyName("Tier2BonusSuccess:F")]
    public float Tier2BonusSuccess { get; set; }
    [JsonPropertyName("Tier2Reward:X")]
    public string Tier2Reward { get; set; } = string.Empty;
    [JsonPropertyName("Tier3Pct:F")]
    public float Tier3Pct { get; set; }
    [JsonPropertyName("Tier3Success:F")]
    public float Tier3Success { get; set; }
    [JsonPropertyName("Tier3BonusSuccess:F")]
    public float Tier3BonusSuccess { get; set; }
    [JsonPropertyName("Tier3Reward:X")]
    public string Tier3Reward { get; set; } = string.Empty;
    [JsonPropertyName("Tier4Pct:F")]
    public float Tier4Pct { get; set; }
    [JsonPropertyName("Tier4Success:F")]
    public float Tier4Success { get; set; }
    [JsonPropertyName("Tier4BonusSuccess:F")]
    public float Tier4BonusSuccess { get; set; }
    [JsonPropertyName("Tier4Reward:X")]
    public string Tier4Reward { get; set; } = string.Empty;
    [JsonPropertyName("Tier5Pct:F")]
    public float Tier5Pct { get; set; }
    [JsonPropertyName("Tier5Success:F")]
    public float Tier5Success { get; set; }
    [JsonPropertyName("Tier5BonusSuccess:F")]
    public float Tier5BonusSuccess { get; set; }
    [JsonPropertyName("Tier5Reward:X")]
    public string Tier5Reward { get; set; } = string.Empty;
}

public enum Faction
{
    Neutral,
    IMC,
    Militia,
    NumFactions
}

public enum PveRegion
{
    Demeter,
    Swampland,
    DeepSpace,
    Harmony,
    Kraken,
    Coliseum,
    Badlands,
    Epic,
    Ops,
    TEST,
    NumRegions
}

public class MissionNameMapping
{
    [JsonPropertyName("ID:S")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("TemplateId:I")]
    public int TemplateId { get; set; }
}