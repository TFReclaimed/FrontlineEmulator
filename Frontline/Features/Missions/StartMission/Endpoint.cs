using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Frontline.Features.Session.Inventory.GetInventory;
using Frontline.Game;
using Frontline.Missions;

namespace Frontline.Features.Missions.StartMission;

public class Endpoint : Endpoint<StartMissionRequest, List<MissionStageStatus>>
{
    private readonly IPlayerRepository _playerRepository;

    private readonly IActiveMissionRepository _activeMissionRepository;

    private readonly IFinishedMissionRepository _finishedMissionRepository;

    private readonly IInventoryRepository _inventoryRepository;

    public Endpoint(IPlayerRepository playerRepository, IActiveMissionRepository activeMissionRepository,
        IFinishedMissionRepository finishedMissionRepository, IInventoryRepository inventoryRepository)
    {
        _playerRepository = playerRepository;
        _activeMissionRepository = activeMissionRepository;
        _finishedMissionRepository = finishedMissionRepository;
        _inventoryRepository = inventoryRepository;
    }

    public override void Configure()
    {
        Post("/Missions/v1/startmission");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(StartMissionRequest req, CancellationToken ct)
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
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but mission doesn't exist.",
                userId, key);
            await Send.NotFoundAsync();
            return;
        }

        if (player.Supply < missionData.SupplyCost
            || player.Credits < missionData.CreditCost
            || player.Tokens < missionData.TokenCost)
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but doesn't have enough resources.",
                userId, key);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var activeMission = await _activeMissionRepository.GetActiveMissionAsync(userId, key);
        if (activeMission is not null)
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but mission is in progress.",
                userId, key);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var finishedMission = await _finishedMissionRepository.GetByIdAsync(userId, key);
        if (finishedMission is not null)
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but mission is already finished.",
                userId, key);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var (requiredIsValid, requiredItem) = await IsValidItem(userId, req.RequiredCardItemId, missionData,
            MissionSlotId.Required);
        var (bonus1IsValid, bonus1Item) = await IsValidItem(userId, req.BonusCard1ItemId, missionData,
            MissionSlotId.Bonus1);
        var (bonus2IsValid, bonus2Item) = await IsValidItem(userId, req.BonusCard2ItemId, missionData,
            MissionSlotId.Bonus2);

        if (!requiredIsValid || !bonus1IsValid || !bonus2IsValid)
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but card items are invalid.",
                userId, key);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var itemIds = new List<int> {req.RequiredCardItemId, req.BonusCard1ItemId, req.BonusCard2ItemId};
        if (await _activeMissionRepository.IsCardOnMissionAsync(userId, itemIds))
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but a card is already on mission.",
                userId, key);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        if (itemIds.Where(x => x != 0).GroupBy(x => x).Any(g => g.Count() > 1))
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but card items are duplicates.",
                userId, key);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        if (!await RequirementsMet(userId, missionData))
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but requirements are not met.",
                userId, key);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        Logger.LogInformation("Player {UserId} started mission {Key}.", userId, key);

        var random = new Random();

        var missionSuccessful = random.NextDouble() <= GetSuccessChance(missionData, MissionSlotId.Required,
            requiredItem, bonus1Item, bonus2Item);
        var bonus1Successful = bonus1Item != null && random.NextDouble() <= GetSuccessChance(missionData,
            MissionSlotId.Bonus1, requiredItem, bonus1Item, bonus2Item);
        var bonus2Successful = bonus2Item != null && random.NextDouble() <= GetSuccessChance(missionData,
            MissionSlotId.Bonus2, requiredItem, bonus1Item, bonus2Item);

        if (missionData.RequiredSlotCount == 1 && !missionSuccessful)
        {
            bonus1Successful = false;
            bonus2Successful = false;
        }
        else if (missionData.RequiredSlotCount == 2 && (!missionSuccessful || !bonus1Successful))
        {
            missionSuccessful = false;
            bonus1Successful = false;
            bonus2Successful = false;
        }
        else if (missionData.RequiredSlotCount == 3 && (!missionSuccessful || !bonus1Successful || !bonus2Successful))
        {
            missionSuccessful = false;
            bonus1Successful = false;
            bonus2Successful = false;
        }

        var casualty = random.NextDouble() <= GetCasualtyChance(missionData, MissionSlotId.Required);
        var bonus1Casualty = bonus1Item != null && random.NextDouble() <= GetCasualtyChance(missionData,
            MissionSlotId.Bonus1);
        var bonus2Casualty = bonus2Item != null && random.NextDouble() <= GetCasualtyChance(missionData,
            MissionSlotId.Bonus2);

        var mission = new ActiveMissionEntity
        {
            UserId = userId,
            MissionKey = key,
            Start = DateTime.UtcNow,
            RequiredCardItemId = req.RequiredCardItemId,
            BonusCard1ItemId = req.BonusCard1ItemId == 0 ? null : req.BonusCard1ItemId,
            BonusCard2ItemId = req.BonusCard2ItemId == 0 ? null : req.BonusCard2ItemId,
            Success = missionSuccessful,
            Bonus1Success = bonus1Successful,
            Bonus2Success = bonus2Successful,
            Casualty = casualty,
            Bonus1Casualty = bonus1Casualty,
            Bonus2Casualty = bonus2Casualty
        };

        await _activeMissionRepository.AddAsync(mission);

        if (missionData.SupplyCost > 0 || missionData.CreditCost > 0 || missionData.TokenCost > 0)
        {
            player.Supply -= missionData.SupplyCost;
            player.Credits -= missionData.CreditCost;
            player.Tokens -= missionData.TokenCost;

            await _playerRepository.UpdateAsync(player);
        }

        var requiredRewardSet = MissionsParser.GetRewardSet(missionData.SuccessReward);
        var bonus1RewardSet = MissionsParser.GetBonusRewardSet(missionData.Bonus1SlotCondition);
        var bonus2RewardSet = MissionsParser.GetBonusRewardSet(missionData.Bonus2SlotCondition);

        var response = new List<MissionStageStatus>
        {
            new()
            {
                Region = missionData.Region,
                Faction = missionData.Faction,
                MissionId = missionData.MissionId,
                CurrentState = MissionStageState.InProgress,
                MissionStageStart = mission.Start.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                Card0TemplateId = req.RequiredCardTemplateId,
                Card0InstanceId = req.RequiredCardItemId,
                Card0Success = missionSuccessful ? CardSlotState.Success : CardSlotState.Fail,
                Card0Reward0 = requiredRewardSet != null && missionSuccessful ? requiredRewardSet.Reward1 : "",
                Card0Reward1 = requiredRewardSet != null && missionSuccessful ? requiredRewardSet.Reward2 : "",
                Card0Reward2 = requiredRewardSet != null && missionSuccessful ? requiredRewardSet.Reward3 : "",
                Card0Reward3 = requiredRewardSet != null && missionSuccessful ? requiredRewardSet.Reward4 : "",
                Card0Reward4 = requiredRewardSet != null && missionSuccessful ? requiredRewardSet.Reward5 : "",
                Card0State = casualty ? CardState.Casualty : CardState.OnMission,
                Card1TemplateId = req.BonusCard1TemplateId,
                Card1InstanceId = req.BonusCard1ItemId,
                Card1Success = req.BonusCard1ItemId == 0 ? CardSlotState.Open :
                    bonus1Successful ? CardSlotState.Success : CardSlotState.Fail,
                Card1Reward0 = bonus1RewardSet != null && bonus1Successful ? bonus1RewardSet.Reward1 : "",
                Card1Reward1 = bonus1RewardSet != null && bonus1Successful ? bonus1RewardSet.Reward2 : "",
                Card1Reward2 = bonus1RewardSet != null && bonus1Successful ? bonus1RewardSet.Reward3 : "",
                Card1Reward3 = bonus1RewardSet != null && bonus1Successful ? bonus1RewardSet.Reward4 : "",
                Card1Reward4 = bonus1RewardSet != null && bonus1Successful ? bonus1RewardSet.Reward5 : "",
                Card1State = bonus1Casualty ? CardState.Casualty : CardState.OnMission,
                Card2TemplateId = req.BonusCard2TemplateId,
                Card2InstanceId = req.BonusCard2ItemId,
                Card2Success = req.BonusCard2ItemId == 0 ? CardSlotState.Open :
                    bonus2Successful ? CardSlotState.Success : CardSlotState.Fail,
                Card2Reward0 = bonus2RewardSet != null && bonus2Successful ? bonus2RewardSet.Reward1 : "",
                Card2Reward1 = bonus2RewardSet != null && bonus2Successful ? bonus2RewardSet.Reward2 : "",
                Card2Reward2 = bonus2RewardSet != null && bonus2Successful ? bonus2RewardSet.Reward3 : "",
                Card2Reward3 = bonus2RewardSet != null && bonus2Successful ? bonus2RewardSet.Reward4 : "",
                Card2Reward4 = bonus2RewardSet != null && bonus2Successful ? bonus2RewardSet.Reward5 : "",
                Card2State = bonus2Casualty ? CardState.Casualty : CardState.OnMission
            }
        };

        await Send.OkAsync(response);
    }

    private async Task<(bool IsValid, ItemEntity? item)> IsValidItem(int userId, int itemId, MissionStage missionData,
        MissionSlotId slot)
    {
        var requiredSlots = missionData.RequiredSlotCount;
        if (requiredSlots == 0)
        {
            requiredSlots = 1;
        }

        if (requiredSlots < (int) slot + 1 && itemId == 0)
        {
            return (true, null);
        }

        var slotCondition = slot switch
        {
            MissionSlotId.Required => missionData.RequiredSlotCondition,
            MissionSlotId.Bonus1 => missionData.Bonus1SlotCondition,
            MissionSlotId.Bonus2 => missionData.Bonus2SlotCondition,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(slotCondition))
        {
            return (true, null);
        }

        var item = await _inventoryRepository.GetItemAsync(userId, itemId);
        if (item is null)
        {
            Logger.LogWarning("Item not found: {ItemId}", itemId);
            return (false, null);
        }

        var template = RulesetParser.GetCardTemplate(item.TemplateId);
        if (template is null)
        {
            Logger.LogWarning("Card template not found: {TemplateId}", item.TemplateId);
            return (false, null);
        }

        var minCommand = slot switch
        {
            MissionSlotId.Required => missionData.RequiredSlotMinCommand,
            MissionSlotId.Bonus1 => missionData.Bonus1SlotMinCommand,
            MissionSlotId.Bonus2 => missionData.Bonus2SlotMinCommand,
            _ => 0
        };

        var maxCommand = slot switch
        {
            MissionSlotId.Required => missionData.RequiredSlotMaxCommand,
            MissionSlotId.Bonus1 => missionData.Bonus1SlotMaxCommand,
            MissionSlotId.Bonus2 => missionData.Bonus2SlotMaxCommand,
            _ => 0
        };

        var minRarity = slot switch
        {
            MissionSlotId.Required => missionData.RequiredSlotMinRarity,
            MissionSlotId.Bonus1 => missionData.Bonus1SlotMinRarity,
            MissionSlotId.Bonus2 => missionData.Bonus2SlotMinRarity,
            _ => CardRarity.Common
        };

        var minRank = slot switch
        {
            MissionSlotId.Required => missionData.RequiredSlotMinRank,
            MissionSlotId.Bonus1 => missionData.Bonus1SlotMinRank,
            MissionSlotId.Bonus2 => missionData.Bonus2SlotMinRank,
            _ => 0
        };

        // TODO: add command check

        if (template.Rarity < minRarity && minRarity != CardRarity.NumRarities)
        {
            Logger.LogWarning("Card rarity too low. ID: {ItemId}, Rarity: {Rarity}, MinRarity: {MinRarity}",
                item.ItemId, template.Rarity, minRarity);
            return (false, null);
        }

        if (slot == MissionSlotId.Required
            && missionData.RequiredSlotMaxRarity != CardRarity.Common
            && template.Rarity > missionData.RequiredSlotMaxRarity)
        {
            Logger.LogWarning("Card rarity too high. ID: {ItemId}, Rarity: {Rarity}, MaxRarity: {MaxRarity}",
                item.ItemId, template.Rarity, missionData.RequiredSlotMaxRarity);
            return (false, null);
        }

        if (item.Rank < minRank)
        {
            Logger.LogWarning("Card rank too low. ID: {ItemId}, Rank: {Rank}, MinRank: {MinRank}",
                item.ItemId, item.Rank, minRank);
            return (false, null);
        }

        if (item.IsInDropship && item.DropshipId != 0 && item.DropshipId != 1)
        {
            Logger.LogWarning("Card {ItemId} is in dropship {DropshipId}", item.ItemId, item.DropshipId);
            return (false, null);
        }

        var conditional = MissionsParser.GetConditional(slotCondition);
        if (conditional is null)
        {
            return (false, null);
        }

        var isValid = CheckConditional(item, template, conditional);
        return (isValid, item);
    }

    private bool CheckConditional(ItemEntity item, CardTemplate template, MissionConditional conditional)
    {
        switch (conditional.Attribute)
        {
            case ConditionalAttribute.IsType:
                if (Enum.TryParse<CardType>(conditional.Comparison, out var type))
                {
                    return CompareValues(conditional.Operator, template.Type, type);
                }

                Logger.LogWarning("Unknown card type: {Type}", conditional.Comparison);
                return false;

            case ConditionalAttribute.IsUnitType:
                if (Enum.TryParse<UnitType>(conditional.Comparison, out var unitType))
                {
                    if (template is UnitCardTemplate unitCardTemplate)
                    {
                        return CompareValues(conditional.Operator, unitCardTemplate.UnitType, unitType);
                    }

                    Logger.LogWarning("Card is not a unit card: {TemplateId}", item.TemplateId);
                    return false;
                }

                Logger.LogWarning("Unknown unit type: {UnitType}", conditional.Comparison);
                break;

            case ConditionalAttribute.HasTrait:
                // TODO: implement this
                break;

            case ConditionalAttribute.HasFlag:
                // TODO: implement this
                break;

            case ConditionalAttribute.IsName:
                var nameMapping = MissionsParser.GetMissionNameMapping(conditional.Comparison);
                if (nameMapping is not null)
                {
                    return CompareValues(conditional.Operator, item.TemplateId, nameMapping.TemplateId);
                }

                Logger.LogWarning("Unknown name mapping: {Name}", conditional.Comparison);
                return false;

            case ConditionalAttribute.Command:
                // TODO: implement this
                break;

            case ConditionalAttribute.IsRarity:
                if (Enum.TryParse<CardRarity>(conditional.Comparison, out var rarity))
                {
                    return CompareValues(conditional.Operator, template.Rarity, rarity);
                }

                Logger.LogWarning("Unknown rarity: {Rarity}", conditional.Comparison);
                return false;

            case ConditionalAttribute.Rank:
                if (int.TryParse(conditional.Comparison, out var rank))
                {
                    return CompareValues(conditional.Operator, item.Rank, rank);
                }

                Logger.LogWarning("Invalid rank: {Rank}", conditional.Comparison);
                return false;
        }

        Logger.LogWarning("Unimplemented conditional attribute: {Attribute}", conditional.Attribute);
        return false;
    }

    private bool CompareValues(Operator @operator, object obj1, object obj2)
    {
        switch (@operator)
        {
            case Operator.IsEqual:
                return obj1.Equals(obj2);

            case Operator.IsNotEqual:
                return !obj1.Equals(obj2);

            case Operator.IsGreaterThan:
                return (double) obj1 > (double) obj2;

            case Operator.IsLessThan:
                return (double) obj1 < (double) obj2;

            case Operator.IsGreaterThanOrEqual:
                return (double) obj1 >= (double) obj2;

            case Operator.IsLessThanOrEqual:
                return (double) obj1 <= (double) obj2;

            default:
                Logger.LogWarning("Unknown operator: {Operator}", @operator);
                return false;
        }
    }

    private async Task<bool> RequirementsMet(int userId, MissionStage missionData)
    {
        var requirements = new List<string>();

        if (!string.IsNullOrEmpty(missionData.Requirement1))
        {
            requirements.Add(missionData.Requirement1);
        }

        if (!string.IsNullOrEmpty(missionData.Requirement2))
        {
            requirements.Add(missionData.Requirement2);
        }

        if (!string.IsNullOrEmpty(missionData.Requirement3))
        {
            requirements.Add(missionData.Requirement3);
        }

        if (!string.IsNullOrEmpty(missionData.Requirement4))
        {
            requirements.Add(missionData.Requirement4);
        }

        if (requirements.Count == 0)
        {
            return true;
        }

        return await _finishedMissionRepository.HasCompletedMissionsAsync(userId, requirements);
    }

    private float GetSuccessChance(MissionStage missionData, MissionSlotId slot, ItemEntity? requiredItem,
        ItemEntity? bonus1Item, ItemEntity? bonus2Item)
    {
        var success = GetBaseSuccessChance(missionData, slot);
        success += GetSuccessBonus(slot, requiredItem, bonus1Item, bonus2Item);

        return Math.Clamp(success, 0f, 1f);
    }

    private float GetBaseSuccessChance(MissionStage missionData, MissionSlotId slot)
    {
        var success = 1f;

        switch (slot)
        {
            case MissionSlotId.Required:
                success = missionData.SuccessChance;
                break;

            case MissionSlotId.Bonus1:
                if (!GetBonusSuccessOverride(missionData, MissionSlotId.Bonus1, out success))
                {
                    success = 0.5f;
                }

                break;

            case MissionSlotId.Bonus2:
                if (!GetBonusSuccessOverride(missionData, MissionSlotId.Bonus2, out success))
                {
                    success = 0.35f;
                }

                break;
        }

        return success;
    }

    private bool GetBonusSuccessOverride(MissionStage missionData, MissionSlotId slot, out float success)
    {
        var slotName = slot switch
        {
            MissionSlotId.Required => missionData.RequiredSlotCondition,
            MissionSlotId.Bonus1 => missionData.Bonus1SlotCondition,
            MissionSlotId.Bonus2 => missionData.Bonus2SlotCondition,
            _ => ""
        };

        var missionSlot = MissionsParser.GetMissionSlot(slotName);
        if (missionSlot is not null && missionSlot.BonusSuccessOverride != -1f)
        {
            success = missionSlot.BonusSuccessOverride;
            return true;
        }

        success = 0f;
        return false;
    }

    private float GetSuccessBonus(MissionSlotId forSlot, ItemEntity? requiredItem, ItemEntity? bonus1Item,
        ItemEntity? bonus2Item)
    {
        CardTemplate? bonus1Template = null;
        if (bonus1Item is not null)
        {
            bonus1Template = RulesetParser.GetCardTemplate(bonus1Item.TemplateId);
        }

        CardTemplate? bonus2Template = null;
        if (bonus2Item is not null)
        {
            bonus2Template = RulesetParser.GetCardTemplate(bonus2Item.TemplateId);
        }

        var bonus = 0f;

        switch (forSlot)
        {
            case MissionSlotId.Required:
                if (requiredItem is not null)
                {
                    bonus += GetSuccessBonus(MissionSlotId.Required, MissionSlotId.Required, requiredItem.Rank);
                }

                if (bonus1Template is not null)
                {
                    bonus += GetSuccessBonus(MissionSlotId.Required, MissionSlotId.Bonus1, bonus1Template.Rarity);
                }

                if (bonus2Template is not null)
                {
                    bonus += GetSuccessBonus(MissionSlotId.Required, MissionSlotId.Bonus2, bonus2Template.Rarity);
                }

                break;

            case MissionSlotId.Bonus1:
                if (requiredItem is not null)
                {
                    bonus += GetSuccessBonus(MissionSlotId.Bonus1, MissionSlotId.Required, requiredItem.Rank);
                }

                if (bonus1Template is not null)
                {
                    bonus += GetSuccessBonus(MissionSlotId.Bonus1, MissionSlotId.Bonus1, bonus1Template.Rarity);
                }

                break;

            case MissionSlotId.Bonus2:
                if (requiredItem is not null)
                {
                    bonus += GetSuccessBonus(MissionSlotId.Bonus2, MissionSlotId.Required, requiredItem.Rank);
                }

                if (bonus2Template is not null)
                {
                    bonus += GetSuccessBonus(MissionSlotId.Bonus2, MissionSlotId.Bonus2, bonus2Template.Rarity);
                }

                break;
        }

        return bonus;
    }

    private float GetSuccessBonus(MissionSlotId forSlot, MissionSlotId fromSlot, int rank)
    {
        var bonus = 0f;

        switch (forSlot)
        {
            case MissionSlotId.Required:
                if (fromSlot == MissionSlotId.Required)
                {
                    bonus = rank switch
                    {
                        1 => 0f,
                        2 => 0.01f,
                        3 => 0.04f,
                        4 => 0.09f,
                        5 => 0.16f,
                        6 => 0.25f,
                        _ => bonus
                    };
                }

                break;

            case MissionSlotId.Bonus1:
            case MissionSlotId.Bonus2:
                if (fromSlot == MissionSlotId.Required)
                {
                    bonus = rank switch
                    {
                        1 => 0.01f,
                        2 => 0.02f,
                        3 => 0.03f,
                        4 => 0.04f,
                        5 => 0.05f,
                        6 => 0.06f,
                        _ => bonus
                    };
                }

                break;
        }

        return bonus;
    }

    private float GetSuccessBonus(MissionSlotId forSlot, MissionSlotId fromSlot, CardRarity rarity)
    {
        var bonus = 0f;

        switch (forSlot)
        {
            case MissionSlotId.Required:
                if (fromSlot is MissionSlotId.Bonus1 or MissionSlotId.Bonus2)
                {
                    bonus = rarity switch
                    {
                        CardRarity.Common => 0f,
                        CardRarity.Uncommon => 0.01f,
                        CardRarity.Rare => 0.04f,
                        CardRarity.UltraRare => 0.09f,
                        CardRarity.Exclusive => 0.16f,
                        _ => bonus
                    };
                }

                break;

            case MissionSlotId.Bonus1:
            case MissionSlotId.Bonus2:
                if (fromSlot is MissionSlotId.Bonus1 or MissionSlotId.Bonus2)
                {
                    bonus = rarity switch
                    {
                        CardRarity.Common => 0f,
                        CardRarity.Uncommon => 0.05f,
                        CardRarity.Rare => 0.1f,
                        CardRarity.UltraRare => 0.15f,
                        CardRarity.Exclusive => 0.2f,
                        _ => bonus
                    };
                }

                break;
        }

        return bonus;
    }

    private float GetCasualtyChance(MissionStage missionData, MissionSlotId slot)
    {
        var casualty = 0f;

        var slotName = slot switch
        {
            MissionSlotId.Required => missionData.RequiredSlotCondition,
            MissionSlotId.Bonus1 => missionData.Bonus1SlotCondition,
            MissionSlotId.Bonus2 => missionData.Bonus2SlotCondition,
            _ => ""
        };

        var missionSlot = MissionsParser.GetMissionSlot(slotName);
        if (missionSlot is null || !GetCasualtyOverride(missionSlot, slot, out casualty))
        {
            casualty = slot switch
            {
                MissionSlotId.Required => 0.1f,
                MissionSlotId.Bonus1 => 0.15f,
                MissionSlotId.Bonus2 => 0.2f,
                _ => casualty
            };
        }

        return Math.Clamp(casualty, 0f, 1f);
    }

    private bool GetCasualtyOverride(MissionSlot missionSlot, MissionSlotId slot, out float casualty)
    {
        switch (slot)
        {
            case MissionSlotId.Required:
                if (missionSlot.ReqCasualtyOverride != -1f)
                {
                    casualty = missionSlot.ReqCasualtyOverride;
                    return true;
                }

                break;

            case MissionSlotId.Bonus1:
            case MissionSlotId.Bonus2:
                if (missionSlot.BonusCasualtyOverride != -1f)
                {
                    casualty = missionSlot.BonusCasualtyOverride;
                    return true;
                }

                break;
        }

        casualty = 0f;
        return false;
    }

    private enum MissionSlotId
    {
        Required,
        Bonus1,
        Bonus2
    }
}