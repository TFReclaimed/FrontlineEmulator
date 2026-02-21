using Frontline.Game.Card;

namespace Frontline.Battle;

public class TraitTargeting
{
    public TraitTargetType type = TraitTargetType.AnyType;

    public TargetTypeMod mod = TargetTypeMod.NumMods;

    public TraitTargetScope scope = TraitTargetScope.AnyScope;

    public TargetableArea area = TargetableArea.AnyAreas;

    public int targetID;

    public bool CheckFriendly()
    {
        return scope == TraitTargetScope.AnyScope || scope == TraitTargetScope.AllFriendly ||
               scope == TraitTargetScope.AllFriendlyNotSelf || scope == TraitTargetScope.FriendlyUnit ||
               scope == TraitTargetScope.FriendlyUnitNotSelf || scope == TraitTargetScope.RandomFriendly ||
               scope == TraitTargetScope.RandomFriendlyNotSelf || scope == TraitTargetScope.Self ||
               scope == TraitTargetScope.UnitStack;
    }

    public bool CheckEnemy()
    {
        return scope == TraitTargetScope.AnyScope || scope == TraitTargetScope.AllEnemy ||
               scope == TraitTargetScope.RandomEnemy || scope == TraitTargetScope.EnemyUnit ||
               scope == TraitTargetScope.UnitStack;
    }

    public bool HasAreaTarget()
    {
        return scope == TraitTargetScope.AnyScope || scope == TraitTargetScope.AllEnemy ||
               scope == TraitTargetScope.AllFriendly || scope == TraitTargetScope.RandomEnemy ||
               scope == TraitTargetScope.RandomFriendly || scope == TraitTargetScope.AllFriendlyNotSelf;
    }

    public bool CheckRegion(RegionEnum checkRegion, sbyte owner)
    {
        if (checkRegion != RegionEnum.NumRegions)
        {
            RegionEnum regionEnum = (RegionEnum) (0 + (byte) owner);
            switch (area)
            {
                case TargetableArea.AnyAreas:
                    return true;
                case TargetableArea.AnyRegion:
                    return true;
                case TargetableArea.BattleField:
                    return true;
                case TargetableArea.BattleFieldNC:
                    return true;
                case TargetableArea.FriendlyHand:
                    return true;
                case TargetableArea.FriendlyDiscard:
                    return true;
                case TargetableArea.EnemyHand:
                    return true;
                case TargetableArea.EnemyDiscard:
                    return true;
                case TargetableArea.Frontline:
                    if (checkRegion == RegionEnum.Control)
                    {
                        return true;
                    }

                    break;
                case TargetableArea.FriendlyPerimeter:
                    if (checkRegion == regionEnum)
                    {
                        return true;
                    }

                    break;
                case TargetableArea.EnemyPerimeter:
                    if (checkRegion != RegionEnum.Control && checkRegion != regionEnum)
                    {
                        return true;
                    }

                    break;
                case TargetableArea.FriendlyRegions:
                    if (checkRegion == regionEnum || checkRegion == RegionEnum.Control)
                    {
                        return true;
                    }

                    break;
                case TargetableArea.EnemyRegions:
                    if (checkRegion == RegionEnum.Control || checkRegion != regionEnum)
                    {
                        return true;
                    }

                    break;
            }

            return false;
        }

        return true;
    }

