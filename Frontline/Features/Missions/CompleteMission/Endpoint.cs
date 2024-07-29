using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Game;
using Frontline.Missions;

namespace Frontline.Features.Missions.CompleteMission;

public class Endpoint : Endpoint<CompleteMissionRequest, List<MissionStageStatus>>
{
    private readonly IPlayerRepository _playerRepository;
    
    private readonly IMissionRepository _missionRepository;

    private readonly IInventoryRepository _inventoryRepository;

    public Endpoint(IPlayerRepository playerRepository, IMissionRepository missionRepository,
        IInventoryRepository inventoryRepository)
    {
        _playerRepository = playerRepository;
        _missionRepository = missionRepository;
        _inventoryRepository = inventoryRepository;
    }

    public override void Configure()
    {
        Post("/Missions/v1/completemission");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(CompleteMissionRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.ClaimValue("UserId")!);
        var player = await _playerRepository.GetPlayerAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player not found: {UserId}", userId);
            await SendNotFoundAsync(ct);
            return;
        }
        
        var key = MissionsParser.GetMissionKey(req.Key.Region, req.Key.Faction, req.Key.MissionId);
        
        var missionData = MissionsParser.GetMission(key);
        if (missionData is null)
        {
            Logger.LogWarning("Player {UserId} attempted to complete mission {Key} but mission doesn't exist.",
                userId, key);
            await SendNotFoundAsync(ct);
            return;
        }
        
        var mission = await _missionRepository.GetActiveMissionAsync(userId, key);
        if (mission is null)
        {
            Logger.LogWarning("Player {UserId} attempted to complete mission {Key} but mission isn't started.",
                userId, key);
            await SendNotFoundAsync(ct);
            return;
        }

        if (mission.Success && missionData.MissionType is not MissionType.Persistent)
        {
            var finishedMission = new FinishedMissionEntity
            {
                UserId = mission.UserId,
                MissionKey = mission.MissionKey
            };
            
            await _missionRepository.AddFinishedMissionAsync(finishedMission);
        }

        await _missionRepository.DeleteActiveMissionAsync(mission);
        
        if (missionData.RequiredSlotConsume && mission.RequiredCardItem is not null)
        {
            await _inventoryRepository.RemoveItemAsync(mission.RequiredCardItem);
        }

        if (missionData.Bonus1SlotConsume && mission.BonusCard1Item is not null)
        {
            await _inventoryRepository.RemoveItemAsync(mission.BonusCard1Item);
        }
        
        if (missionData.Bonus2SlotConsume && mission.BonusCard2Item is not null)
        {
            await _inventoryRepository.RemoveItemAsync(mission.BonusCard2Item);
        }

        if (mission.Success)
        {
            var requiredRewardSet = MissionsParser.GetRewardSet(missionData.SuccessReward);
            
            MissionRewardSet? bonus1RewardSet = null;
            MissionRewardSet? bonus2RewardSet = null;

            if (mission.Bonus1Success)
            {
                bonus1RewardSet = MissionsParser.GetBonusRewardSet(missionData.Bonus1SlotCondition);
            }

            if (mission.Bonus2Success)
            {
                bonus2RewardSet = MissionsParser.GetBonusRewardSet(missionData.Bonus2SlotCondition);
            }
            
            await GivePlayerRewards(player, requiredRewardSet, bonus1RewardSet, bonus2RewardSet);
        }
        
        // TODO: casualties
        // TODO: xp to cards
        
        var response = new List<MissionStageStatus>
        {
            new()
            {
                Region = req.Key.Region,
                Faction = req.Key.Faction,
                MissionId = req.Key.MissionId,
                CurrentState = mission.Success ? MissionStageState.Finalized : MissionStageState.Available
            }
        };
        
        response.AddRange(await GetNextMissions(userId, key));
        
