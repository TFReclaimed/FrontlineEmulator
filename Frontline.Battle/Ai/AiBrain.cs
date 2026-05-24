using Frontline.Battle.Data.Card;

namespace Frontline.Battle.Ai;

public class AiBrain
{
    private readonly Player _player;

    private readonly List<AiWeightVariable> _activeWeightValues;

    private readonly List<AiGameAction> _actions = [];

    public AiBrain(AiProfile data, Player player)
    {
        _player = player;
        _activeWeightValues = new List<AiWeightVariable>(data.BaseWeightValues.Count);
        _activeWeightValues.AddRange(data.BaseWeightValues);
    }

    public AiGameAction CalculateNextAction(CcgGameState game)
    {
        _actions.Clear();
        CalculateDeployActions(game);
        CalculateMoveActions(game);
        CalculateAttackActions(game);
        _actions.Add(CreateEndTurnAction());

        var newActions = new List<AiGameAction>();
        for (var i = 0; i < _actions.Count; i++)
        {
            var action = _actions[i];
            if (action.Weight < 0f)
            {
                action.Weight = 0f - action.Weight;
                newActions.Add(action);
            }
        }

        if (newActions.Count > 0)
        {
            _actions.Clear();
            _actions.AddRange(newActions);
        }

        _actions.Sort(AiGameAction.SortByWeight);

        var num = GetWeightValue(AiWeightType.AiWeightTolerance) + 5f;
        var actionIndex = 0;
        if (num > 0f)
        {
            for (; actionIndex < _actions.Count && _actions[actionIndex].Weight + num >= _actions[0].Weight; actionIndex++)
            {
            }
        }
        else
        {
            actionIndex = _actions.Count;
        }

        if (actionIndex > 1)
        {
            var weight = 0f;
            for (var k = 0; k < actionIndex; k++)
            {
                weight += _actions[k].Weight;
            }

            weight = RandomBetween(0f, weight);
            actionIndex = 0;

            while (weight > 0f)
            {
                weight -= _actions[actionIndex].Weight;
                if (weight > 0f)
                {
                    actionIndex++;
                }
            }
        }
        else
        {
            actionIndex = 0;
        }

        return _actions[actionIndex];
    }

    private static float RandomBetween(float minInclusive, float maxInclusive)
    {
        return Random.Shared.NextSingle() * (maxInclusive - minInclusive) + minInclusive;
    }

    private static AiGameAction CreateEndTurnAction()
    {
        return new AiGameAction
        {
            ActionType = GameEvent.TriggerEndTurnTraits,
            Hostile = false,
            Weight = 1f
        };
    }

    private void CalculateDeployActions(CcgGameState game)
    {
        Card? card;
        var count = _player.Hand.Cards.Count;
        for (var i = 0; i < count; i++)
        {
            card = _player.Hand.Cards[i];
            GenerateDeployTargets(card, game);
        }

        card = _player.SupportDeck.GetCurrent();
        if (card != null)
        {
            GenerateDeployTargets(card, game);
        }
    }

    private void CalculateMoveActions(CcgGameState game)
    {
        var gameIndex = _player.PlayerIndex;
        var regionEnum = (Region) gameIndex;
        var slots = game.Board.Regions[(uint) regionEnum].Slots;
        for (sbyte slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            var card = slots[slotIndex].PrimaryCard;
            if (card != null)
            {
                GenerateMoveTargets(card, game);
            }
        }
    }

    private void CalculateAttackActions(CcgGameState game)
    {
        var gameIndex = _player.PlayerIndex;
        var regionEnum = (Region) gameIndex;
        var slots = game.Board.Regions[(uint) regionEnum].Slots;
        for (sbyte slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            var card = slots[slotIndex].PrimaryCard;
            if (card != null)
            {
                GenerateAttackTargets(card, game);
            }
        }

        slots = game.Board.Regions[2].Slots;
        for (sbyte slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            var card = slots[slotIndex].PrimaryCard;
            if (card != null && card.ActiveData.Owner == gameIndex)
            {
                GenerateAttackTargets(card, game);
            }
        }
    }