    public bool CardTargetMatch(CCG gameState, Card card, Card source)
    {
        CardStack cardStack = null;
        List<CardStack> list = gameState.FindCardStack(card);
        if (list != null && list.Count > 0)
        {
            cardStack = list[0];
        }

        switch (scope)
        {
            case TraitTargetScope.Self:
                if (!card.EqualsTo(source))
                {
                    return false;
                }

                break;
            case TraitTargetScope.UnitStack:
                if (cardStack == null || !cardStack.HasCard(source))
                {
                    return false;
                }

                break;
            case TraitTargetScope.FriendlyUnit:
            case TraitTargetScope.AllFriendly:
                if (card.activeData.owner != source.activeData.owner)
                {
                    return false;
                }

                break;
            case TraitTargetScope.FriendlyUnitNotSelf:
            case TraitTargetScope.AllFriendlyNotSelf:
            case TraitTargetScope.RandomFriendlyNotSelf:
                if (card.EqualsTo(source) || card.activeData.owner != source.activeData.owner)
                {
                    return false;
                }

                break;
            case TraitTargetScope.EnemyUnit:
            case TraitTargetScope.AllEnemy:
                if (card.activeData.owner == source.activeData.owner)
                {
                    return false;
                }

                break;
        }

        RegionEnum traitActorRegion = gameState.GetTraitActorRegion(card.activeData.owner, card.instanceId);
        if (area == TargetableArea.CurrentRegion)
        {
            if (traitActorRegion != gameState.GetTraitActorRegion(source.activeData.owner, source.instanceId))
            {
                return false;
            }
        }
        else if (area == TargetableArea.BattleFieldNC)
        {
            if (card.GetTemplate().Type == CardType.Commander)
            {
                return false;
            }
        }
        else if (area == TargetableArea.FriendlyCommander)
        {
            if (card.GetTemplate().Type != CardType.Commander || card.activeData.owner != source.activeData.owner)
            {
                return false;
            }
        }
        else if (area == TargetableArea.EnemyCommander)
        {
            if (card.GetTemplate().Type != CardType.Commander || card.activeData.owner == source.activeData.owner)
            {
                return false;
            }
        }
        else if (!CheckRegion(traitActorRegion, source.activeData.owner))
        {
            return false;
        }

        if (!DoesMatchType(card))
        {
            return false;
        }

        return true;
    }

    public bool DoesMatchType(Card card)
    {
        return DoesMatchType(type, mod, targetID, card);
    }

