using System.Text.Json.Serialization;
using Frontline.Battle.CcgEvents;
using Frontline.Battle.Traits;

namespace Frontline.Battle;

[JsonDerivedType(typeof(ActedModExclusive), "ActedModExlusive")]
[JsonDerivedType(typeof(ActedModPassive), "ActedModPassive")]
[JsonDerivedType(typeof(ApplyDamage), "ApplyDamage")]
[JsonDerivedType(typeof(ApplyDamageMultiply), "ApplyDamageMultiply")]
[JsonDerivedType(typeof(ApplyHeal), "ApplyHeal")]
[JsonDerivedType(typeof(ApplyHealMultiply), "ApplyHealMultiply")]
[JsonDerivedType(typeof(ApplyStatus), "ApplyStatus")]
[JsonDerivedType(typeof(BlockEmbark), "BlockEmbark")]
[JsonDerivedType(typeof(ChallengeEffect), "ChallengeEffect")]
[JsonDerivedType(typeof(CombatManipulationPassive), "CombatManipulationPassive")]
[JsonDerivedType(typeof(CommandModEffect), "CommandModEffect")]
[JsonDerivedType(typeof(DamageImmunity), "DamageImmunity")]
[JsonDerivedType(typeof(DeployCardEffect), "DeployCardEffect")]
[JsonDerivedType(typeof(DiscardEffect), "DiscardEffect")]
[JsonDerivedType(typeof(DrawCardEffect), "DrawCardEffect")]
[JsonDerivedType(typeof(DrawCardMultiply), "DrawCardMultiply")]
[JsonDerivedType(typeof(EjectEffect), "EjectEffect")]
[JsonDerivedType(typeof(ForceCombatEffect), "ForceCombatEffect")]
[JsonDerivedType(typeof(ForceDisembarkEffect), "ForceDisembarkEffect")]
[JsonDerivedType(typeof(ForceMoveEffect), "ForceMoveEffect")]
[JsonDerivedType(typeof(IgnoreInterceptPassive), "IgnoreInterceptPassive")]
[JsonDerivedType(typeof(InterceptPassive), "InterceptPassive")]
[JsonDerivedType(typeof(NegateActivationEffect), "NegateActivationEffect")]
[JsonDerivedType(typeof(ReactiveDamage), "ReactiveDamage")]
[JsonDerivedType(typeof(RemoveStatus), "RemoveStatus")]
[JsonDerivedType(typeof(RemoveTraitEffect), "RemoveTraitEffect")]
[JsonDerivedType(typeof(StatModifierMultiply), "StatModifierMultiply")]
[JsonDerivedType(typeof(StatModifierPassive), "StatModifierPassive")]
[JsonDerivedType(typeof(StatTraitOverride), "StatTraitOverride")]
[JsonDerivedType(typeof(StatTransfer), "StatTransfer")]
[JsonDerivedType(typeof(SummonTrait), "SummonTrait")]
[JsonDerivedType(typeof(TargetEffect), "TargetEffect")]
[JsonDerivedType(typeof(TraitTrigger), "TraitTrigger")]
[JsonDerivedType(typeof(UnsummonEffect), "UnsummonEffect")]
[JsonDerivedType(typeof(WarpFallEffect), "WarpFallEffect")]
public class BaseTraitEffect
{
    [JsonPropertyName("effectTraitID")]
    public int EffectTraitId { get; set; }

    [JsonPropertyName("traitParentID")]
    public int TraitParentId { get; set; }

    public bool TargetPrimary { get; set; }

    public bool EmbarkedInherit { get; set; }

    public bool Deterable { get; set; } = true;

    public sbyte Priority { get; set; }

    public TraitTargeting Targets { get; set; }

    public TraitDuration DurationData { get; set; }

    protected CCG GameState = null!;

    public void Init(CCG gameState)
    {
        GameState = gameState;
    }

