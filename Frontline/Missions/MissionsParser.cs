using System.Text.Json;
using Frontline.Missions.Json;

namespace Frontline.Missions;

public static class MissionsParser
{
    public static MissionsData? Data { get; private set; }
    
    public static void Initialize()
    {
        var pvePath = Path.Combine(AppContext.BaseDirectory, "PvEData.json");
        var json = File.ReadAllText(pvePath);

        var options = new JsonSerializerOptions();
        options.Converters.Add(new StringIntConverter());
        options.Converters.Add(new StringFloatConverter());
        options.Converters.Add(new StringBooleanConverter());
        options.Converters.Add(new FactionConverter());
        options.Converters.Add(new PveRegionConverter());
        options.Converters.Add(new CardRarityConverter());
        options.Converters.Add(new ConditionalOperatorConverter());
        options.Converters.Add(new ConditionalConjunctionConverter());

        Data = JsonSerializer.Deserialize<MissionsData>(json, options);
    }
    
    public static string GetMissionKey(PveRegion region, Faction faction, int missionId)
    {
        var factionStr = faction switch
        {
            Faction.IMC => "IMC",
            Faction.Militia => "MIL",
            Faction.Neutral => "NEU",
            _ => "NEU"
        };
        
        return $"{region}-{factionStr}-{missionId:D4}";
    }

    public static string GetMissionKey(MissionStage mission)
    {
        return GetMissionKey(mission.Region, mission.Faction, mission.MissionId);
    }

    public static MissionStage? GetMission(string key)
    {
        return Data?.MissionStages[key];
    }
    
    public static MissionSlots? GetMissionSlots(string name)
    {
        return Data?.Slots[name];
    }
    
    public static MissionRewardSet? GetRewardSet(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        
        return Data?.RewardSets[name];
    }

    public static MissionRewardSet? GetBonusRewardSet(string bonusRequirement)
    {
        if (string.IsNullOrWhiteSpace(bonusRequirement))
        {
            return null;
        }
        
        var slots = GetMissionSlots(bonusRequirement);
        if (slots is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(slots.BonusSuccessReward))
        {
            return null;
        }
        
        return GetRewardSet(slots.BonusSuccessReward);
    }

    public static MissionReward? GetReward(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        
        return Data?.Rewards[name];
    }

    public static MissionConditional? GetConditional(string name)
    {
        return Data?.Conditionals[name];
    }
    
    public static MissionNameMapping? GetMissionNameMapping(string name)
    {
        return Data?.NameMap[name];
    }
}