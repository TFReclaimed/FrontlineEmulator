using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Frontline.Game;
using Frontline.Game.Card;
using Frontline.Missions;

namespace Frontline.Endpoints.Missions.CompleteMission;

public class CompleteMissionEndpoint : Endpoint<CompleteMissionRequest, List<MissionStageStatus>>
{
    private readonly IPlayerRepository _playerRepository;

    private readonly IActiveMissionRepository _activeMissionRepository;

    private readonly IFinishedMissionRepository _finishedMissionRepository;

    private readonly IInventoryRepository _inventoryRepository;

    public CompleteMissionEndpoint(IPlayerRepository playerRepository, IActiveMissionRepository activeMissionRepository,
        IFinishedMissionRepository finishedMissionRepository, IInventoryRepository inventoryRepository)
    {
        _playerRepository = playerRepository;
        _activeMissionRepository = activeMissionRepository;
        _finishedMissionRepository = finishedMissionRepository;
        _inventoryRepository = inventoryRepository;
    }

    public override void Configure()
    {
        Post("/Missions/v1/completemission");
        AllowFormData(true);
    }

    public override async Task HandleAsync(CompleteMissionRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var player = await _playerRepository.GetByIdAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player not found: {UserId}", userId);
            await Send.NotFoundAsync();
            return;
        }

        var key = MissionsParser.GetMissionKey(req.Key.Region, req.Key.Faction, req.Key.MissionId);

        var missionData = MissionsParser.GetMission(key);
        if (missionData is null)
        {
            Logger.LogWarning("Player {UserId} attempted to complete mission {Key} but mission doesn't exist.",
                userId, key);
            await Send.NotFoundAsync();
            return;
        }

        var mission = await _activeMissionRepository.GetActiveMissionAsync(userId, key);
        if (mission is null)
        {
            Logger.LogWarning("Player {UserId} attempted to complete mission {Key} but mission isn't started.",
                userId, key);
            await Send.NotFoundAsync();
            return;
        }

        Logger.LogInformation("Player {UserId} completed mission {Key}.", userId, key);

        if (mission.Success && missionData.MissionType is not MissionType.Persistent)
        {
            var finishedMission = new FinishedMissionEntity
            {
                UserId = mission.UserId,
                MissionKey = mission.MissionKey
            };

            await _finishedMissionRepository.AddAsync(finishedMission);
        }

        await _activeMissionRepository.DeleteAsync(mission);

        if (missionData.RequiredSlotConsume && mission.RequiredCardItem is not null)
        {
            await _inventoryRepository.DeleteAsync(mission.RequiredCardItem);
        }
        else if (mission.Casualty && mission.RequiredCardItem is not null)
        {
            mission.RequiredCardItem.Casualty = true;
            await _inventoryRepository.UpdateAsync(mission.RequiredCardItem);
        }

        if (missionData.Bonus1SlotConsume && mission.BonusCard1Item is not null)
        {
            await _inventoryRepository.DeleteAsync(mission.BonusCard1Item);
        }
        else if (mission.Bonus1Casualty && mission.BonusCard1Item is not null)
        {
            mission.BonusCard1Item.Casualty = true;
            await _inventoryRepository.UpdateAsync(mission.BonusCard1Item);
        }

        if (missionData.Bonus2SlotConsume && mission.BonusCard2Item is not null)
        {
            await _inventoryRepository.DeleteAsync(mission.BonusCard2Item);
        }
        else if (mission.Bonus2Casualty && mission.BonusCard2Item is not null)
        {
            mission.BonusCard2Item.Casualty = true;
            await _inventoryRepository.UpdateAsync(mission.BonusCard2Item);
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

            player.MissionsComplete++;
            await _playerRepository.UpdateAsync(player);
        }

        if (mission.RequiredCardItem is not null && mission.Success)
        {
            await GiveCardXp(mission.RequiredCardItem, missionData, missionData.RequiredSlotCount >= 1);
        }

        if (mission.BonusCard1Item is not null && mission.Bonus1Success)
        {
            await GiveCardXp(mission.BonusCard1Item, missionData, missionData.RequiredSlotCount >= 2);
        }

        if (mission.BonusCard2Item is not null && mission.Bonus2Success)
        {
            await GiveCardXp(mission.BonusCard2Item, missionData, missionData.RequiredSlotCount >= 3);
        }

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

        await Send.OkAsync(response);
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