    public static bool DoesMatchType(TraitTargetType type, TargetTypeMod mod, int targetID, Card card)
    {
        if (type == TraitTargetType.AnyType && mod == TargetTypeMod.NumMods)
        {
            return true;
        }

        if (card == null)
        {
            return false;
        }

        if (card.GetTemplate() == null)
        {
            return false;
        }

        CardType cardType = card.GetTemplate().Type;
        UnitType unitType = card.GetUnitType();
        switch (type)
        {
            case TraitTargetType.Pilot:
                if (cardType != 0)
                {
                    return false;
                }

                if (mod == TargetTypeMod.EmbarkedPilot)
                {
                    UnitCard unitCard = (UnitCard) card;
                    if (!unitCard.pilotEmbarked)
                    {
                        return false;
                    }
                }

                break;
            case TraitTargetType.Titan:
                if (cardType != CardType.Titan)
                {
                    return false;
                }

                switch (mod)
                {
                    case TargetTypeMod.Piloted:
                    {
                        UnitCard unitCard3 = (UnitCard) card;
                        if (unitCard3.embarkedPilot == null)
                        {
                            return false;
                        }

                        break;
                    }
                    case TargetTypeMod.NotPiloted:
                    {
                        UnitCard unitCard2 = (UnitCard) card;
                        if (unitCard2.embarkedPilot != null)
                        {
                            return false;
                        }

                        break;
                    }
                }

                break;
            case TraitTargetType.Support:
                if (cardType != CardType.Support)
                {
                    return false;
                }

                break;
            case TraitTargetType.BurnCard:
                if (cardType != CardType.BurnCard)
                {
                    return false;
                }

                break;
            case TraitTargetType.Secret:
                if (cardType != CardType.Secret)
                {
                    return false;
                }

                break;
            case TraitTargetType.Commander:
                if (cardType != CardType.Commander)
                {
                    return false;
                }

                break;
            case TraitTargetType.Hard:
                if (!card.GetTemplate().IsHard)
                {
                    return false;
                }

                break;
            case TraitTargetType.Soft:
                if (card.GetTemplate().IsHard)
                {
                    return false;
                }

                break;
            case TraitTargetType.Light:
                if (unitType != UnitType.Light)
                {
                    return false;
                }

                break;
            case TraitTargetType.Medium:
                if (unitType != UnitType.Medium)
                {
                    return false;
                }

                break;
            case TraitTargetType.Heavy:
                if (unitType != UnitType.Heavy)
                {
                    return false;
                }

                break;
            case TraitTargetType.Stryder:
                if (unitType != UnitType.Stryder)
                {
                    return false;
                }

                break;
            case TraitTargetType.Atlas:
                if (unitType != UnitType.Atlas)
                {
                    return false;
                }

                break;
            case TraitTargetType.Ogre:
                if (unitType != UnitType.Ogre)
                {
                    return false;
                }

                break;
            case TraitTargetType.Spectre:
                if (unitType != UnitType.Spectre)
                {
                    return false;
                }

                break;
            case TraitTargetType.Installation:
                if (unitType != UnitType.Installation)
                {
                    return false;
                }

                break;
            case TraitTargetType.CardID:
                if (card.GetTemplate().CardId != targetID)
                {
                    return false;
                }

                break;
            default:
                return false;
            case TraitTargetType.AnyType:
                break;
        }

        switch (mod)
        {
            case TargetTypeMod.HasIntercept:
            {
                for (int num = card.activeData.activeTraits.Count - 1; num >= 0; num--)
                {
                    ActiveTrait activeTrait = card.activeData.activeTraits[num];
                    if (activeTrait.GetTraitInfo().IsIntercept(activeTrait))
                    {
                        return true;
                    }
                }

                return false;
            }
            case TargetTypeMod.HasStealth:
            {
                for (int num2 = card.activeData.activeTraits.Count - 1; num2 >= 0; num2--)
                {
                    ActiveTrait activeTrait2 = card.activeData.activeTraits[num2];
                    if (activeTrait2.GetTraitInfo().IsCombatManipulationPassive(1, activeTrait2))
                    {
                        return true;
                    }
                }

                return false;
            }
            case TargetTypeMod.NotInstallation:
                if (unitType == UnitType.Installation)
                {
                    return false;
                }

                break;
            case TargetTypeMod.IsWounded:
                if (card.GetTemplate().Type != CardType.BurnCard && card.GetTemplate().Type != CardType.Secret)
                {
                    sbyte maxModHealth = card.GetMaxModHealth();
                    if (card.GetCurrentHealth(false) < maxModHealth)
                    {
                        return true;
                    }
                }

                return false;
            case TargetTypeMod.IsStunned:
                if (card.HasStatusEffect(1))
                {
                    return true;
                }

                return false;
            case TargetTypeMod.IsDetered:
                if (card.HasStatusEffect(2))
                {
                    return true;
                }

                return false;
            case TargetTypeMod.IsImmobalized:
                if (card.HasStatusEffect(4))
                {
                    return true;
                }

                return false;
            case TargetTypeMod.IsActive:
                if (card.HasAnyActionsAvailable())
                {
                    return true;
                }

                return false;
            case TargetTypeMod.NotActive:
                if (card.HasAnyActionsAvailable())
                {
                    return false;
                }

                return true;
            case TargetTypeMod.HasAttack:
                if (card.HasAttack())
                {
                    return true;
                }

                return false;
        }

        return true;
    }

    public int CalculateCount(CCG gameState, ActiveTrait active)
    {
        int num = 0;
        RegionEnum region = RegionEnum.NumRegions;
        if (area == TargetableArea.CurrentRegion)
        {
            region = gameState.GetTraitActorRegion(active.GetTraitTarget().activeData.owner,
                active.GetTraitTarget().instanceId);
        }

        List<CardStack> list = gameState.FindCards(this, region, active.GetTraitSource());
        Card card = null;
        List<Card> list2 = null;
        for (int i = 0; i < list.Count; i++)
        {
            card = list[i].primaryCard;
            if (DoesMatchType(card))
            {
                num++;
            }

            list2 = card.GetSecrets();
            if (list2 != null)
            {
                for (int j = 0; j < list2.Count; j++)
                {
                    if (DoesMatchType(list2[j]))
                    {
                        num++;
                    }
                }
            }

            if (card.HasPilot() && DoesMatchType(card.GetEmbarkedPilot()))
            {
                num++;
            }
        }

        return num;
    }
}