using System.Text.Json.Serialization;
using Frontline.Battle.Traits;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class TraitTargeting
{
    public TraitTargetType Type { get; set; } = TraitTargetType.AnyType;

    public TargetTypeMod Mod { get; set; } = TargetTypeMod.NumMods;

    public TraitTargetScope Scope { get; set; } = TraitTargetScope.AnyScope;

    public TargetableArea Area { get; set; } = TargetableArea.AnyAreas;

    [JsonPropertyName("targetID")]
    public int TargetId { get; set; }

    public bool CheckFriendly()
    {
        return Scope == TraitTargetScope.AnyScope || Scope == TraitTargetScope.AllFriendly ||
               Scope == TraitTargetScope.AllFriendlyNotSelf || Scope == TraitTargetScope.FriendlyUnit ||
               Scope == TraitTargetScope.FriendlyUnitNotSelf || Scope == TraitTargetScope.RandomFriendly ||
               Scope == TraitTargetScope.RandomFriendlyNotSelf || Scope == TraitTargetScope.Self ||
               Scope == TraitTargetScope.UnitStack;
    }

    public bool CheckEnemy()
    {
        return Scope == TraitTargetScope.AnyScope || Scope == TraitTargetScope.AllEnemy ||
               Scope == TraitTargetScope.RandomEnemy || Scope == TraitTargetScope.EnemyUnit ||
               Scope == TraitTargetScope.UnitStack;
    }

    public bool HasAreaTarget()
    {
        return Scope == TraitTargetScope.AnyScope || Scope == TraitTargetScope.AllEnemy ||
               Scope == TraitTargetScope.AllFriendly || Scope == TraitTargetScope.RandomEnemy ||
               Scope == TraitTargetScope.RandomFriendly || Scope == TraitTargetScope.AllFriendlyNotSelf;
    }

    public bool CheckRegion(Region checkRegion, sbyte owner)
    {
        if (checkRegion != Region.NumRegions)
        {
            var region = (Region) (0 + (byte) owner);
            switch (Area)
            {
                case TargetableArea.AnyAreas:
                    return true;
                case TargetableArea.AnyRegion:
                    return true;
                case TargetableArea.BattleField:
                    return true;
                case TargetableArea.BattleFieldNc:
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
                    if (checkRegion == Region.Control)
                    {
                        return true;
                    }

                    break;
                case TargetableArea.FriendlyPerimeter:
                    if (checkRegion == region)
                    {
                        return true;
                    }

                    break;
                case TargetableArea.EnemyPerimeter:
                    if (checkRegion != Region.Control && checkRegion != region)
                    {
                        return true;
                    }

                    break;
                case TargetableArea.FriendlyRegions:
                    if (checkRegion == region || checkRegion == Region.Control)
                    {
                        return true;
                    }

                    break;
                case TargetableArea.EnemyRegions:
                    if (checkRegion == Region.Control || checkRegion != region)
                    {
                        return true;
                    }

                    break;
            }

            return false;
        }

        return true;
    }

    public bool CardTargetMatch(CcgGameState gameState, Card card, Card source)
    {
        CardStack cardStack = null;
        var list = gameState.FindCardStack(card);
        if (list != null && list.Count > 0)
        {
            cardStack = list[0];
        }

        switch (Scope)
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
                if (card.ActiveData.Owner != source.ActiveData.Owner)
                {
                    return false;
                }

                break;
            case TraitTargetScope.FriendlyUnitNotSelf:
            case TraitTargetScope.AllFriendlyNotSelf:
            case TraitTargetScope.RandomFriendlyNotSelf:
                if (card.EqualsTo(source) || card.ActiveData.Owner != source.ActiveData.Owner)
                {
                    return false;
                }

                break;
            case TraitTargetScope.EnemyUnit:
            case TraitTargetScope.AllEnemy:
                if (card.ActiveData.Owner == source.ActiveData.Owner)
                {
                    return false;
                }

                break;
        }

        var traitActorRegion = gameState.GetTraitActorRegion(card.ActiveData.Owner, card.InstanceId);
        if (Area == TargetableArea.CurrentRegion)
        {
            if (traitActorRegion != gameState.GetTraitActorRegion(source.ActiveData.Owner, source.InstanceId))
            {
                return false;
            }
        }
        else if (Area == TargetableArea.BattleFieldNc)
        {
            if (card.GetTemplate().Type == CardType.Commander)
            {
                return false;
            }
        }
        else if (Area == TargetableArea.FriendlyCommander)
        {
            if (card.GetTemplate().Type != CardType.Commander || card.ActiveData.Owner != source.ActiveData.Owner)
            {
                return false;
            }
        }
        else if (Area == TargetableArea.EnemyCommander)
        {
            if (card.GetTemplate().Type != CardType.Commander || card.ActiveData.Owner == source.ActiveData.Owner)
            {
                return false;
            }
        }
        else if (!CheckRegion(traitActorRegion, source.ActiveData.Owner))
        {
            return false;
        }

        if (!DoesMatchType(card))
        {
            return false;
        }

        return true;
    }

    public bool DoesMatchType(Card? card)
    {
        return DoesMatchType(Type, Mod, TargetId, card);
    }

    public static bool DoesMatchType(TraitTargetType type, TargetTypeMod mod, int targetId, Card? card)
    {
        if (type == TraitTargetType.AnyType && mod == TargetTypeMod.NumMods)
        {
            return true;
        }

        if (card == null)
        {
            return false;
        }

        var cardType = card.GetTemplate().Type;
        var unitType = card.GetUnitType();
        switch (type)
        {
            case TraitTargetType.Pilot:
                if (cardType != 0)
                {
                    return false;
                }

                if (mod == TargetTypeMod.EmbarkedPilot)
                {
                    var unitCard = (UnitCard) card;
                    if (!unitCard.PilotEmbarked)
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
                        var unitCard3 = (UnitCard) card;
                        if (unitCard3.EmbarkedPilot == null)
                        {
                            return false;
                        }

                        break;
                    }
                    case TargetTypeMod.NotPiloted:
                    {
                        var unitCard2 = (UnitCard) card;
                        if (unitCard2.EmbarkedPilot != null)
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
            case TraitTargetType.CardId:
                if (card.GetTemplate().CardId != targetId)
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
                for (var num = card.ActiveData.ActiveTraits.Count - 1; num >= 0; num--)
                {
                    var activeTrait = card.ActiveData.ActiveTraits[num];
                    if (activeTrait.GetTraitInfo().IsIntercept(activeTrait))
                    {
                        return true;
                    }
                }

                return false;
            }
            case TargetTypeMod.HasStealth:
            {
                for (var num2 = card.ActiveData.ActiveTraits.Count - 1; num2 >= 0; num2--)
                {
                    var activeTrait2 = card.ActiveData.ActiveTraits[num2];
                    if (activeTrait2.GetTraitInfo().IsCombatManipulationPassive(CombatManipulationPassiveType.Stealth, activeTrait2))
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
                    var maxModHealth = card.GetMaxModHealth();
                    if (card.GetCurrentHealth(false) < maxModHealth)
                    {
                        return true;
                    }
                }

                return false;
            case TargetTypeMod.IsStunned:
                if (card.HasStatusEffect(ApplyStatusTraitStatusType.Stun))
                {
                    return true;
                }

                return false;
            case TargetTypeMod.IsDeterred:
                if (card.HasStatusEffect(ApplyStatusTraitStatusType.Deter))
                {
                    return true;
                }

                return false;
            case TargetTypeMod.IsImmobilized:
                if (card.HasStatusEffect(ApplyStatusTraitStatusType.Immobilize))
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

    public int CalculateCount(CcgGameState gameState, ActiveTrait active)
    {
        var num = 0;
        var region = Region.NumRegions;
        if (Area == TargetableArea.CurrentRegion)
        {
            region = gameState.GetTraitActorRegion(active.GetTraitTarget().ActiveData.Owner,
                active.GetTraitTarget().InstanceId);
        }

        var list = gameState.FindCards(this, region, active.GetTraitSource());
        for (var i = 0; i < list.Count; i++)
        {
            var card = list[i].PrimaryCard;
            if (DoesMatchType(card))
            {
                num++;
            }

            var secrets = card.GetSecrets();
            foreach (var secret in secrets)
            {
                if (DoesMatchType(secret))
                {
                    num++;
                }
            }

            if (card.HasPilot() && DoesMatchType(card.GetEmbarkedPilot()!))
            {
                num++;
            }
        }

        return num;
    }
}