            var rewardsToGive = new List<string>();
            if (rewardSet.Type == RewardSetType.All)
            {
                rewardsToGive.Add(rewardSet.Reward1);
                rewardsToGive.Add(rewardSet.Reward2);
                rewardsToGive.Add(rewardSet.Reward3);
                rewardsToGive.Add(rewardSet.Reward4);
                rewardsToGive.Add(rewardSet.Reward5);
            }
            else if (rewardSet.Type == RewardSetType.Pick)
            {
                var rewardOptions = new List<string>();
                if (!string.IsNullOrEmpty(rewardSet.Reward1))
                {
                    rewardOptions.Add(rewardSet.Reward1);
                }

                if (!string.IsNullOrEmpty(rewardSet.Reward2))
                {
                    rewardOptions.Add(rewardSet.Reward2);
                }

                if (!string.IsNullOrEmpty(rewardSet.Reward3))
                {
                    rewardOptions.Add(rewardSet.Reward3);
                }

                if (!string.IsNullOrEmpty(rewardSet.Reward4))
                {
                    rewardOptions.Add(rewardSet.Reward4);
                }

                if (!string.IsNullOrEmpty(rewardSet.Reward5))
                {
                    rewardOptions.Add(rewardSet.Reward5);
                }

                if (rewardOptions.Count > 0)
                {
                    var selectedReward = rewardOptions[Random.Shared.Next(rewardOptions.Count)];
                    rewardsToGive.Add(selectedReward);
                }
            }

            foreach (var rewardName in rewardsToGive)
            {
                var rewardGiven = await GivePlayerReward(player, items, rewardName);
                if (rewardGiven)
                {
                    playerUpdated = true;
                }
            }
        }

        if (playerUpdated)
        {
            await _playerRepository.UpdateAsync(player);
        }

        if (items.Count > 0)
        {
            await _inventoryRepository.AddItemsAsync(player.Id, items);
        }
    }

    private async Task<bool> GivePlayerReward(PlayerEntity player, List<ItemEntity> items, string rewardName)
    {
        var reward = MissionsParser.GetReward(rewardName);
        if (reward is null)
        {
            return false;
        }

        var element = reward.Element;
        if (string.IsNullOrEmpty(element) || element == "Null")
        {
            return false;
        }

        var elementParts = element.Split(':');
        if (elementParts.Length != 2)
        {
            Logger.LogWarning("Invalid reward element: {Element}", element);
            return false;
        }

        var rewardType = elementParts[0];
        if (rewardType == "Card")
        {
            var name = elementParts[1];
            GiveCardReward(player, items, out var playerUpdated, name, reward);
            return playerUpdated;
        }

        if (rewardType == "RewardSet")
        {
            var name = elementParts[1];
            var rewardSet = MissionsParser.GetRewardSet(name);
            if (rewardSet is null)
            {
                Logger.LogWarning("Reward set not found: {Name}", name);
                return false;
            }

            await GivePlayerRewards(player, rewardSet);
            return true;
        }

        return false;
    }

    private void GiveCardReward(PlayerEntity player, List<ItemEntity> items, out bool playerUpdated, string name,
        MissionReward reward)
    {
        playerUpdated = false;

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

    private async Task GiveCardXp(ItemEntity card, MissionStage missionData, bool required)
    {
        var cardTemplate = RulesetParser.GetCardTemplate(card.TemplateId);
        if (cardTemplate is null)
        {
            Logger.LogWarning("Card template not found: {TemplateId}", card.TemplateId);
            return;
        }

        if (cardTemplate.Type != CardType.Pilot && cardTemplate.Type != CardType.Titan)
        {
            return;
        }

        var slotXp = MissionsParser.GetMissionSlotXp(required ? "Required" : "Bonus");
        if (slotXp is null)
        {
            Logger.LogWarning("Slot XP definition not found: {SlotType}", required ? "Required" : "Bonus");
            return;
        }

        var multiplier = missionData.VisibilityRarity switch
        {
            VisibilityRarity.Uncommon => slotXp.Uncommon,
            VisibilityRarity.Rare => slotXp.Rare,
            VisibilityRarity.VeryRare => slotXp.VeryRare,
            _ => 1f
        };

        card.Xp += (int) Math.Ceiling(slotXp.Base * multiplier);

        await _inventoryRepository.UpdateAsync(card);
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

            var requirementsMet = await _finishedMissionRepository.HasCompletedMissionsAsync(userId, requirements);
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