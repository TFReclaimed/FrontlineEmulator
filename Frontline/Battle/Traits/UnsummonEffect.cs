using Frontline.Battle.CcgEvents;
using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class UnsummonEffect : BaseTraitEffect
{
    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte owner = card.ActiveData.Owner;
        Player player = GameState.Players[owner];
        CardTransitionCCGEvent cardTransitionCCGEvent = null;
        if (Targets.Area == TargetableArea.EnemyDiscard || Targets.Area == TargetableArea.FriendlyDiscard)
        {
            CardCollection discard = player.Discard;
            discard.RemoveCard(card.InstanceId);
        }
        else
        {
            List<CardStack> list = GameState.FindCardStack(card);
            List<Card> list2 = null;
            Card card2 = null;
            bool flag = card.GetTemplate().Type == CardType.Titan;
            if (list == null || list.Count == 0)
            {
                return;
            }

            card2 = list[0].RemoveCard(card.InstanceId, owner);
            list2 = card2.GetSecrets();
            if (list2 != null)
            {
                for (int num = list2.Count - 1; num >= 0; num--)
                {
                    list2[num].Discard(GameState.Players);
                    GameState.SecretDestroyed(list2[num], source);
                }
            }

            if (card2.HasPilot())
            {
                card2 = card2.GetEmbarkedPilot();
                list2 = card2.GetSecrets();
                if (list2 != null)
                {
                    for (int num2 = list2.Count - 1; num2 >= 0; num2--)
                    {
                        list2[num2].Discard(GameState.Players);
                        GameState.SecretDestroyed(list2[num2], source);
                    }
                }

                card2.ResetCard();
                player.Hand.Cards.Add(card2);
                cardTransitionCCGEvent = new CardTransitionCCGEvent(CCGEventType.CardUnsummon, card2.InstanceId,
                    card2.ActiveData.Owner, 0, 0, false, RegionEnum.NumRegions, 0, 0);
                cardTransitionCCGEvent.TemplateId = card2.TemplateId;
                cardTransitionCCGEvent.Rank = card2.Rank;
                cardTransitionCCGEvent.EffectId = EffectTraitId;
                cardTransitionCCGEvent.TraitId = TraitParentId;
                GameState.AddCCGEventLog(cardTransitionCCGEvent);
            }
        }

        card.ResetCard();
        player.Hand.Cards.Add(card);
        cardTransitionCCGEvent = new CardTransitionCCGEvent(CCGEventType.CardUnsummon, card.InstanceId,
            card.ActiveData.Owner, 0, 0, false, RegionEnum.NumRegions, 0, 0);
        cardTransitionCCGEvent.TemplateId = card.TemplateId;
        cardTransitionCCGEvent.Rank = card.Rank;
        cardTransitionCCGEvent.EffectId = EffectTraitId;
        cardTransitionCCGEvent.TraitId = TraitParentId;
        GameState.AddCCGEventLog(cardTransitionCCGEvent);
    }
}