    private void CheckDeployRegionIndex(CcgGameState game, sbyte playerIndex, Card card, TargetableArea area,
        Region region, sbyte slotIndex, sbyte pushDir, float cardWeight, CardStack[]? slots)
    {
        var targetCardId = 0;
        sbyte ownerId = 0;
        var weight = 0f;
        var instanceId = card.InstanceId;
        if (!game.CanDeploy(playerIndex, instanceId, area, region, slotIndex, 0))
        {
            return;
        }

        if (slots?[slotIndex].PrimaryCard != null)
        {
            var card2 = slots[slotIndex].PrimaryCard!;
            targetCardId = card2.InstanceId;
            ownerId = card2.ActiveData.Owner;
            weight = 1f * GetWeightValue(AiWeightType.DeployEmbarkWeight);
        }

        var action = new AiGameAction
        {
            ActionType = GameEvent.Deploy,
            SourceCardId = instanceId,
            TargetCardId = targetCardId,
            Hostile = ownerId != playerIndex,
            Area = area,
            Region = region,
            SlotIndex = slotIndex,
            PushDir = pushDir,
            Weight = cardWeight + weight
        };

        _actions.Add(action);
    }

    private void GenerateDeployTargets(Card card, CcgGameState game)
    {
        var gameIndex = _player.PlayerIndex;
        var regionEnum = (Region) gameIndex;
        var regionEnum2 = (Region) game.GetOpponentPlayerIndex(gameIndex);
        var instanceId = card.InstanceId;
        var type = card.GetTemplate().Type;
        AiGameAction? action;
        var cardWeight = CalculateCardWeight(card);
        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.FriendlyPerimeter, regionEnum, -1, 1))
        {
            action = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = 0,
                Hostile = false,
                Area = TargetableArea.FriendlyPerimeter,
                Region = regionEnum,
                SlotIndex = 0,
                PushDir = 1,
                Weight = cardWeight
            };
            _actions.Add(action);
        }

        if (type != CardType.BurnCard && type != CardType.Secret && game.CanDeploy(gameIndex, instanceId, TargetableArea.UnitStack, regionEnum, -1, 1))
        {
            var emptyCardStackIndex = game.Board.Regions[(uint)regionEnum].GetEmptyCardStackIndex(false, !card.GetTemplate().IsCombatUnit());
            action = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = 0,
                Hostile = false,
                Area = TargetableArea.UnitStack,
                Region = regionEnum,
                SlotIndex = (sbyte) emptyCardStackIndex,
                PushDir = 1,
                Weight = cardWeight
            };
            _actions.Add(action);
        }

        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.EnemyPerimeter, regionEnum2, -1, 1))
        {
            action = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = 0,
                Hostile = true,
                Area = TargetableArea.EnemyPerimeter,
                Region = regionEnum2,
                SlotIndex = 0,
                PushDir = 1,
                Weight = cardWeight
            };
            _actions.Add(action);
        }

        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.BattleField, Region.NumRegions, -1, 1))
        {
            action = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = 0,
                Hostile = true,
                Area = TargetableArea.BattleField,
                Region = Region.NumRegions,
                SlotIndex = 0,
                PushDir = 1,
                Weight = cardWeight
            };
            _actions.Add(action);
        }

        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.Frontline, Region.Control, -1, 1))
        {
            action = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = 0,
                Hostile = true,
                Area = TargetableArea.Frontline,
                Region = Region.Control,
                SlotIndex = 0,
                PushDir = 1,
                Weight = cardWeight
            };
            _actions.Add(action);
        }

        var slots = game.Board.Regions[2].Slots;
        for (sbyte slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            CheckDeployRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, Region.Control, slotIndex, 0, cardWeight, slots);
        }

        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.FriendlyCommander, Region.Player0, -1, 0))
        {
            action = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = _player.Commander.PrimaryCard!.InstanceId,
                Hostile = false,
                Area = TargetableArea.UnitStack,
                Region = Region.Control,
                SlotIndex = 0,
                PushDir = 0,
                Weight = cardWeight
            };
            _actions.Add(action);
        }

        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.EnemyCommander, Region.Player0, -1, 0))
        {
            var opponentPlayerIndex = game.GetOpponentPlayerIndex(gameIndex);
            action = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = game.Players[opponentPlayerIndex].Commander.PrimaryCard!.InstanceId,
                Hostile = false,
                Area = TargetableArea.UnitStack,
                Region = Region.Control,
                SlotIndex = 0,
                PushDir = 0,
                Weight = cardWeight
            };
            _actions.Add(action);
        }

        slots = game.Board.Regions[(uint) regionEnum].Slots;
        for (sbyte slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            if (slots[slotIndex].PrimaryCard != null)
            {
                CheckDeployRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, regionEnum, slotIndex, 0, cardWeight, slots);
            }
        }

        slots = game.Board.Regions[(uint) regionEnum2].Slots;
        for (sbyte slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            if (slots[slotIndex].PrimaryCard != null)
            {
                CheckDeployRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, regionEnum2, slotIndex, 0, cardWeight, slots);
            }
        }
    }

    private void CheckMoveRegionIndex(CcgGameState game, sbyte playerIndex, Card card, TargetableArea area,
        Region region, sbyte slotIndex, sbyte pushDir, float cardWeight, CardStack[]? slots)
    {
        var instanceId = card.InstanceId;
        var targetCardId = 0;
        if (slots?[slotIndex].PrimaryCard != null)
        {
            targetCardId = slots[slotIndex].PrimaryCard!.InstanceId;
            if (instanceId == targetCardId)
            {
                return;
            }
        }

        if (!game.CanMove(playerIndex, instanceId, region, slotIndex, pushDir))
        {
            return;
        }

        var action = new AiGameAction
        {
            ActionType = GameEvent.Move,
            SourceCardId = instanceId,
            TargetCardId = targetCardId,
            Hostile = false,
            Area = area,
            Region = region,
            SlotIndex = slotIndex,
            PushDir = pushDir,
            Weight = cardWeight
        };
        _actions.Add(action);
    }

    private void GenerateMoveTargets(Card card, CcgGameState game)
    {
        var gameIndex = _player.PlayerIndex;
        var traitActorRegion = game.GetTraitActorRegion(gameIndex, card.InstanceId);
        var regionEnum = (Region) gameIndex;
        var instanceId = card.InstanceId;
        var cardWeight = CalculateCardWeight(card);
        if (traitActorRegion == Region.Control && game.CanMove(gameIndex, instanceId, regionEnum, -1, 1))
        {
            var action = new AiGameAction
            {
                ActionType = GameEvent.Move,
                SourceCardId = instanceId,
                TargetCardId = 0,
                Hostile = false,
                Area = TargetableArea.FriendlyPerimeter,
                Region = regionEnum,
                SlotIndex = 0,
                PushDir = 1,
                Weight = cardWeight
            };
            _actions.Add(action);
        }

        var slots = game.Board.Regions[2].Slots;
        for (sbyte slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            CheckMoveRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, Region.Control, slotIndex, 0, cardWeight, slots);
        }

        slots = game.Board.Regions[(uint) regionEnum].Slots;
        for (sbyte slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            if (slots[slotIndex].PrimaryCard != null)
            {
                CheckMoveRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, regionEnum, slotIndex, 0, cardWeight, slots);
            }
        }
    }

    private void CheckAttackRegionIndex(CcgGameState game, sbyte playerIndex, Card card, sbyte slotIndex,
        CardStack[] slots)
    {
        var instanceId = card.InstanceId;
        if (slots[slotIndex].PrimaryCard == null)
        {
            return;
        }

        var targetCard = slots[slotIndex].PrimaryCard!;
        var targetCardId = targetCard.InstanceId;
        var ownerIndex = targetCard.ActiveData.Owner;
        if (playerIndex == ownerIndex || !game.CanAttack(playerIndex, instanceId, ownerIndex, targetCardId))
        {
            return;
        }

        var weight = CalculateCombatResultWeight(card, targetCard);
        var action = new AiGameAction
        {
            ActionType = GameEvent.Attack,
            SourceCardId = instanceId,
            TargetCardId = targetCardId,
            Hostile = true,
            Area = TargetableArea.AnyAreas,
            Region = Region.NumRegions,
            SlotIndex = slotIndex,
            PushDir = 0,
            Weight = weight
        };
        _actions.Add(action);
    }

    private float CalculateCombatResultWeight(Card card, Card targetCard)
    {
        var sourceCardWeight = CalculateCardWeight(card);
        var targetCardWeight = CalculateCardWeight(targetCard);
        var resultWeight = 1f;
        int attackAgainstTarget = card.GetCurrentAttack(targetCard, false);
        int targetDefense = targetCard.GetCurrentDefense(false);
        if (targetDefense > 0)
        {
            var bypassDefense = card.GetCurrentBypassDefense(targetCard, false);
            targetDefense -= bypassDefense;
            if (targetDefense < 0)
            {
                targetDefense = 0;
            }
        }

        attackAgainstTarget -= targetDefense;
        if (attackAgainstTarget >= targetCard.GetCurrentHealth(false))
        {
            resultWeight += targetCardWeight;
        }

        var attackAgainstAttacker = targetCard.GetCurrentAttack(card, false);
        var attackerDefense = card.GetCurrentDefense(false);
        if (attackerDefense > 0)
        {
            var bypassDefense = targetCard.GetCurrentBypassDefense(card, false);
            attackerDefense -= bypassDefense;
            if (attackerDefense < 0)
            {
                attackerDefense = 0;
            }
        }

        attackAgainstAttacker -= attackerDefense;
        if (attackAgainstAttacker >= card.GetCurrentHealth(false))
        {
            resultWeight -= sourceCardWeight;
        }

        if (resultWeight < 0f)
        {
            resultWeight = 0.01f;
        }

        return resultWeight;
    }

    private void GenerateAttackTargets(Card card, CcgGameState game)
    {
        var gameIndex = _player.PlayerIndex;
        var opponentPlayerIndex = game.GetOpponentPlayerIndex(gameIndex);
        var regionEnum = (Region) opponentPlayerIndex;
        var instanceId = card.InstanceId;
        var cardWeight = CalculateCardWeight(card);
        var slots = game.Board.Regions[2].Slots;
        for (sbyte slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            CheckAttackRegionIndex(game, gameIndex, card, slotIndex, slots);
        }

        slots = game.Board.Regions[(uint) regionEnum].Slots;
        for (sbyte slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            CheckAttackRegionIndex(game, gameIndex, card, slotIndex, slots);
        }

        var primaryCard = game.Players[opponentPlayerIndex].Commander.PrimaryCard!;
        if (!game.CanAttack(gameIndex, instanceId, opponentPlayerIndex, primaryCard.InstanceId))
        {
            return;
        }

        var action = new AiGameAction
        {
            ActionType = GameEvent.Attack,
            SourceCardId = instanceId,
            TargetCardId = primaryCard.InstanceId,
            Hostile = true,
            Area = TargetableArea.EnemyCommander,
            Region = Region.NumRegions,
            SlotIndex = 0,
            PushDir = 0,
            Weight = cardWeight + 1f * GetWeightValue(AiWeightType.DestroyCommanderWeight)
        };
        _actions.Add(action);
    }

    private float CalculateCardWeight(Card card)
    {
        if (card.GetTemplate().Type == CardType.BurnCard)
        {
            return 1f + card.GetTemplate().Cost;
        }

        var attackWeightValue = GetWeightValue(AiWeightType.AttackValueWeight);
        var armorWeightValue = GetWeightValue(AiWeightType.ArmorValueWeight);
        var healthWeightValue = GetWeightValue(AiWeightType.HealthValueWeight);

        attackWeightValue = card.GetCurrentAttack(null, false) * attackWeightValue;
        armorWeightValue = card.GetCurrentHealth(false) * armorWeightValue;
        healthWeightValue = card.GetCurrentDefense(false) * healthWeightValue;

        return attackWeightValue + armorWeightValue + healthWeightValue;
    }

    private float GetWeightValue(AiWeightType type)
    {
        foreach (var variable in _activeWeightValues)
        {
            if (variable.Type == type)
            {
                return variable.Value;
            }
        }

        return 1f;
    }
}