        await SendAsync(response, cancellation: ct);
    }

    private async Task GivePlayerRewards(PlayerEntity player, params MissionRewardSet?[] rewardSets)
    {
        var playerUpdated = false;
        List<ItemEntity> items = [];
        
        foreach (var rewardSet in rewardSets)
        {
            if (rewardSet is null)
            {
                continue;
            }

            GivePlayerReward(player, items, rewardSet.Reward1, out var playerUpdated1);
            GivePlayerReward(player, items, rewardSet.Reward2, out var playerUpdated2);
            GivePlayerReward(player, items, rewardSet.Reward3, out var playerUpdated3);
            GivePlayerReward(player, items, rewardSet.Reward4, out var playerUpdated4);
            GivePlayerReward(player, items, rewardSet.Reward5, out var playerUpdated5);
            
            playerUpdated = playerUpdated || playerUpdated1 || playerUpdated2 || playerUpdated3
                            || playerUpdated4 || playerUpdated5;
        }

        if (playerUpdated)
        {
            await _playerRepository.UpdatePlayerAsync(player);
        }

        if (items.Count > 0)
        {
            await _inventoryRepository.AddItemsAsync(player.Id, items);
        }
    }

    private void GivePlayerReward(PlayerEntity player, List<ItemEntity> items, string rewardName,
        out bool playerUpdated)
    {
        playerUpdated = false;
        
        var reward = MissionsParser.GetReward(rewardName);
        if (reward is null)
        {
            return;
        }

        var element = reward.Element;
        if (string.IsNullOrEmpty(element) || element == "Null")
        {
            return;
        }

        var elementParts = element.Split(':');
        if (elementParts.Length != 2)
        {
            Logger.LogWarning("Invalid reward element: {Element}", element);
            return;
        }

        if (elementParts[0] != "Card")
        {
            Logger.LogWarning("Invalid reward element type: {Type}", elementParts[0]);
            return;
        }

        var name = elementParts[1];
        var nameMapping = MissionsParser.GetMissionNameMapping(name);
        if (nameMapping is null)
        {
            Logger.LogWarning("Reward name mapping not found: {Name}", name);
            return;
        }

        var cardTemplate = RulesetParser.GetCardTemplate(nameMapping.TemplateId);
        if (cardTemplate is null)
        {
            Logger.LogWarning("Card template not found: {TemplateId}", nameMapping.TemplateId);
            return;
        }

        if (cardTemplate.Type == CardType.Resource)
        {
            var resourceCardTemplate = (ResourceCardTemplate) cardTemplate;
            
            var resourceValue = resourceCardTemplate.ResourceValue;
            if (resourceValue == 0)
            {
                resourceValue = 1;
            }
            
            var amount = resourceValue * reward.Quantity;

            switch (resourceCardTemplate.ResourceType)
            {
                case ResourceType.Credit:
                    player.Credits += amount;
                    break;
                
                case ResourceType.Xp:
                    player.Xp += amount;
                    break;
                
                case ResourceType.Supply:
                    player.Supply += amount;
                    break;
                
                case ResourceType.Token:
                    player.Tokens += amount;
                    break;
                
                case ResourceType.IntelTypeOperational:
                case ResourceType.IntelTypeTechnical:
                case ResourceType.IntelTypePersonnel:
                    items.Add(new ItemEntity
                    {
                        TemplateId = resourceCardTemplate.CardId,
                        Rank = (sbyte) resourceCardTemplate.MinimumRank
                    });
                    
                    return;
                
                default:
                    Logger.LogWarning("Unhandled resource type: {ResourceType}", resourceCardTemplate.ResourceType);
                    return;
            }
            
            playerUpdated = true;
        }
        else
        {
            items.Add(new ItemEntity
            {
                TemplateId = cardTemplate.CardId,
                Rank = (sbyte) cardTemplate.MinimumRank
            });
        }
    }

    private async Task<List<MissionStageStatus>> GetNextMissions(int userId, string key)
    {
        var nextMissions = new List<MissionStageStatus>();

        foreach (var mission in MissionsParser.Data!.MissionStages.Values)
        {
            if (mission.Requirement1 != key
                && mission.Requirement2 != key
                && mission.Requirement3 != key
                && mission.Requirement4 != key)
            {
                continue;
            }
            
            var requirements = new List<string>();
            
            if (!string.IsNullOrEmpty(mission.Requirement1))
            {
                requirements.Add(mission.Requirement1);
            }
            
            if (!string.IsNullOrEmpty(mission.Requirement2))
            {
                requirements.Add(mission.Requirement2);
            }
            
            if (!string.IsNullOrEmpty(mission.Requirement3))
            {
                requirements.Add(mission.Requirement3);
            }
            
            if (!string.IsNullOrEmpty(mission.Requirement4))
            {
                requirements.Add(mission.Requirement4);
            }

            if (requirements.Count == 1)
            {
                nextMissions.Add(new MissionStageStatus
                {
                    Region = mission.Region,
                    Faction = mission.Faction,
                    MissionId = mission.MissionId,
                    CurrentState = MissionStageState.Available
                });
                
                continue;
            }

            var requirementsMet = await _missionRepository.HasCompletedMissionsAsync(userId, requirements);
            if (!requirementsMet)
            {
                continue;
            }
            
            nextMissions.Add(new MissionStageStatus
            {
                Region = mission.Region,
                Faction = mission.Faction,
                MissionId = mission.MissionId,
                CurrentState = MissionStageState.Available
            });
        }
        
        return nextMissions;
    }
}