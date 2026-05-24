using Frontline.Battle.Data.Card;

namespace Frontline.Battle.Ai;

public class AiBrain
{
    private readonly AiProfile _profile;

    private readonly Player _player;

    private List<AiWeightVariable>? _activeWeightValues;

    private List<AiGameAction> _actions = [];

    public AiBrain(AiProfile data, Player player)
    {
        _profile = data;
        _player = player;
        /*if (_profile.baseWeightValues != null)
        {
            _activeWeightValues = new List<AiWeightVariable>(_profile.baseWeightValues.Length);
            _activeWeightValues.AddRange(_profile.baseWeightValues);
        }*/
    }

    public AiGameAction CalculateNextAction(CcgGameState game)
    {
        AiGameAction aIGameAction = null;
        _actions.Clear();
        CalculateDeployActions(game);
        CalculateMoveActions(game);
        CalculateAttackActions(game);
        aIGameAction = CreateEndTurnAction();
        _actions.Add(aIGameAction);
        List<AiGameAction> list = new List<AiGameAction>();
        AiGameAction aIGameAction2 = null;
        for (int i = 0; i < _actions.Count; i++)
        {
            aIGameAction2 = _actions[i];
            if (aIGameAction2.Weight < 0f)
            {
                aIGameAction2.Weight = 0f - aIGameAction2.Weight;
                list.Add(aIGameAction2);
            }
        }
        if (list.Count > 0)
        {
            _actions.Clear();
            _actions.AddRange(list);
        }
        float num = GetWeightValue(AiWeightType.AiWeightTolerance) + 5f;
        float num2 = 0f;
        int j = 0;
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
            for (int k = 0; k < j; k++)
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

    private AiGameAction CreateEndTurnAction()
    {
        AiGameAction aIGameAction = new AiGameAction();
        aIGameAction.ActionType = GameEvent.TriggerEndTurnTraits;
        aIGameAction.Hostile = false;
        aIGameAction.Weight = 1f;
        return aIGameAction;
    }

    private void CalculateDeployActions(CcgGameState game)
    {
        Card card = null;
        int count = _player.Hand.Cards.Count;
        for (int i = 0; i < count; i++)
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
        sbyte gameIndex = _player.PlayerIndex;
        Region regionEnum = (Region)gameIndex;
        sbyte b = 0;
        CardStack[] array = null;
        Card card = null;
        array = game.Board.Regions[(uint)regionEnum].Slots;
        for (b = 0; b < array.Length; b++)
        {
            card = array[b].PrimaryCard;
            if (card != null)
            {
                GenerateMoveTargets(card, game);
            }
        }
    }

    private void CalculateAttackActions(CcgGameState game)
    {
        sbyte gameIndex = _player.PlayerIndex;
        Region regionEnum = (Region)gameIndex;
        sbyte b = 0;
        CardStack[] array = null;
        Card card = null;
        array = game.Board.Regions[(uint)regionEnum].Slots;
        for (b = 0; b < array.Length; b++)
        {
            card = array[b].PrimaryCard;
            if (card != null)
            {
                GenerateAttackTargets(card, game);
            }
        }
        array = game.Board.Regions[2].Slots;
        for (b = 0; b < array.Length; b++)
        {
            card = array[b].PrimaryCard;
            if (card != null && card.ActiveData.Owner == gameIndex)
            {
                GenerateAttackTargets(card, game);
            }
        }
    }

    private void CheckDeloyRegionIndex(CcgGameState game, sbyte playerIndex, Card card, TargetableArea area, Region region, sbyte slotIndex, sbyte pushDir, float cardWeight, CardStack[] slots)
    {
        int targetCardID = 0;
        sbyte b = 0;
        Card card2 = null;
        float num = 0f;
        int instanceId = card.InstanceId;
        if (game.CanDeploy(playerIndex, instanceId, area, region, slotIndex, 0))
        {
            if (slots != null && slots[slotIndex].PrimaryCard != null)
            {
                card2 = slots[slotIndex].PrimaryCard;
                targetCardID = card2.InstanceId;
                b = card2.ActiveData.Owner;
                num = 1f * GetWeightValue(AiWeightType.DeployEmbarkWeight);
            }
            AiGameAction aIGameAction = new AiGameAction();
            aIGameAction.ActionType = GameEvent.Deploy;
            aIGameAction.SourceCardId = instanceId;
            aIGameAction.TargetCardId = targetCardID;
            aIGameAction.Hostile = b != playerIndex;
            aIGameAction.Area = area;
            aIGameAction.Region = region;
            aIGameAction.SlotIndex = slotIndex;
            aIGameAction.PushDir = pushDir;
            aIGameAction.Weight = cardWeight + num;
            _actions.Add(aIGameAction);
        }
    }

    private void GenerateDeployTargets(Card card, CcgGameState game)
    {
        sbyte gameIndex = _player.PlayerIndex;
        Region regionEnum = (Region)gameIndex;
        Region regionEnum2 = (Region)game.GetOpponentPlayerIndex(gameIndex);
        int instanceId = card.InstanceId;
        CardType type = card.GetTemplate().Type;
        sbyte b = 0;
        CardStack[] array = null;
        AiGameAction aIGameAction = null;
        float num = CalculateCardWeight(card);
        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.FriendlyPerimeter, regionEnum, -1, 1))
        {
            aIGameAction = new AiGameAction();
            aIGameAction.ActionType = GameEvent.Deploy;
            aIGameAction.SourceCardId = instanceId;
            aIGameAction.TargetCardId = 0;
            aIGameAction.Hostile = false;
            aIGameAction.Area = TargetableArea.FriendlyPerimeter;
            aIGameAction.Region = regionEnum;
            aIGameAction.SlotIndex = 0;
            aIGameAction.PushDir = 1;
            aIGameAction.Weight = num;
            _actions.Add(aIGameAction);
        }
        if (type != CardType.BurnCard && type != CardType.Secret && game.CanDeploy(gameIndex, instanceId, TargetableArea.UnitStack, regionEnum, -1, 1))
        {
            int emptyCardStackIndex = game.Board.Regions[(uint)regionEnum].GetEmptyCardStackIndex(false, !card.GetTemplate().IsCombatUnit());
            aIGameAction = new AiGameAction();
            aIGameAction.ActionType = GameEvent.Deploy;
            aIGameAction.SourceCardId = instanceId;
            aIGameAction.TargetCardId = 0;
            aIGameAction.Hostile = false;
            aIGameAction.Area = TargetableArea.UnitStack;
            aIGameAction.Region = regionEnum;
            aIGameAction.SlotIndex = (sbyte)emptyCardStackIndex;
            aIGameAction.PushDir = 1;
            aIGameAction.Weight = num;
            _actions.Add(aIGameAction);
        }
        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.EnemyPerimeter, regionEnum2, -1, 1))
        {
            aIGameAction = new AiGameAction();
            aIGameAction.ActionType = GameEvent.Deploy;
            aIGameAction.SourceCardId = instanceId;
            aIGameAction.TargetCardId = 0;
            aIGameAction.Hostile = true;
            aIGameAction.Area = TargetableArea.EnemyPerimeter;
            aIGameAction.Region = regionEnum2;
            aIGameAction.SlotIndex = 0;
            aIGameAction.PushDir = 1;
            aIGameAction.Weight = num;
            _actions.Add(aIGameAction);
        }
        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.BattleField, Region.NumRegions, -1, 1))
        {
            aIGameAction = new AiGameAction();
            aIGameAction.ActionType = GameEvent.Deploy;
            aIGameAction.SourceCardId = instanceId;
            aIGameAction.TargetCardId = 0;
            aIGameAction.Hostile = true;
            aIGameAction.Area = TargetableArea.BattleField;
            aIGameAction.Region = Region.NumRegions;
            aIGameAction.SlotIndex = 0;
            aIGameAction.PushDir = 1;
            aIGameAction.Weight = num;
            _actions.Add(aIGameAction);
        }
        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.Frontline, Region.Control, -1, 1))
        {
            aIGameAction = new AiGameAction();
            aIGameAction.ActionType = GameEvent.Deploy;
            aIGameAction.SourceCardId = instanceId;
            aIGameAction.TargetCardId = 0;
            aIGameAction.Hostile = true;
            aIGameAction.Area = TargetableArea.Frontline;
            aIGameAction.Region = Region.Control;
            aIGameAction.SlotIndex = 0;
            aIGameAction.PushDir = 1;
            aIGameAction.Weight = num;
            _actions.Add(aIGameAction);
        }
        array = game.Board.Regions[2].Slots;
        for (b = 0; b < array.Length; b++)
        {
            CheckDeloyRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, Region.Control, b, 0, num, array);
        }
        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.FriendlyCommander, Region.Player0, -1, 0))
        {
            aIGameAction = new AiGameAction();
            aIGameAction.ActionType = GameEvent.Deploy;
            aIGameAction.SourceCardId = instanceId;
            aIGameAction.TargetCardId = _player.Commander.PrimaryCard.InstanceId;
            aIGameAction.Hostile = false;
            aIGameAction.Area = TargetableArea.UnitStack;
            aIGameAction.Region = Region.Control;
            aIGameAction.SlotIndex = 0;
            aIGameAction.PushDir = 0;
            aIGameAction.Weight = num;
            _actions.Add(aIGameAction);
        }
        if (game.CanDeploy(gameIndex, instanceId, TargetableArea.EnemyCommander, Region.Player0, -1, 0))
        {
            aIGameAction = new AiGameAction();
            sbyte opponentPlayerIndex = game.GetOpponentPlayerIndex(gameIndex);
            aIGameAction.ActionType = GameEvent.Deploy;
            aIGameAction.SourceCardId = instanceId;
            aIGameAction.TargetCardId = game.Players[opponentPlayerIndex].Commander.PrimaryCard.InstanceId;
            aIGameAction.Hostile = false;
            aIGameAction.Area = TargetableArea.UnitStack;
            aIGameAction.Region = Region.Control;
            aIGameAction.SlotIndex = 0;
            aIGameAction.PushDir = 0;
            aIGameAction.Weight = num;
            _actions.Add(aIGameAction);
        }
        array = game.Board.Regions[(uint)regionEnum].Slots;
        for (b = 0; b < array.Length; b++)
        {
            if (array[b].PrimaryCard != null)
            {
                CheckDeloyRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, regionEnum, b, 0, num, array);
            }
        }
        array = game.Board.Regions[(uint)regionEnum2].Slots;
        for (b = 0; b < array.Length; b++)
        {
            if (array[b].PrimaryCard != null)
            {
                CheckDeloyRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, regionEnum2, b, 0, num, array);
            }
        }
    }

    private void CheckMoveRegionIndex(CcgGameState game, sbyte playerIndex, Card card, TargetableArea area, Region region, sbyte slotIndex, sbyte pushDir, float cardWeight, CardStack[] slots)
    {
        int instanceId = card.InstanceId;
        int num = 0;
        if (slots != null && slots[slotIndex].PrimaryCard != null)
        {
            num = slots[slotIndex].PrimaryCard.InstanceId;
            if (instanceId == num)
            {
                return;
            }
        }
        if (game.CanMove(playerIndex, instanceId, region, slotIndex, pushDir))
        {
            AiGameAction aIGameAction = new AiGameAction();
            aIGameAction.ActionType = GameEvent.Move;
            aIGameAction.SourceCardId = instanceId;
            aIGameAction.TargetCardId = num;
            aIGameAction.Hostile = false;
            aIGameAction.Area = area;
            aIGameAction.Region = region;
            aIGameAction.SlotIndex = slotIndex;
            aIGameAction.PushDir = pushDir;
            aIGameAction.Weight = cardWeight;
            _actions.Add(aIGameAction);
        }
    }

    private void GenerateMoveTargets(Card card, CcgGameState game)
    {
        sbyte gameIndex = _player.PlayerIndex;
        Region traitActorRegion = game.GetTraitActorRegion(gameIndex, card.InstanceId);
        Region regionEnum = (Region)gameIndex;
        int instanceId = card.InstanceId;
        sbyte b = 0;
        CardStack[] array = null;
        AiGameAction aIGameAction = null;
        float num = CalculateCardWeight(card);
        if (traitActorRegion == Region.Control && game.CanMove(gameIndex, instanceId, regionEnum, -1, 1))
        {
            aIGameAction = new AiGameAction();
            aIGameAction.ActionType = GameEvent.Move;
            aIGameAction.SourceCardId = instanceId;
            aIGameAction.TargetCardId = 0;
            aIGameAction.Hostile = false;
            aIGameAction.Area = TargetableArea.FriendlyPerimeter;
            aIGameAction.Region = regionEnum;
            aIGameAction.SlotIndex = 0;
            aIGameAction.PushDir = 1;
            aIGameAction.Weight = num;
            _actions.Add(aIGameAction);
        }
        array = game.Board.Regions[2].Slots;
        for (b = 0; b < array.Length; b++)
        {
            CheckMoveRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, Region.Control, b, 0, num, array);
        }
        array = game.Board.Regions[(uint)regionEnum].Slots;
        for (b = 0; b < array.Length; b++)
        {
            if (array[b].PrimaryCard != null)
            {
                CheckMoveRegionIndex(game, gameIndex, card, TargetableArea.UnitStack, regionEnum, b, 0, num, array);
            }
        }
    }

    private void CheckAttackRegionIndex(CcgGameState game, sbyte playerIndex, Card card, sbyte slotIndex, CardStack[] slots)
    {
        var instanceId = card.InstanceId;
        if (slots[slotIndex].PrimaryCard == null)
        {
            return;
        }

        var card2 = slots[slotIndex].PrimaryCard;
        var num = card2.InstanceId;
        var b = card2.ActiveData.Owner;
        if (playerIndex == b || !game.CanAttack(playerIndex, instanceId, b, num))
        {
            return;
        }

        var aIGameAction = new AiGameAction();
        var num2 = CalculateCombatResultWeight(card, card2);
        aIGameAction.ActionType = GameEvent.Attack;
        aIGameAction.SourceCardId = instanceId;
        aIGameAction.TargetCardId = num;
        aIGameAction.Hostile = true;
        aIGameAction.Area = TargetableArea.AnyAreas;
        aIGameAction.Region = Region.NumRegions;
        aIGameAction.SlotIndex = slotIndex;
        aIGameAction.PushDir = 0;
        aIGameAction.Weight = num2;
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
        sbyte gameIndex = _player.PlayerIndex;
        sbyte opponentPlayerIndex = game.GetOpponentPlayerIndex(gameIndex);
        Region regionEnum = (Region)opponentPlayerIndex;
        int instanceId = card.InstanceId;
        sbyte b = 0;
        CardStack[] array = null;
        float num = CalculateCardWeight(card);
        array = game.Board.Regions[2].Slots;
        for (b = 0; b < array.Length; b++)
        {
            CheckAttackRegionIndex(game, gameIndex, card, b, array);
        }
        array = game.Board.Regions[(uint)regionEnum].Slots;
        for (b = 0; b < array.Length; b++)
        {
            CheckAttackRegionIndex(game, gameIndex, card, b, array);
        }
        Card primaryCard = game.Players[opponentPlayerIndex].Commander.PrimaryCard;
        if (game.CanAttack(gameIndex, instanceId, opponentPlayerIndex, primaryCard.InstanceId))
        {
            AiGameAction aIGameAction = new AiGameAction();
            aIGameAction.ActionType = GameEvent.Attack;
            aIGameAction.SourceCardId = instanceId;
            aIGameAction.TargetCardId = primaryCard.InstanceId;
            aIGameAction.Hostile = true;
            aIGameAction.Area = TargetableArea.EnemyCommander;
            aIGameAction.Region = Region.NumRegions;
            aIGameAction.SlotIndex = 0;
            aIGameAction.PushDir = 0;
            aIGameAction.Weight = num + 1f * GetWeightValue(AiWeightType.DestroyCommanderWeight);
            _actions.Add(aIGameAction);
        }
    }

    private float CalculateCardWeight(Card card)
    {
        if (card.GetTemplate().Type == CardType.BurnCard)
        {
            return 1f + card.GetTemplate().Cost;
        }

        var weightValue = GetWeightValue(AiWeightType.AttackValueWeight);
        var weightValue2 = GetWeightValue(AiWeightType.ArmorValueWeight);
        var weightValue3 = GetWeightValue(AiWeightType.HealthValueWeight);

        weightValue = card.GetCurrentAttack(null, false) * weightValue;
        weightValue2 = card.GetCurrentHealth(false) * weightValue2;
        weightValue3 = card.GetCurrentDefense(false) * weightValue3;

        return weightValue + weightValue2 + weightValue3;
    }

    private float GetWeightValue(AiWeightType type)
    {
        if (_activeWeightValues == null)
        {
            return 1f;
        }

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