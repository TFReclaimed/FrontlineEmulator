using System.Text.Json.Serialization;
using Frontline.Battle.CcgEvents;
using Frontline.Battle.Traits;

namespace Frontline.Battle;

[JsonDerivedType(typeof(ActedModExlusive), "ActedModExlusive")]
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
    public int effectTraitID;

    public int traitParentID;

    public bool targetPrimary;

    public bool embarkedInherit;

    public bool deterable = true;

    public sbyte priority;

    public TraitTargeting targets;

    public TraitDuration durationData;

    protected CCG GameState = null!;

    public void Init(CCG gameState)
    {
        GameState = gameState;
    }

    public virtual void Activate(Card card, CardStack target, RegionEnum region)
    {
        if (IsTrigger())
        {
            return;
        }

        List<Card> list = new List<Card>();
        Card card2 = null;
        TraitActivateCCGEvent traitActivateCCGEvent = new TraitActivateCCGEvent();
        traitActivateCCGEvent.cardID = card.instanceId;
        traitActivateCCGEvent.owner = card.activeData.owner;
        traitActivateCCGEvent.traitID = traitParentID;
        traitActivateCCGEvent.effectID = effectTraitID;
        traitActivateCCGEvent.region = region;
        traitActivateCCGEvent.deactivate = false;
        GameState.AddCCGEventLog(traitActivateCCGEvent);
        GameState.TraitEffectActivating(this, card, target, region);
        ActiveTrait activeTrait = CheckEffectNegation(card);
        if (activeTrait != null)
        {
            if (activeTrait.HasCharges())
            {
                activeTrait.ExpendCharge(GameState);
            }

            return;
        }

        if (targets.area == TargetableArea.Self || targets.scope == TraitTargetScope.Self)
        {
            if (CheckAndApplyTrait(card, card, false, true))
            {
                list.Add(card);
            }
        }
        else if (targets.scope == TraitTargetScope.TriggeringUnit || targets.scope == TraitTargetScope.TriggerTarget ||
                 targets.scope == TraitTargetScope.FriendlyUnit ||
                 targets.scope == TraitTargetScope.FriendlyUnitNotSelf || targets.scope == TraitTargetScope.EnemyUnit)
        {
            if (targets.area == TargetableArea.FriendlyCommander)
            {
                sbyte owner = card.activeData.owner;
                card2 = GameState.players[owner].commander.primaryCard;
                if (CheckAndApplyTrait(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
            else if (targets.area == TargetableArea.EnemyCommander)
            {
                sbyte opponentPlayerIndex = GameState.GetOpponentPlayerIndex(card.activeData.owner);
                card2 = GameState.players[opponentPlayerIndex].commander.primaryCard;
                if (CheckAndApplyTrait(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
            else if (target != null && target.primaryCard != null)
            {
                card2 = target.primaryCard;
                if (CheckAndApplyTrait(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
        }
        else if (targets.area == TargetableArea.UnitStack || targets.scope == TraitTargetScope.UnitStack)
        {
            CardStack cardStack = target;
            if (cardStack == null)
            {
                if (targets.area == TargetableArea.FriendlyCommander)
                {
                    sbyte owner2 = card.activeData.owner;
                    cardStack = GameState.players[owner2].commander;
                }
                else if (targets.area == TargetableArea.EnemyCommander)
                {
                    sbyte opponentPlayerIndex2 = GameState.GetOpponentPlayerIndex(card.activeData.owner);
                    cardStack = GameState.players[opponentPlayerIndex2].commander;
                }
            }

            if (cardStack != null)
            {
                if (cardStack.primaryCard != null)
                {
                    if (cardStack.primaryCard.HasPilot())
                    {
                        card2 = cardStack.primaryCard.GetEmbarkedPilot();
                        if (CheckAndApplyTrait(card2, card, false, true))
                        {
                            list.Add(card2);
                        }
                    }

                    List<Card> secrets = cardStack.primaryCard.GetSecrets();
                    if (secrets != null)
                    {
                        for (int num = secrets.Count - 1; num >= 0; num--)
                        {
                            card2 = secrets[num];
                            if (CheckAndApplyTrait(card2, card, false, true))
                            {
                                list.Add(card2);
                            }
                        }
                    }

                    card2 = cardStack.primaryCard;
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
            traitActivateCCGEvent.targets = new ActiveTraitCardInfo[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                traitActivateCCGEvent.targets[i] = new ActiveTraitCardInfo();
                traitActivateCCGEvent.targets[i].instanceId = list[i].instanceId;
                traitActivateCCGEvent.targets[i].owner = list[i].activeData.owner;
            }
        }
    }

    public void ActivateTrigger(Card card, CardStack target, TraitTargeting triggerTarget)
    {
        if (!IsTrigger())
        {
            return;
        }

        List<Card> list = new List<Card>();
        if (triggerTarget.scope == TraitTargetScope.Self)
        {
            if (triggerTarget.DoesMatchType(card))
            {
                ActiveTrait active = GenerateActiveTrait(card, card);
                Apply(card, card, active);
            }
        }
        else if (triggerTarget.area == TargetableArea.FriendlyCommander)
        {
            Card primaryCard = GameState.players[card.activeData.owner].commander.primaryCard;
            if (triggerTarget.DoesMatchType(primaryCard))
            {
                ActiveTrait active2 = GenerateActiveTrait(primaryCard, card);
                Apply(primaryCard, card, active2);
                list.Add(primaryCard);
            }
        }
        else if (triggerTarget.area == TargetableArea.EnemyCommander)
        {
            sbyte opponentPlayerIndex = GameState.GetOpponentPlayerIndex(card.activeData.owner);
            Card primaryCard2 = GameState.players[opponentPlayerIndex].commander.primaryCard;
            if (triggerTarget.DoesMatchType(primaryCard2))
            {
                ActiveTrait active3 = GenerateActiveTrait(primaryCard2, card);
                Apply(primaryCard2, card, active3);
                list.Add(primaryCard2);
            }
        }
        else if (triggerTarget.area == TargetableArea.AnyCommander)
        {
            for (int i = 0; i < GameState.players.Length; i++)
            {
                Card primaryCard3 = GameState.players[i].commander.primaryCard;
                if (triggerTarget.DoesMatchType(primaryCard3))
                {
                    ActiveTrait active4 = GenerateActiveTrait(primaryCard3, card);
                    Apply(primaryCard3, card, active4);
                    list.Add(primaryCard3);
                }
            }
        }
        else if (triggerTarget.HasAreaTarget())
        {
            List<CardStack> list2 = GameState.FindCards(triggerTarget, RegionEnum.NumRegions, card);
            for (int j = 0; j < list2.Count; j++)
            {
                Card primaryCard4 = list2[j].primaryCard;
                if (triggerTarget.DoesMatchType(primaryCard4))
                {
                    ActiveTrait active5 = GenerateActiveTrait(primaryCard4, card);
                    Apply(primaryCard4, card, active5);
                    list.Add(primaryCard4);
                }
            }
        }
        else if (target != null && target.primaryCard != null)
        {
            Card primaryCard5 = target.primaryCard;
            if (triggerTarget.DoesMatchType(primaryCard5))
            {
                ActiveTrait active6 = GenerateActiveTrait(primaryCard5, card);
                Apply(primaryCard5, card, active6);
                list.Add(primaryCard5);
            }
        }
    }

    public void CheckGlobalApply(Card card, RegionEnum region, bool ignoreSelf, List<Card> appliedTo = null)
    {
        RegionEnum region2 = RegionEnum.NumRegions;
        if (targets.area == TargetableArea.CurrentRegion)
        {
            region2 = region;
        }

        List<CardStack> list = GameState.FindCards(targets, region2, card);
        Card card2 = null;
        CardStack cardStack = null;
        if (targets.scope == TraitTargetScope.RandomEnemy || targets.scope == TraitTargetScope.RandomFriendly)
        {
            List<Card> list2 = new List<Card>();
            for (int i = 0; i < list.Count; i++)
            {
                cardStack = list[i];
                if (cardStack == null)
                {
                    continue;
                }

                if (cardStack.primaryCard != null)
                {
                    if (cardStack.primaryCard.HasPilot())
                    {
                        card2 = cardStack.primaryCard.GetEmbarkedPilot();
                        if (DoesApply(card2, card, false, true))
                        {
                            list2.Add(card2);
                        }
                    }

                    List<Card> secrets = cardStack.primaryCard.GetSecrets();
                    if (secrets != null)
                    {
                        for (int num = secrets.Count - 1; num >= 0; num--)
                        {
                            card2 = secrets[num];
                            if (DoesApply(card2, card, false, true))
                            {
                                list2.Add(card2);
                            }
                        }
                    }

                    card2 = cardStack.primaryCard;
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
                int serverIntValue = GameState.GetGame().GetServerIntValue(0, list2.Count);
                card2 = list2[serverIntValue];
                ActiveTrait active = GenerateActiveTrait(card2, card);
                Apply(card2, card, active);
                if (appliedTo != null)
                {
                    appliedTo.Add(card2);
                }
            }

            return;
        }

        for (int j = 0; j < list.Count; j++)
        {
            cardStack = list[j];
            if (cardStack == null)
            {
                continue;
            }

            if (cardStack.primaryCard != null)
            {
                if (cardStack.primaryCard.HasPilot())
                {
                    card2 = cardStack.primaryCard.GetEmbarkedPilot();
                    if (CheckAndApplyTrait(card2, card, false, true) && appliedTo != null)
                    {
                        appliedTo.Add(card2);
                    }
                }

                List<Card> secrets2 = cardStack.primaryCard.GetSecrets();
                if (secrets2 != null)
                {
                    for (int num2 = secrets2.Count - 1; num2 >= 0; num2--)
                    {
                        card2 = secrets2[num2];
                        if (CheckAndApplyTrait(card2, card, false, true) && appliedTo != null)
                        {
                            appliedTo.Add(card2);
                        }
                    }
                }

                card2 = cardStack.primaryCard;
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
            ActiveTrait active = GenerateActiveTrait(card, source);
            Apply(card, source, active);
            return true;
        }

        return false;
    }

    public List<Card> CheckForAppliedTargets(Card card, CardStack target, RegionEnum region)
    {
        List<Card> list = new List<Card>();
        List<Card> list2 = null;
        Card card2 = null;
        if (IsTrigger() || TargetTrait())
        {
            return list;
        }

        ActiveTrait activeTrait = CheckEffectNegation(card);
        if (activeTrait != null)
        {
            if (activeTrait.HasCharges())
            {
                activeTrait.ExpendCharge(GameState);
            }

            return list;
        }

        if (targets.area == TargetableArea.Self || targets.scope == TraitTargetScope.Self)
        {
            if (DoesApply(card, card, false, true))
            {
                list.Add(card);
            }
        }
        else if (targets.scope == TraitTargetScope.TriggeringUnit || targets.scope == TraitTargetScope.TriggerTarget ||
                 targets.scope == TraitTargetScope.FriendlyUnit ||
                 targets.scope == TraitTargetScope.FriendlyUnitNotSelf || targets.scope == TraitTargetScope.EnemyUnit)
        {
            if (targets.area == TargetableArea.FriendlyCommander)
            {
                sbyte owner = card.activeData.owner;
                card2 = GameState.players[owner].commander.primaryCard;
                if (DoesApply(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
            else if (targets.area == TargetableArea.EnemyCommander)
            {
                sbyte opponentPlayerIndex = GameState.GetOpponentPlayerIndex(card.activeData.owner);
                card2 = GameState.players[opponentPlayerIndex].commander.primaryCard;
                if (DoesApply(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
            else if (target != null && target.primaryCard != null)
            {
                card2 = target.primaryCard;
                if (DoesApply(card2, card, false, true))
                {
                    list.Add(card2);
                }
            }
        }
        else if (targets.area == TargetableArea.UnitStack || targets.scope == TraitTargetScope.UnitStack)
        {
            CardStack cardStack = target;
            if (cardStack == null)
            {
                if (targets.area == TargetableArea.FriendlyCommander)
                {
                    sbyte owner2 = card.activeData.owner;
                    cardStack = GameState.players[owner2].commander;
                }
                else if (targets.area == TargetableArea.EnemyCommander)
                {
                    sbyte opponentPlayerIndex2 = GameState.GetOpponentPlayerIndex(card.activeData.owner);
                    cardStack = GameState.players[opponentPlayerIndex2].commander;
                }
            }

            if (cardStack != null)
            {
                if (cardStack.primaryCard != null)
                {
                    if (cardStack.primaryCard.HasPilot())
                    {
                        card2 = cardStack.primaryCard.GetEmbarkedPilot();
                        if (DoesApply(card2, card, false, true))
                        {
                            list.Add(card2);
                        }
                    }

                    list2 = cardStack.primaryCard.GetSecrets();
                    if (list2 != null)
                    {
                        for (int num = list2.Count - 1; num >= 0; num--)
                        {
                            card2 = list2[num];
                            if (DoesApply(card2, card, false, true))
                            {
                                list.Add(card2);
                            }
                        }
                    }

                    card2 = cardStack.primaryCard;
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
            RegionEnum region2 = RegionEnum.NumRegions;
            if (targets.area == TargetableArea.CurrentRegion)
            {
                region2 = region;
            }

            List<CardStack> list3 = GameState.FindCards(targets, region2, card);
            CardStack cardStack2 = null;
            for (int i = 0; i < list3.Count; i++)
            {
                cardStack2 = list3[i];
                if (cardStack2 != null && cardStack2.primaryCard != null)
                {
                    card2 = cardStack2.primaryCard;
                    if (DoesApply(card2, card, false, true))
                    {
                        list.Add(card2);
                    }

                    list2 = cardStack2.primaryCard.GetSecrets();
                    if (list2 != null)
                    {
                        for (int num2 = list2.Count - 1; num2 >= 0; num2--)
                        {
                            card2 = list2[num2];
                            if (DoesApply(card2, card, false, true))
                            {
                                list.Add(card2);
                            }
                        }
                    }

                    if (cardStack2.primaryCard.HasPilot())
                    {
                        card2 = cardStack2.primaryCard.GetEmbarkedPilot();
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
        if (targets.area == TargetableArea.Self || targets.scope == TraitTargetScope.Self ||
            targets.area == TargetableArea.UnitStack || targets.scope == TraitTargetScope.UnitStack)
        {
            return false;
        }

        return true;
    }

    public virtual bool DoesApply(Card card, Card source, bool checkRange, bool onDeploy)
    {
        if (!onDeploy && durationData.type != TraitDurationType.Permanent)
        {
            return false;
        }

        if (checkRange)
        {
            if (targets.area == TargetableArea.Self)
            {
                if (!card.EqualsTo(source))
                {
                    return false;
                }
            }
            else if (targets.area == TargetableArea.UnitStack)
            {
                List<CardStack> list = GameState.FindCardStack(card);
                if (list == null || list.Count == 0)
                {
                    return false;
                }
            }
            else if (targets.area == TargetableArea.CurrentRegion)
            {
                RegionEnum traitActorRegion = GameState.GetTraitActorRegion(card.activeData.owner, card.instanceId);
                RegionEnum traitActorRegion2 = GameState.GetTraitActorRegion(source.activeData.owner, source.instanceId);
                if (traitActorRegion != traitActorRegion2)
                {
                    return false;
                }
            }
            else
            {
                RegionEnum traitActorRegion3 = GameState.GetTraitActorRegion(card.activeData.owner, card.instanceId);
                if (!targets.CheckRegion(traitActorRegion3, source.activeData.owner))
                {
                    return false;
                }
            }
        }

        if (targets.scope != TraitTargetScope.TriggeringUnit && targets.scope != TraitTargetScope.TriggerTarget)
        {
            if (card.activeData.owner == source.activeData.owner && !targets.CheckFriendly())
            {
                return false;
            }

            if (card.activeData.owner != source.activeData.owner && !targets.CheckEnemy())
            {
                return false;
            }

            if ((targets.scope == TraitTargetScope.AllFriendlyNotSelf ||
                 targets.scope == TraitTargetScope.FriendlyUnitNotSelf ||
                 targets.scope == TraitTargetScope.RandomFriendlyNotSelf) && card.EqualsTo(source))
            {
                return false;
            }
        }

        if (!targets.DoesMatchType(card))
        {
            return false;
        }

        return true;
    }

    public virtual void Apply(Card card, Card source, ActiveTrait active)
    {
        if (deterable && card.IsCardTraitsDetered())
        {
            active.detered = true;
        }

        card.activeData.activeTraits.Add(active);
    }

    public virtual void Init(Card card, Card source, ActiveTrait active)
    {
    }

    public virtual void Deactivate(ActiveTrait active)
    {
        Card traitSource = active.GetTraitSource();
        Card traitTarget = active.GetTraitTarget();
        TraitActivateCCGEvent traitActivateCCGEvent = new TraitActivateCCGEvent();
        traitActivateCCGEvent.cardID = ((traitSource != null) ? traitSource.instanceId : 0);
        traitActivateCCGEvent.owner = (sbyte) ((traitSource != null) ? traitSource.activeData.owner : 0);
        traitActivateCCGEvent.traitID = traitParentID;
        traitActivateCCGEvent.effectID = effectTraitID;
        traitActivateCCGEvent.deactivate = true;
        if (traitTarget != null)
        {
            ActiveTraitCardInfo activeTraitCardInfo = new ActiveTraitCardInfo();
            activeTraitCardInfo.instanceId = traitTarget.instanceId;
            activeTraitCardInfo.owner = traitTarget.activeData.owner;
            traitActivateCCGEvent.targets = new ActiveTraitCardInfo[1];
            traitActivateCCGEvent.targets[0] = activeTraitCardInfo;
        }

        GameState.AddCCGEventLog(traitActivateCCGEvent);
    }

    public ActiveTrait GenerateActiveTrait(Card card, Card source)
    {
        ActiveTrait activeTrait = new ActiveTrait();
        activeTrait.Init(this, card, source, durationData);
        int num = 0;
        ActiveTrait activeTrait2 = null;
        for (int i = 0; i < card.activeData.activeTraits.Count; i++)
        {
            activeTrait2 = card.activeData.activeTraits[i];
            if (activeTrait2.GetTraitInfo() == null)
            {
                Console.WriteLine("Activated Trait is Missing Trait Data! effect:" + activeTrait2.traitEffectId +
                                  " trait:" + activeTrait2.traitSourceId);
            }
            else
            {
                num = activeTrait2.GetTraitInfo().GetOverrideData(activeTrait2);
                if (num != 0)
                {
                    activeTrait.dataValue = num;
                    break;
                }
            }
        }

        return activeTrait;
    }

    public ActiveTrait CheckEffectNegation(Card source)
    {
        ActiveTrait activeTrait = null;
        List<ActiveTrait> battleEffects = GameState.GetBattleEffects();
        for (int num = battleEffects.Count - 1; num >= 0; num--)
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

    public virtual bool CanDeploy(CardStack target, RegionEnum region)
    {
        return true;
    }

    public virtual bool CanDeployOverride(RegionEnum region)
    {
        return false;
    }

    public virtual bool CanMove(RegionEnum target, sbyte cardOwner, ActiveTrait active)
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

    public virtual void Move(CardStack location, RegionEnum region, bool embark, ActiveTrait active)
    {
    }

    public virtual void Attack(Card target, ActiveTrait active)
    {
    }

    public virtual void ActivateAction(CardStack location, RegionEnum region, ActiveTrait active)
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

    public virtual void OnCardMovedEvent(Card parent, Card movedCard, CardStack location, RegionEnum region,
        RegionEnum origin)
    {
    }

    public virtual void NewTurn(ActiveTrait active, sbyte playerIndex)
    {
    }

    public virtual void EndTurn(ActiveTrait active, sbyte playerIndex)
    {
    }

    public virtual bool CanDisembark()
    {
        return true;
    }

    public virtual void CardDeployed(Card deployed, ActiveTrait active)
    {
    }

    public virtual void CardMoved(Card theCard, CardStack target, RegionEnum destination, RegionEnum origin,
        ActiveTrait active)
    {
    }

    public virtual void CardAttacked(Card attacker, Card target, ActiveTrait active)
    {
    }

    public virtual void CardCounterAttacked(Card attacker, Card target, ActiveTrait active)
    {
    }

    public virtual void CardActivateTrait(Card source, Card target, ActiveTrait active)
    {
    }

    public virtual void CardHacked(Card runner, Card target, ActiveTrait active)
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

    public virtual void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, RegionEnum region,
        ActiveTrait active)
    {
    }
}