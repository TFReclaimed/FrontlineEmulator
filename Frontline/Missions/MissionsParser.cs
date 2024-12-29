using System.Text.Json;
using Frontline.Features.Missions;
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
    
    public static MissionKey ParseMissionKey(string key)
    {
        var parts = key.Split('-');
        if (parts.Length != 3)
        {
            throw new ArgumentException("Invalid mission key format");
        }

        var region = Enum.Parse<PveRegion>(parts[0]);
        
        var faction = parts[1] switch
        {
            "IMC" => Faction.IMC,
            "MIL" => Faction.Militia,
            "NEU" => Faction.Neutral,
            _ => Faction.Neutral
        };
        
        var missionId = int.Parse(parts[2]);

        return new MissionKey
        {
            Region = region,
            Faction = faction,
            MissionId = missionId
        };
    }

    public static MissionStage? GetMission(string key)
    {
        return Data?.MissionStages.TryGetValue(key, out var mission) == true ? mission : null;
    }
    
    public static MissionSlot? GetMissionSlot(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        
        return Data?.Slots.TryGetValue(name, out var slot) == true ? slot : null;
    }

    public static MissionSlotXp? GetMissionSlotXp(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        
        return Data?.SlotXp.TryGetValue(name, out var slotXp) == true ? slotXp : null;
    }
    
    public static MissionRewardSet? GetRewardSet(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        
        return Data?.RewardSets.TryGetValue(name, out var rewardSet) == true ? rewardSet : null;
    }

    public static MissionRewardSet? GetBonusRewardSet(string bonusRequirement)
    {
        if (string.IsNullOrWhiteSpace(bonusRequirement))
        {
            return null;
        }
        
        var slots = GetMissionSlot(bonusRequirement);
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
        
        return Data?.Rewards.TryGetValue(name, out var reward) == true ? reward : null;
    }

    public static MissionConditional? GetConditional(string name)
    {
        return Data?.Conditionals.TryGetValue(name, out var conditional) == true ? conditional : null;
    }
    
    public static MissionNameMapping? GetMissionNameMapping(string name)
    {
        return Data?.NameMap.TryGetValue(name, out var mapping) == true ? mapping : null;
    }
}