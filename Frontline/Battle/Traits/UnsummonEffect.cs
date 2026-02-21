using Frontline.Battle.CcgEvents;
using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class UnsummonEffect : BaseTraitEffect
{
    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte owner = card.activeData.owner;
        Player player = GameState.players[owner];
        CardTransitionCCGEvent cardTransitionCCGEvent = null;
        if (targets.area == TargetableArea.EnemyDiscard || targets.area == TargetableArea.FriendlyDiscard)
        {
            CardCollection discard = player.discard;
            discard.RemoveCard(card.instanceId);
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

            card2 = list[0].RemoveCard(card.instanceId, owner);
            list2 = card2.GetSecrets();
            if (list2 != null)
            {
                for (int num = list2.Count - 1; num >= 0; num--)
                {
                    list2[num].Discard(GameState.players);
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
                        list2[num2].Discard(GameState.players);
                        GameState.SecretDestroyed(list2[num2], source);
                    }
                }

                card2.ResetCard();
                player.hand.cards.Add(card2);
                cardTransitionCCGEvent = new CardTransitionCCGEvent(CCGEventType.CardUnsummon, card2.instanceId,
                    card2.activeData.owner, 0, 0, false, RegionEnum.NumRegions, 0, 0);
                cardTransitionCCGEvent.templateId = card2.templateId;
                cardTransitionCCGEvent.rank = card2.rank;
                cardTransitionCCGEvent.effectID = effectTraitID;
                cardTransitionCCGEvent.traitID = traitParentID;
                GameState.AddCCGEventLog(cardTransitionCCGEvent);
            }
        }

        card.ResetCard();
        player.hand.cards.Add(card);
        cardTransitionCCGEvent = new CardTransitionCCGEvent(CCGEventType.CardUnsummon, card.instanceId,
            card.activeData.owner, 0, 0, false, RegionEnum.NumRegions, 0, 0);
        cardTransitionCCGEvent.templateId = card.templateId;
        cardTransitionCCGEvent.rank = card.rank;
        cardTransitionCCGEvent.effectID = effectTraitID;
        cardTransitionCCGEvent.traitID = traitParentID;
        GameState.AddCCGEventLog(cardTransitionCCGEvent);
    }
}