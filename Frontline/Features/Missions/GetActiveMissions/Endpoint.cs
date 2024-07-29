using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Features.Session.Inventory.GetInventory;
using Frontline.Missions;

namespace Frontline.Features.Missions.GetActiveMissions;

public class Endpoint : EndpointWithoutRequest<List<MissionStageStatus>>
{
    private readonly IMissionRepository _missionRepository;

    public Endpoint(IMissionRepository missionRepository)
    {
        _missionRepository = missionRepository;
    }

    public override void Configure()
    {
        Get("/Missions/v1/activemissions");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = int.Parse(User.ClaimValue("UserId")!);

        var activeMissions = await _missionRepository.GetActiveMissionsAsync(userId);
        var finishedMissions = await _missionRepository.GetFinishedMissionsAsync(userId);
        
        var response = new List<MissionStageStatus>();
        
        foreach (var mission in MissionsParser.Data!.MissionStages.Values)
        {
            if (!AreRequirementsMet(mission, activeMissions, finishedMissions, out var missionEntity))
            {
                continue;
            }

            if (missionEntity is null)
            {
                response.Add(new MissionStageStatus
                {
                    Region = mission.Region,
                    Faction = mission.Faction,
                    MissionId = mission.MissionId,
                    CurrentState = MissionStageState.Available
                });
            }
            else
            {
                var requiredRewardSet = MissionsParser.GetRewardSet(mission.SuccessReward);
                var bonus1RewardSet = MissionsParser.GetBonusRewardSet(mission.Bonus1SlotCondition);
                var bonus2RewardSet = MissionsParser.GetBonusRewardSet(mission.Bonus2SlotCondition);
                
                response.Add(new MissionStageStatus
                {
                    Region = mission.Region,
                    Faction = mission.Faction,
                    MissionId = mission.MissionId,
                    CurrentState = IsMissionInProgress(missionEntity)
                        ? MissionStageState.InProgress
                        : MissionStageState.Finished,
                    MissionStageStart = missionEntity.Start.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    Card0TemplateId = missionEntity.RequiredCardItem?.TemplateId ?? 0,
                    Card0InstanceId = missionEntity.RequiredCardItem?.ItemId ?? 0,
                    Card0Success = missionEntity.Success ? CardSlotState.Success : CardSlotState.Fail,
                    Card0Reward0 = requiredRewardSet != null && missionEntity.Success ? requiredRewardSet.Reward1 : "",
                    Card0Reward1 = requiredRewardSet != null && missionEntity.Success ? requiredRewardSet.Reward2 : "",
                    Card0Reward2 = requiredRewardSet != null && missionEntity.Success ? requiredRewardSet.Reward3 : "",
                    Card0Reward3 = requiredRewardSet != null && missionEntity.Success ? requiredRewardSet.Reward4 : "",
                    Card0Reward4 = requiredRewardSet != null && missionEntity.Success ? requiredRewardSet.Reward5 : "",
                    Card0State = missionEntity.Casualty ? CardState.Casualty : CardState.OnMission,
                    Card1TemplateId = missionEntity.BonusCard1Item?.TemplateId ?? 0,
                    Card1InstanceId = missionEntity.BonusCard1Item?.ItemId ?? 0,
                    Card1Success = missionEntity.BonusCard1Item == null ? CardSlotState.Open :
                        missionEntity.Bonus1Success ? CardSlotState.Success : CardSlotState.Fail,
                    Card1Reward0 = bonus1RewardSet != null && missionEntity.Bonus1Success ? bonus1RewardSet.Reward1 : "",
                    Card1Reward1 = bonus1RewardSet != null && missionEntity.Bonus1Success ? bonus1RewardSet.Reward2 : "",
                    Card1Reward2 = bonus1RewardSet != null && missionEntity.Bonus1Success ? bonus1RewardSet.Reward3 : "",
                    Card1Reward3 = bonus1RewardSet != null && missionEntity.Bonus1Success ? bonus1RewardSet.Reward4 : "",
                    Card1Reward4 = bonus1RewardSet != null && missionEntity.Bonus1Success ? bonus1RewardSet.Reward5 : "",
                    Card1State = missionEntity.Bonus1Casualty ? CardState.Casualty : CardState.OnMission,
                    Card2TemplateId = missionEntity.BonusCard2Item?.TemplateId ?? 0,
                    Card2InstanceId = missionEntity.BonusCard2Item?.ItemId ?? 0,
                    Card2Success = missionEntity.BonusCard2Item == null ? CardSlotState.Open :
                        missionEntity.Bonus2Success ? CardSlotState.Success : CardSlotState.Fail,
                    Card2Reward0 = bonus2RewardSet != null && missionEntity.Bonus2Success ? bonus2RewardSet.Reward1 : "",
                    Card2Reward1 = bonus2RewardSet != null && missionEntity.Bonus2Success ? bonus2RewardSet.Reward2 : "",
                    Card2Reward2 = bonus2RewardSet != null && missionEntity.Bonus2Success ? bonus2RewardSet.Reward3 : "",
                    Card2Reward3 = bonus2RewardSet != null && missionEntity.Bonus2Success ? bonus2RewardSet.Reward4 : "",
                    Card2Reward4 = bonus2RewardSet != null && missionEntity.Bonus2Success ? bonus2RewardSet.Reward5 : "",
                    Card2State = missionEntity.Bonus2Casualty ? CardState.Casualty : CardState.OnMission
                });
            }
        }

        await SendAsync(response, cancellation: ct);
    }

    private bool AreRequirementsMet(MissionStage mission, List<ActiveMissionEntity> activeMissions,
        List<FinishedMissionEntity> finishedMissions, out ActiveMissionEntity? missionEntity)
    {
        var key = MissionsParser.GetMissionKey(mission);
        
        if (finishedMissions.Any(m => m.MissionKey == key))
        {
            missionEntity = null;
            return false;
        }
        
        missionEntity = activeMissions.FirstOrDefault(m => m.MissionKey == key);
        
        if (!string.IsNullOrEmpty(mission.Requirement1))
        {
            if (!finishedMissions.Any(m => m.MissionKey == mission.Requirement1))
            {
                return false;
            }
        }
        
        if (!string.IsNullOrEmpty(mission.Requirement2))
        {
            if (!finishedMissions.Any(m => m.MissionKey == mission.Requirement2))
            {
                return false;
            }
        }
        
        if (!string.IsNullOrEmpty(mission.Requirement3))
        {
            if (!finishedMissions.Any(m => m.MissionKey == mission.Requirement3))
            {
                return false;
            }
        }
        
        if (!string.IsNullOrEmpty(mission.Requirement4))
        {
            if (!finishedMissions.Any(m => m.MissionKey == mission.Requirement4))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsMissionInProgress(ActiveMissionEntity mission)
    {
        var missionData = MissionsParser.GetMission(mission.MissionKey);
        if (missionData is null)
        {
            return false;
        }
        
        var finishTime = mission.Start.AddSeconds(missionData.Duration);
        return DateTime.Now < finishTime;
    }
}