    public virtual void Activate(Card card, CardStack target, Region region)
    {
        if (IsTrigger())
        {
            return;
        }

        var list = new List<Card>();
        Card card2 = null;
        var traitActivateCCGEvent = new TraitActivateCcgEvent();
        traitActivateCCGEvent.CardId = card.InstanceId;
        traitActivateCCGEvent.Owner = card.ActiveData.Owner;
        traitActivateCCGEvent.TraitId = TraitParentId;
        traitActivateCCGEvent.EffectId = EffectTraitId;
        traitActivateCCGEvent.Region = region;
        traitActivateCCGEvent.Deactivate = false;
        GameState.AddCCGEventLog(traitActivateCCGEvent);
        GameState.TraitEffectActivating(this, card, target, region);
        var activeTrait = CheckEffectNegation(card);
        if (activeTrait != null)
        {
            if (activeTrait.HasCharges())
            {
                activeTrait.ExpendCharge();
            }

            return;
        }

        if (Targets.Area == TargetableArea.Self || Targets.Scope == TraitTargetScope.Self)
        {
            if (CheckAndApplyTrait(card, card, false, true))
            {
                list.Add(card);
            }
        }
        else if (Targets.Scope == TraitTargetScope.TriggeringUnit || Targets.Scope == TraitTargetScope.TriggerTarget ||
                 Targets.Scope == TraitTargetScope.FriendlyUnit ||
                 Targets.Scope == TraitTargetScope.FriendlyUnitNotSelf || Targets.Scope == TraitTargetScope.EnemyUnit)
        {
            if (Targets.Area == TargetableArea.FriendlyCommander)
            {
                var owner = card.ActiveData.Owner;
                card2 = GameState.Players[owner].Commander.PrimaryCard;
                if (CheckAndApplyTrait(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
            else if (Targets.Area == TargetableArea.EnemyCommander)
            {
                var opponentPlayerIndex = GameState.GetOpponentPlayerIndex(card.ActiveData.Owner);
                card2 = GameState.Players[opponentPlayerIndex].Commander.PrimaryCard;
                if (CheckAndApplyTrait(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
            else if (target != null && target.PrimaryCard != null)
            {
                card2 = target.PrimaryCard;
                if (CheckAndApplyTrait(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
        }
        else if (Targets.Area == TargetableArea.UnitStack || Targets.Scope == TraitTargetScope.UnitStack)
        {
            var cardStack = target;
            if (cardStack == null)
            {
                if (Targets.Area == TargetableArea.FriendlyCommander)
                {
                    var owner2 = card.ActiveData.Owner;
                    cardStack = GameState.Players[owner2].Commander;
                }
                else if (Targets.Area == TargetableArea.EnemyCommander)
                {
                    var opponentPlayerIndex2 = GameState.GetOpponentPlayerIndex(card.ActiveData.Owner);
                    cardStack = GameState.Players[opponentPlayerIndex2].Commander;
                }
            }

            if (cardStack != null)
            {
                if (cardStack.PrimaryCard != null)
                {
                    if (cardStack.PrimaryCard.HasPilot())
                    {
                        card2 = cardStack.PrimaryCard.GetEmbarkedPilot();
                        if (CheckAndApplyTrait(card2, card, false, true))
                        {
                            list.Add(card2);
                        }
                    }

                    var secrets = cardStack.PrimaryCard.GetSecrets();
                    if (secrets != null)
                    {
                        for (var num = secrets.Count - 1; num >= 0; num--)
                        {
                            card2 = secrets[num];
                            if (CheckAndApplyTrait(card2, card, false, true))
                            {
                                list.Add(card2);
                            }
                        }
                    }

                    card2 = cardStack.PrimaryCard;
                    if (CheckAndApplyTrait(card2, card, false, true))
                    {
                        list.Add(card2);
                    }
                }

                card2 = cardStack.GetEjectedCard();
                if (card2 != null && CheckAndApplyTrait(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
        }
        else
        {
            CheckGlobalApply(card, region, false, list);
        }

        if (list.Count > 0)
        {
            traitActivateCCGEvent.Targets = new ActiveTraitCardInfo[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                traitActivateCCGEvent.Targets[i] = new ActiveTraitCardInfo();
                traitActivateCCGEvent.Targets[i].InstanceId = list[i].InstanceId;
                traitActivateCCGEvent.Targets[i].Owner = list[i].ActiveData.Owner;
            }
        }
    }

    public void ActivateTrigger(Card card, CardStack target, TraitTargeting triggerTarget)
    {
        if (!IsTrigger())
        {
            return;
        }

        var list = new List<Card>();
        if (triggerTarget.Scope == TraitTargetScope.Self)
        {
            if (triggerTarget.DoesMatchType(card))
            {
                var active = GenerateActiveTrait(card, card);
                Apply(card, card, active);
            }
        }
        else if (triggerTarget.Area == TargetableArea.FriendlyCommander)
        {
            var primaryCard = GameState.Players[card.ActiveData.Owner].Commander.PrimaryCard;
            if (triggerTarget.DoesMatchType(primaryCard))
            {
                var active2 = GenerateActiveTrait(primaryCard, card);
                Apply(primaryCard, card, active2);
                list.Add(primaryCard);
            }
        }
        else if (triggerTarget.Area == TargetableArea.EnemyCommander)
        {
            var opponentPlayerIndex = GameState.GetOpponentPlayerIndex(card.ActiveData.Owner);
            var primaryCard2 = GameState.Players[opponentPlayerIndex].Commander.PrimaryCard;
            if (triggerTarget.DoesMatchType(primaryCard2))
            {
                var active3 = GenerateActiveTrait(primaryCard2, card);
                Apply(primaryCard2, card, active3);
                list.Add(primaryCard2);
            }
        }
        else if (triggerTarget.Area == TargetableArea.AnyCommander)
        {
            for (var i = 0; i < GameState.Players.Length; i++)
            {
                var primaryCard3 = GameState.Players[i].Commander.PrimaryCard;
                if (triggerTarget.DoesMatchType(primaryCard3))
                {
                    var active4 = GenerateActiveTrait(primaryCard3, card);
                    Apply(primaryCard3, card, active4);
                    list.Add(primaryCard3);
                }
            }
        }
        else if (triggerTarget.HasAreaTarget())
        {
            var list2 = GameState.FindCards(triggerTarget, Region.NumRegions, card);
            for (var j = 0; j < list2.Count; j++)
            {
                var primaryCard4 = list2[j].PrimaryCard;
                if (triggerTarget.DoesMatchType(primaryCard4))
                {
                    var active5 = GenerateActiveTrait(primaryCard4, card);
                    Apply(primaryCard4, card, active5);
                    list.Add(primaryCard4);
                }
            }
        }
        else if (target != null && target.PrimaryCard != null)
        {
            var primaryCard5 = target.PrimaryCard;
            if (triggerTarget.DoesMatchType(primaryCard5))
            {
                var active6 = GenerateActiveTrait(primaryCard5, card);
                Apply(primaryCard5, card, active6);
                list.Add(primaryCard5);
            }
        }
    }

    public void CheckGlobalApply(Card card, Region region, bool ignoreSelf, List<Card> appliedTo = null)
    {
        var region2 = Region.NumRegions;
        if (Targets.Area == TargetableArea.CurrentRegion)
        {
            region2 = region;
        }

        var list = GameState.FindCards(Targets, region2, card);
        Card card2 = null;
        CardStack cardStack = null;
        if (Targets.Scope == TraitTargetScope.RandomEnemy || Targets.Scope == TraitTargetScope.RandomFriendly)
        {
            var list2 = new List<Card>();
            for (var i = 0; i < list.Count; i++)
            {
                cardStack = list[i];
                if (cardStack == null)
                {
                    continue;
                }

                if (cardStack.PrimaryCard != null)
                {
                    if (cardStack.PrimaryCard.HasPilot())
                    {
                        card2 = cardStack.PrimaryCard.GetEmbarkedPilot();
                        if (DoesApply(card2, card, false, true))
                        {
                            list2.Add(card2);
                        }
                    }

                    var secrets = cardStack.PrimaryCard.GetSecrets();
                    if (secrets != null)
                    {
                        for (var num = secrets.Count - 1; num >= 0; num--)
                        {
                            card2 = secrets[num];
                            if (DoesApply(card2, card, false, true))
                            {
                                list2.Add(card2);
                            }
                        }
                    }

                    card2 = cardStack.PrimaryCard;
                    if (DoesApply(card2, card, false, true))
                    {
                        list2.Add(card2);
                    }
                }

                card2 = cardStack.GetEjectedCard();
                if (card2 != null && DoesApply(card2, card, false, true))
                {
                    list2.Add(card2);
                }
            }

            if (list2.Count > 0)
            {
                var serverIntValue = GameState.GetGame().GetServerIntValue(0, list2.Count);
                card2 = list2[serverIntValue];
                var active = GenerateActiveTrait(card2, card);
                Apply(card2, card, active);
                if (appliedTo != null)
                {
                    appliedTo.Add(card2);
                }
            }

            return;
        }

        for (var j = 0; j < list.Count; j++)
        {
            cardStack = list[j];
            if (cardStack == null)
            {
                continue;
            }

            if (cardStack.PrimaryCard != null)
            {
                if (cardStack.PrimaryCard.HasPilot())
                {
                    card2 = cardStack.PrimaryCard.GetEmbarkedPilot();
                    if (CheckAndApplyTrait(card2, card, false, true) && appliedTo != null)
                    {
                        appliedTo.Add(card2);
                    }
                }

                var secrets2 = cardStack.PrimaryCard.GetSecrets();
                if (secrets2 != null)
                {
                    for (var num2 = secrets2.Count - 1; num2 >= 0; num2--)
                    {
                        card2 = secrets2[num2];
                        if (CheckAndApplyTrait(card2, card, false, true) && appliedTo != null)
                        {
                            appliedTo.Add(card2);
                        }
                    }
                }

                card2 = cardStack.PrimaryCard;
                if (CheckAndApplyTrait(card2, card, false, true) && appliedTo != null)
                {
                    appliedTo.Add(card2);
                }
            }

            card2 = cardStack.GetEjectedCard();
            if (card2 != null && CheckAndApplyTrait(card2, card, false, true) && appliedTo != null)
            {
                appliedTo.Add(card2);
            }
        }
    }

    public bool CheckAndApplyTrait(Card card, Card source, bool checkRange, bool onDeploy)
    {
        if (DoesApply(card, source, checkRange, onDeploy))
        {
            var active = GenerateActiveTrait(card, source);
            Apply(card, source, active);
            return true;
        }

        return false;
    }

    public List<Card> CheckForAppliedTargets(Card card, CardStack target, Region region)
    {
        var list = new List<Card>();
        List<Card> list2 = null;
        Card card2 = null;
        if (IsTrigger() || TargetTrait())
        {
            return list;
        }

        var activeTrait = CheckEffectNegation(card);
        if (activeTrait != null)
        {
            if (activeTrait.HasCharges())
            {
                activeTrait.ExpendCharge();
            }

            return list;
        }

        if (Targets.Area == TargetableArea.Self || Targets.Scope == TraitTargetScope.Self)
        {
            if (DoesApply(card, card, false, true))
            {
                list.Add(card);
            }
        }
        else if (Targets.Scope == TraitTargetScope.TriggeringUnit || Targets.Scope == TraitTargetScope.TriggerTarget ||
                 Targets.Scope == TraitTargetScope.FriendlyUnit ||
                 Targets.Scope == TraitTargetScope.FriendlyUnitNotSelf || Targets.Scope == TraitTargetScope.EnemyUnit)
        {
            if (Targets.Area == TargetableArea.FriendlyCommander)
            {
                var owner = card.ActiveData.Owner;
                card2 = GameState.Players[owner].Commander.PrimaryCard;
                if (DoesApply(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
            else if (Targets.Area == TargetableArea.EnemyCommander)
            {
                var opponentPlayerIndex = GameState.GetOpponentPlayerIndex(card.ActiveData.Owner);
                card2 = GameState.Players[opponentPlayerIndex].Commander.PrimaryCard;
                if (DoesApply(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
            else if (target != null && target.PrimaryCard != null)
            {
                card2 = target.PrimaryCard;
                if (DoesApply(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
        }
        else if (Targets.Area == TargetableArea.UnitStack || Targets.Scope == TraitTargetScope.UnitStack)
        {
            var cardStack = target;
            if (cardStack == null)
            {
                if (Targets.Area == TargetableArea.FriendlyCommander)
                {
                    var owner2 = card.ActiveData.Owner;
                    cardStack = GameState.Players[owner2].Commander;
                }
                else if (Targets.Area == TargetableArea.EnemyCommander)
                {
                    var opponentPlayerIndex2 = GameState.GetOpponentPlayerIndex(card.ActiveData.Owner);
                    cardStack = GameState.Players[opponentPlayerIndex2].Commander;
                }
            }

            if (cardStack != null)
            {
                if (cardStack.PrimaryCard != null)
                {
                    if (cardStack.PrimaryCard.HasPilot())
                    {
                        card2 = cardStack.PrimaryCard.GetEmbarkedPilot();
                        if (DoesApply(card2, card, false, true))
                        {
                            list.Add(card2);
                        }
                    }

                    list2 = cardStack.PrimaryCard.GetSecrets();
                    if (list2 != null)
                    {
                        for (var num = list2.Count - 1; num >= 0; num--)
                        {
                            card2 = list2[num];
                            if (DoesApply(card2, card, false, true))
                            {
                                list.Add(card2);
                            }
                        }
                    }

                    card2 = cardStack.PrimaryCard;
                    if (DoesApply(card2, card, false, true))
                    {
                        list.Add(card2);
                    }
                }

                card2 = cardStack.GetEjectedCard();
                if (card2 != null && DoesApply(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
        }
        else
        {
            var region2 = Region.NumRegions;
            if (Targets.Area == TargetableArea.CurrentRegion)
            {
                region2 = region;
            }

            var list3 = GameState.FindCards(Targets, region2, card);
            CardStack cardStack2 = null;
            for (var i = 0; i < list3.Count; i++)
            {
                cardStack2 = list3[i];
                if (cardStack2 != null && cardStack2.PrimaryCard != null)
                {
                    card2 = cardStack2.PrimaryCard;
                    if (DoesApply(card2, card, false, true))
                    {
                        list.Add(card2);
                    }

                    list2 = cardStack2.PrimaryCard.GetSecrets();
                    if (list2 != null)
                    {
                        for (var num2 = list2.Count - 1; num2 >= 0; num2--)
                        {
                            card2 = list2[num2];
                            if (DoesApply(card2, card, false, true))
                            {
                                list.Add(card2);
                            }
                        }
                    }

                    if (cardStack2.PrimaryCard.HasPilot())
                    {
                        card2 = cardStack2.PrimaryCard.GetEmbarkedPilot();
                        if (DoesApply(card2, card, false, true))
                        {
                            list.Add(card2);
                        }
                    }
                }

                card2 = cardStack2.GetEjectedCard();
                if (card2 != null && DoesApply(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
        }

        return list;
    }

    public bool HasBroadTargetRange()
    {
        if (Targets.Area == TargetableArea.Self || Targets.Scope == TraitTargetScope.Self ||
            Targets.Area == TargetableArea.UnitStack || Targets.Scope == TraitTargetScope.UnitStack)
        {
            return false;
        }

        return true;
    }

    public virtual bool DoesApply(Card card, Card source, bool checkRange, bool onDeploy)
    {
        if (!onDeploy && DurationData.Type != TraitDurationType.Permanent)
        {
            return false;
        }

        if (checkRange)
        {
            if (Targets.Area == TargetableArea.Self)
            {
                if (!card.EqualsTo(source))
                {
                    return false;
                }
            }
            else if (Targets.Area == TargetableArea.UnitStack)
            {
                var list = GameState.FindCardStack(card);
                if (list == null || list.Count == 0)
                {
                    return false;
                }
            }
            else if (Targets.Area == TargetableArea.CurrentRegion)
            {
                var traitActorRegion = GameState.GetTraitActorRegion(card.ActiveData.Owner, card.InstanceId);
                var traitActorRegion2 = GameState.GetTraitActorRegion(source.ActiveData.Owner, source.InstanceId);
                if (traitActorRegion != traitActorRegion2)
                {
                    return false;
                }
            }
            else
            {
                var traitActorRegion3 = GameState.GetTraitActorRegion(card.ActiveData.Owner, card.InstanceId);
                if (!Targets.CheckRegion(traitActorRegion3, source.ActiveData.Owner))
                {
                    return false;
                }
            }
        }

        if (Targets.Scope != TraitTargetScope.TriggeringUnit && Targets.Scope != TraitTargetScope.TriggerTarget)
        {
            if (card.ActiveData.Owner == source.ActiveData.Owner && !Targets.CheckFriendly())
            {
                return false;
            }

            if (card.ActiveData.Owner != source.ActiveData.Owner && !Targets.CheckEnemy())
            {
                return false;
            }

            if ((Targets.Scope == TraitTargetScope.AllFriendlyNotSelf ||
                 Targets.Scope == TraitTargetScope.FriendlyUnitNotSelf ||
                 Targets.Scope == TraitTargetScope.RandomFriendlyNotSelf) && card.EqualsTo(source))
            {
                return false;
            }
        }

        if (!Targets.DoesMatchType(card))
        {
            return false;
        }

        return true;
    }

    public virtual void Apply(Card card, Card source, ActiveTrait active)
    {
        if (Deterable && card.IsCardTraitsDetered())
        {
            active.Detered = true;
        }

        card.ActiveData.ActiveTraits.Add(active);
    }

    public virtual void Init(Card card, Card source, ActiveTrait active)
    {
    }

    public virtual void Deactivate(ActiveTrait active)
    {
        var traitSource = active.GetTraitSource();
        var traitTarget = active.GetTraitTarget();
        var traitActivateCCGEvent = new TraitActivateCcgEvent();
        traitActivateCCGEvent.CardId = traitSource != null ? traitSource.InstanceId : 0;
        traitActivateCCGEvent.Owner = (sbyte) (traitSource != null ? traitSource.ActiveData.Owner : 0);
        traitActivateCCGEvent.TraitId = TraitParentId;
        traitActivateCCGEvent.EffectId = EffectTraitId;
        traitActivateCCGEvent.Deactivate = true;
        if (traitTarget != null)
        {
            var activeTraitCardInfo = new ActiveTraitCardInfo();
            activeTraitCardInfo.InstanceId = traitTarget.InstanceId;
            activeTraitCardInfo.Owner = traitTarget.ActiveData.Owner;
            traitActivateCCGEvent.Targets = new ActiveTraitCardInfo[1];
            traitActivateCCGEvent.Targets[0] = activeTraitCardInfo;
        }

        GameState.AddCCGEventLog(traitActivateCCGEvent);
    }

    public ActiveTrait GenerateActiveTrait(Card card, Card source)
    {
        var activeTrait = new ActiveTrait(GameState);
        activeTrait.Init(this, card, source, DurationData);
        var num = 0;
        ActiveTrait activeTrait2 = null;
        for (var i = 0; i < card.ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait2 = card.ActiveData.ActiveTraits[i];
            if (activeTrait2.GetTraitInfo() == null)
            {
                GameState.Logger.Warning("Activated Trait is Missing Trait Data! effect:" + activeTrait2.TraitEffectId +
                                  " trait:" + activeTrait2.TraitSourceId);
            }
            else
            {
                num = activeTrait2.GetTraitInfo().GetOverrideData(activeTrait2);
                if (num != 0)
                {
                    activeTrait.DataValue = num;
                    break;
                }
            }
        }

        return activeTrait;
    }

    public ActiveTrait CheckEffectNegation(Card source)
    {
        ActiveTrait activeTrait = null;
        var battleEffects = GameState.GetBattleEffects();
        for (var num = battleEffects.Count - 1; num >= 0; num--)
        {
            activeTrait = battleEffects[num];
            if (activeTrait.GetTraitInfo().DoesNegateEffect(this, source, activeTrait))
            {
                return activeTrait;
            }
        }

        return null;
    }

    public virtual bool TargetTrait()
    {
        return false;
    }

    public virtual bool IsTrigger()
    {
        return false;
    }

    public virtual bool IsDamageHeal(bool damage)
    {
        return false;
    }

    public virtual void CheckCardDeployed(Card deployed, Card source)
    {
    }

    public virtual bool Embark(ActiveTrait active)
    {
        return true;
    }

    public virtual bool Disembark(ActiveTrait active)
    {
        return true;
    }

    public virtual bool CanDeploy(CardStack target, Region region)
    {
        return true;
    }

    public virtual bool CanDeployOverride(Region region)
    {
        return false;
    }

    public virtual bool CanMove(Region target, sbyte cardOwner, ActiveTrait active)
    {
        return true;
    }

    public virtual bool CanAttack(CardStack target, ActiveTrait active)
    {
        return true;
    }

    public virtual bool CanCounterAttack(CardStack target, ActiveTrait active)
    {
        return true;
    }

    public virtual void Move(CardStack location, Region region, bool embark, ActiveTrait active)
    {
    }

    public virtual void Attack(Card target, ActiveTrait active)
    {
    }

    public virtual void ActivateAction(CardStack location, Region region, ActiveTrait active)
    {
    }

    public virtual sbyte GetAttackBonus(Card target, ActiveTrait active)
    {
        return 0;
    }

    public virtual sbyte GetBypassDefenseBonus(Card target, ActiveTrait active)
    {
        return 0;
    }

    public virtual sbyte GetDefenseBonus(ActiveTrait active)
    {
        return 0;
    }

    public virtual sbyte GetHealthBonus(ActiveTrait active)
    {
        return 0;
    }

    public virtual sbyte GetCommandMod(ActiveTrait active)
    {
        return 0;
    }

    public virtual int GetOverrideData(ActiveTrait active)
    {
        return 0;
    }

    public virtual bool IsIntercept(ActiveTrait active)
    {
        return false;
    }

    public virtual bool IgnoreIntercept(ActiveTrait active)
    {
        return false;
    }

    public virtual bool DoesNegateEffect(BaseTraitEffect effect, Card source, ActiveTrait active)
    {
        return false;
    }

    public virtual bool IsStatusEffect(sbyte effectID, ActiveTrait active)
    {
        return false;
    }

    public virtual bool IsCombatManipulationPassive(sbyte effectID, ActiveTrait active)
    {
        return false;
    }

    public virtual bool IsDamageImmunity(bool bypass, ActiveTrait active)
    {
        return false;
    }

    public virtual bool CanEmbark()
    {
        return true;
    }

    public virtual void OnNewTurnEvent(Card owner, sbyte playerIndex)
    {
    }

    public virtual void OnCardMovedEvent(Card parent, Card movedCard, CardStack location, Region region,
        Region origin)
    {
    }

    public virtual void NewTurn(ActiveTrait active, sbyte playerIndex)
    {
    }

    public virtual void EndTurn(ActiveTrait active, sbyte playerIndex)
    {
    }

    public virtual void CardDeployed(Card deployed, ActiveTrait active)
    {
    }

    public virtual void CardMoved(Card theCard, CardStack target, Region destination, Region origin,
        ActiveTrait active)
    {
    }

    public virtual void CardAttacked(Card attacker, Card target, ActiveTrait active)
    {
    }

    public virtual void CardCounterAttacked(Card attacker, Card target, ActiveTrait active)
    {
    }

    public virtual void CardGainedStatus(Card theCard, Card source, sbyte statusType, ActiveTrait activeTrait)
    {
    }

    public virtual void CardDamaged(Card damagedCard, Card source, ActiveTrait activeTrait)
    {
    }

    public virtual void CardDied(Card deadCard, Card source, ActiveTrait active)
    {
    }

    public virtual void SecretTriggered(Card secret, Card source, ActiveTrait active)
    {
    }

    public virtual void SecretDestroyed(Card secret, Card source, ActiveTrait active)
    {
    }

    public virtual void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn, ActiveTrait active)
    {
    }

    public virtual void CardDiscardEffect(sbyte playerIndex, int numberOfCards, ActiveTrait active)
    {
    }

    public virtual void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, Region region,
        ActiveTrait active)
    {
    }
}