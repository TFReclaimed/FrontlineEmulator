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
        var aiGameAction = CreateEndTurnAction();
        _actions.Add(aiGameAction);
        var list = new List<AiGameAction>();
        for (var i = 0; i < _actions.Count; i++)
        {
            var aiGameAction2 = _actions[i];
            if (aiGameAction2.Weight < 0f)
            {
                aiGameAction2.Weight = 0f - aiGameAction2.Weight;
                list.Add(aiGameAction2);
            }
        }
        if (list.Count > 0)
        {
            _actions.Clear();
            _actions.AddRange(list);
        }
        var num = GetWeightValue(AiWeightType.AiWeightTolerance) + 5f;
        var num2 = 0f;
        var j = 0;
        _actions.Sort(AiGameAction.SortByWeight);
        if (num > 0f)
        {
            for (; j < _actions.Count && _actions[j].Weight + num >= _actions[0].Weight; j++)
            {
            }
        }
        else
        {
            j = _actions.Count;
        }
        if (j > 1)
        {
            for (var k = 0; k < j; k++)
            {
                num2 += _actions[k].Weight;
            }
            num2 = Range(0f, num2);
            j = 0;
            while (num2 > 0f)
            {
                num2 -= _actions[j].Weight;
                if (num2 > 0f)
                {
                    j++;
                }
            }
        }
        else
        {
            j = 0;
        }

        return _actions[j];
    }

    private static float Range(float minInclusive, float maxInclusive)
    {
        return Random.Shared.NextSingle() * (maxInclusive - minInclusive) + minInclusive;
    }

    private static AiGameAction CreateEndTurnAction()
    {
        var aIGameAction = new AiGameAction
        {
            ActionType = GameEvent.TriggerEndTurnTraits,
            Hostile = false,
            Weight = 1f
        };

        return aIGameAction;
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
        var array = game.Board.Regions[(uint) regionEnum].Slots;
        for (sbyte b = 0; b < array.Length; b++)
        {
            var card = array[b].PrimaryCard;
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
        var array = game.Board.Regions[(uint)regionEnum].Slots;
        for (sbyte b = 0; b < array.Length; b++)
        {
            var card = array[b].PrimaryCard;
            if (card != null)
            {
                GenerateAttackTargets(card, game);
            }
        }

        array = game.Board.Regions[2].Slots;
        for (sbyte b = 0; b < array.Length; b++)
        {
            var card = array[b].PrimaryCard;
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
        sbyte b = 0;
        var num = 0f;
        var instanceId = card.InstanceId;
        if (!game.CanDeploy(playerIndex, instanceId, area, region, slotIndex, 0))
        {
            return;
        }

        if (slots?[slotIndex].PrimaryCard != null)
        {
            var card2 = slots[slotIndex].PrimaryCard!;
            targetCardId = card2.InstanceId;
            b = card2.ActiveData.Owner;
            num = 1f * GetWeightValue(AiWeightType.DeployEmbarkWeight);
        }

        var aiGameAction = new AiGameAction
        {
            ActionType = GameEvent.Deploy,
            SourceCardId = instanceId,
            TargetCardId = targetCardId,
            Hostile = b != playerIndex,
            Area = area,
            Region = region,
            SlotIndex = slotIndex,
            PushDir = pushDir,
            Weight = cardWeight + num
        };

        _actions.Add(aiGameAction);
    }

    private void GenerateDeployTargets(Card card, CcgGameState game)
    {
        var gameIndex = _player.PlayerIndex;
        var regionEnum = (Region) gameIndex;
        var regionEnum2 = (Region) game.GetOpponentPlayerIndex(gameIndex);
        var instanceId = card.InstanceId;
        var type = card.GetTemplate().Type;
        sbyte b;
        AiGameAction? aiGameAction;
        var num = CalculateCardWeight(card);
        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.FriendlyPerimeter, regionEnum, -1, 1))
        {
            aiGameAction = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = 0,
                Hostile = false,
                Area = TargetableArea.FriendlyPerimeter,
                Region = regionEnum,
                SlotIndex = 0,
                PushDir = 1,
                Weight = num
            };
            _actions.Add(aiGameAction);
        }

        if (type != CardType.BurnCard && type != CardType.Secret && game.CanDeploy(gameIndex, instanceId, TargetableArea.UnitStack, regionEnum, -1, 1))
        {
            var emptyCardStackIndex = game.Board.Regions[(uint)regionEnum].GetEmptyCardStackIndex(false, !card.GetTemplate().IsCombatUnit());
            aiGameAction = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = 0,
                Hostile = false,
                Area = TargetableArea.UnitStack,
                Region = regionEnum,
                SlotIndex = (sbyte) emptyCardStackIndex,
                PushDir = 1,
                Weight = num
            };
            _actions.Add(aiGameAction);
        }

        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.EnemyPerimeter, regionEnum2, -1, 1))
        {
            aiGameAction = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = 0,
                Hostile = true,
                Area = TargetableArea.EnemyPerimeter,
                Region = regionEnum2,
                SlotIndex = 0,
                PushDir = 1,
                Weight = num
            };
            _actions.Add(aiGameAction);
        }

        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.BattleField, Region.NumRegions, -1, 1))
        {
            aiGameAction = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = 0,
                Hostile = true,
                Area = TargetableArea.BattleField,
                Region = Region.NumRegions,
                SlotIndex = 0,
                PushDir = 1,
                Weight = num
            };
            _actions.Add(aiGameAction);
        }

        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.Frontline, Region.Control, -1, 1))
        {
            aiGameAction = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = 0,
                Hostile = true,
                Area = TargetableArea.Frontline,
                Region = Region.Control,
                SlotIndex = 0,
                PushDir = 1,
                Weight = num
            };
            _actions.Add(aiGameAction);
        }

        var array = game.Board.Regions[2].Slots;
        for (b = 0; b < array.Length; b++)
        {
            CheckDeployRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, Region.Control, b, 0, num, array);
        }

        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.FriendlyCommander, Region.Player0, -1, 0))
        {
            aiGameAction = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = _player.Commander.PrimaryCard!.InstanceId,
                Hostile = false,
                Area = TargetableArea.UnitStack,
                Region = Region.Control,
                SlotIndex = 0,
                PushDir = 0,
                Weight = num
            };
            _actions.Add(aiGameAction);
        }

        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.EnemyCommander, Region.Player0, -1, 0))
        {
            var opponentPlayerIndex = game.GetOpponentPlayerIndex(gameIndex);
            aiGameAction = new AiGameAction
            {
                ActionType = GameEvent.Deploy,
                SourceCardId = instanceId,
                TargetCardId = game.Players[opponentPlayerIndex].Commander.PrimaryCard!.InstanceId,
                Hostile = false,
                Area = TargetableArea.UnitStack,
                Region = Region.Control,
                SlotIndex = 0,
                PushDir = 0,
                Weight = num
            };
            _actions.Add(aiGameAction);
        }

        array = game.Board.Regions[(uint) regionEnum].Slots;
        for (b = 0; b < array.Length; b++)
        {
            if (array[b].PrimaryCard != null)
            {
                CheckDeployRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, regionEnum, b, 0, num, array);
            }
        }

        array = game.Board.Regions[(uint) regionEnum2].Slots;
        for (b = 0; b < array.Length; b++)
        {
            if (array[b].PrimaryCard != null)
            {
                CheckDeployRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, regionEnum2, b, 0, num, array);
            }
        }
    }

    private void CheckMoveRegionIndex(CcgGameState game, sbyte playerIndex, Card card, TargetableArea area,
        Region region, sbyte slotIndex, sbyte pushDir, float cardWeight, CardStack[]? slots)
    {
        var instanceId = card.InstanceId;
        var num = 0;
        if (slots?[slotIndex].PrimaryCard != null)
        {
            num = slots[slotIndex].PrimaryCard!.InstanceId;
            if (instanceId == num)
            {
                return;
            }
        }

        if (!game.CanMove(playerIndex, instanceId, region, slotIndex, pushDir))
        {
            return;
        }

        var aiGameAction = new AiGameAction
        {
            ActionType = GameEvent.Move,
            SourceCardId = instanceId,
            TargetCardId = num,
            Hostile = false,
            Area = area,
            Region = region,
            SlotIndex = slotIndex,
            PushDir = pushDir,
            Weight = cardWeight
        };
        _actions.Add(aiGameAction);
    }

    private void GenerateMoveTargets(Card card, CcgGameState game)
    {
        var gameIndex = _player.PlayerIndex;
        var traitActorRegion = game.GetTraitActorRegion(gameIndex, card.InstanceId);
        var regionEnum = (Region) gameIndex;
        var instanceId = card.InstanceId;
        sbyte b;
        var num = CalculateCardWeight(card);
        if (traitActorRegion == Region.Control && game.CanMove(gameIndex, instanceId, regionEnum, -1, 1))
        {
            var aiGameAction = new AiGameAction
            {
                ActionType = GameEvent.Move,
                SourceCardId = instanceId,
                TargetCardId = 0,
                Hostile = false,
                Area = TargetableArea.FriendlyPerimeter,
                Region = regionEnum,
                SlotIndex = 0,
                PushDir = 1,
                Weight = num
            };
            _actions.Add(aiGameAction);
        }

        var array = game.Board.Regions[2].Slots;
        for (b = 0; b < array.Length; b++)
        {
            CheckMoveRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, Region.Control, b, 0, num, array);
        }

        array = game.Board.Regions[(uint) regionEnum].Slots;
        for (b = 0; b < array.Length; b++)
        {
            if (array[b].PrimaryCard != null)
            {
                CheckMoveRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, regionEnum, b, 0, num, array);
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

        var card2 = slots[slotIndex].PrimaryCard!;
        var num = card2.InstanceId;
        var b = card2.ActiveData.Owner;
        if (playerIndex == b || !game.CanAttack(playerIndex, instanceId, b, num))
        {
            return;
        }

        var num2 = CalculateCombatResultWeight(card, card2);
        var aIGameAction = new AiGameAction
        {
            ActionType = GameEvent.Attack,
            SourceCardId = instanceId,
            TargetCardId = num,
            Hostile = true,
            Area = TargetableArea.AnyAreas,
            Region = Region.NumRegions,
            SlotIndex = slotIndex,
            PushDir = 0,
            Weight = num2
        };
        _actions.Add(aIGameAction);
    }

    private float CalculateCombatResultWeight(Card card, Card targetCard)
    {
        var num = CalculateCardWeight(card);
        var num2 = CalculateCardWeight(targetCard);
        var num3 = 1f;
        int num4 = card.GetCurrentAttack(targetCard, false);
        int num5 = targetCard.GetCurrentDefense(false);
        if (num5 > 0)
        {
            var num6 = card.GetCurrentBypassDefense(targetCard, false);
            num5 -= num6;
            if (num5 < 0)
            {
                num5 = 0;
            }
        }

        num4 -= num5;
        if (num4 >= targetCard.GetCurrentHealth(false))
        {
            num3 += num2;
        }

        num4 = targetCard.GetCurrentAttack(card, false);
        num5 = card.GetCurrentDefense(false);
        if (num5 > 0)
        {
            var num6 = targetCard.GetCurrentBypassDefense(card, false);
            num5 -= num6;
            if (num5 < 0)
            {
                num5 = 0;
            }
        }

        num4 -= num5;
        if (num4 >= card.GetCurrentHealth(false))
        {
            num3 -= num;
        }

        if (num3 < 0f)
        {
            num3 = 0.01f;
        }

        return num3;
    }

    private void GenerateAttackTargets(Card card, CcgGameState game)
    {
        var gameIndex = _player.PlayerIndex;
        var opponentPlayerIndex = game.GetOpponentPlayerIndex(gameIndex);
        var regionEnum = (Region) opponentPlayerIndex;
        var instanceId = card.InstanceId;
        sbyte b;
        var num = CalculateCardWeight(card);
        var array = game.Board.Regions[2].Slots;
        for (b = 0; b < array.Length; b++)
        {
            CheckAttackRegionIndex(game, gameIndex, card, b, array);
        }

        array = game.Board.Regions[(uint) regionEnum].Slots;
        for (b = 0; b < array.Length; b++)
        {
            CheckAttackRegionIndex(game, gameIndex, card, b, array);
        }

        var primaryCard = game.Players[opponentPlayerIndex].Commander.PrimaryCard!;
        if (!game.CanAttack(gameIndex, instanceId, opponentPlayerIndex, primaryCard.InstanceId))
        {
            return;
        }

        var aiGameAction = new AiGameAction
        {
            ActionType = GameEvent.Attack,
            SourceCardId = instanceId,
            TargetCardId = primaryCard.InstanceId,
            Hostile = true,
            Area = TargetableArea.EnemyCommander,
            Region = Region.NumRegions,
            SlotIndex = 0,
            PushDir = 0,
            Weight = num + 1f * GetWeightValue(AiWeightType.DestroyCommanderWeight)
        };
        _actions.Add(aiGameAction);
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