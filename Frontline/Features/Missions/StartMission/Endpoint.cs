using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Features.Session.Inventory.GetInventory;
using Frontline.Game;
using Frontline.Missions;

namespace Frontline.Features.Missions.StartMission;

public class Endpoint : Endpoint<StartMissionRequest, List<MissionStageStatus>>
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
        Post("/Missions/v1/startmission");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(StartMissionRequest req, CancellationToken ct)
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
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but mission doesn't exist.",
                userId, key);
            await SendNotFoundAsync(ct);
            return;
        }
        
        if (player.Supply < missionData.SupplyCost
            || player.Credits < missionData.CreditCost
            || player.Tokens < missionData.TokenCost)
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but doesn't have enough resources.",
                userId, key);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }

        var activeMission = await _missionRepository.GetActiveMissionAsync(userId, key);
        if (activeMission is not null)
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but mission is in progress.",
                userId, key);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }

        var finishedMission = await _missionRepository.GetFinishedMissionAsync(userId, key);
        if (finishedMission is not null)
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but mission is already finished.",
                userId, key);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        if (!await IsValidItem(userId, req.RequiredCardItemId, missionData, CheckItem.Required)
            || !await IsValidItem(userId, req.BonusCard1ItemId, missionData, CheckItem.Bonus1)
            || !await IsValidItem(userId, req.BonusCard2ItemId, missionData, CheckItem.Bonus2))
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but card items are invalid.",
                userId, key);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        var itemIds = new List<int> {req.RequiredCardItemId, req.BonusCard1ItemId, req.BonusCard2ItemId};
        if (await _missionRepository.IsCardOnMissionAsync(userId, itemIds))
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but a card is already on mission.",
                userId, key);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }

        if (itemIds.Where(x => x != 0).GroupBy(x => x).Any(g => g.Count() > 1))
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but card items are duplicates.",
                userId, key);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        if (!await RequirementsMet(userId, missionData))
        {
            Logger.LogWarning("Player {UserId} attempted to start mission {Key} but requirements are not met.",
                userId, key);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }

        var random = new Random();
        
        // TODO: this is not correct.
        var successChance = missionData.SuccessChance == 0 ? 0.7 : missionData.SuccessChance;
        var missionSuccessful = random.NextDouble() <= successChance;

        var bonus1Successful = false; // TODO: implement this
        var bonus2Successful = false; // TODO: implement this

        var casualty = false; // TODO: implement this
        var bonus1Casualty = false; // TODO: implement this
        var bonus2Casualty = false; // TODO: implement this

        var mission = new ActiveMissionEntity
        {
            UserId = userId,
            MissionKey = key,
            Start = DateTime.Now,
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
        
        await _missionRepository.AddActiveMissionAsync(mission);

        if (missionData.SupplyCost > 0 || missionData.CreditCost > 0 || missionData.TokenCost > 0)
        {
            player.Supply -= missionData.SupplyCost;
            player.Credits -= missionData.CreditCost;
            player.Tokens -= missionData.TokenCost;

            await _playerRepository.UpdatePlayerAsync(player);
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

        await SendAsync(response, cancellation: ct);
    }

    private async Task<bool> IsValidItem(int userId, int itemId, MissionStage missionData, CheckItem itemToCheck)
    {
        var requiredSlots = missionData.RequiredSlotCount;
        if (requiredSlots == 0)
        {
            requiredSlots = 1;
        }

        if (requiredSlots < (int) itemToCheck + 1 && itemId == 0)
        {
            return true;
        }
        
        var slotCondition = itemToCheck switch
        {
            CheckItem.Required => missionData.RequiredSlotCondition,
            CheckItem.Bonus1 => missionData.Bonus1SlotCondition,
            CheckItem.Bonus2 => missionData.Bonus2SlotCondition,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(slotCondition))
        {
            return true;
        }
        
        var item = await _inventoryRepository.GetItemAsync(userId, itemId);
        if (item is null)
        {
            Logger.LogWarning("Item not found: {ItemId}", itemId);
            return false;
        }
        
        var template = RulesetParser.GetCardTemplate(item.TemplateId);
        if (template is null)
        {
            Logger.LogWarning("Card template not found: {TemplateId}", item.TemplateId);
            return false;
        }
        
        var minCommand = itemToCheck switch
        {
            CheckItem.Required => missionData.RequiredSlotMinCommand,
            CheckItem.Bonus1 => missionData.Bonus1SlotMinCommand,
            CheckItem.Bonus2 => missionData.Bonus2SlotMinCommand,
            _ => 0
        };
        
        var maxCommand = itemToCheck switch
        {
            CheckItem.Required => missionData.RequiredSlotMaxCommand,
            CheckItem.Bonus1 => missionData.Bonus1SlotMaxCommand,
            CheckItem.Bonus2 => missionData.Bonus2SlotMaxCommand,
            _ => 0
        };
        
        var minRarity = itemToCheck switch
        {
            CheckItem.Required => missionData.RequiredSlotMinRarity,
            CheckItem.Bonus1 => missionData.Bonus1SlotMinRarity,
            CheckItem.Bonus2 => missionData.Bonus2SlotMinRarity,
            _ => CardRarity.Common
        };
        
        var minRank = itemToCheck switch
        {
            CheckItem.Required => missionData.RequiredSlotMinRank,
            CheckItem.Bonus1 => missionData.Bonus1SlotMinRank,
            CheckItem.Bonus2 => missionData.Bonus2SlotMinRank,
            _ => 0
        };
        
        // TODO: add command check
        
        if (template.Rarity < minRarity && minRarity != CardRarity.NumRarities)
        {
            Logger.LogWarning("Card rarity too low. ID: {ItemId}, Rarity: {Rarity}, MinRarity: {MinRarity}",
                item.ItemId, template.Rarity, minRarity);
            return false;
        }
        
        if (itemToCheck == CheckItem.Required
            && missionData.RequiredSlotMaxRarity != CardRarity.Common
            && template.Rarity > missionData.RequiredSlotMaxRarity)
        {
            Logger.LogWarning("Card rarity too high. ID: {ItemId}, Rarity: {Rarity}, MaxRarity: {MaxRarity}",
                item.ItemId, template.Rarity, missionData.RequiredSlotMaxRarity);
            return false;
        }
        
        if (item.Rank < minRank)
        {
            Logger.LogWarning("Card rank too low. ID: {ItemId}, Rank: {Rank}, MinRank: {MinRank}",
                item.ItemId, item.Rank, minRank);
            return false;
        }

        var conditional = MissionsParser.GetConditional(slotCondition);
        if (conditional is null)
        {
            return false;
        }
        
        return CheckConditional(item, template, conditional);
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
        
        return await _missionRepository.HasCompletedMissionsAsync(userId, requirements);
    }
    
    private enum CheckItem
    {
        Required,
        Bonus1,
        Bonus2
    }
}