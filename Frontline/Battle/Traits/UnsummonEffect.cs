using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.Traits;

public class UnsummonEffect : BaseTraitEffect
{
    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var owner = card.ActiveData.Owner;
        var player = GameState.Players[owner];
        if (Targets.Area == TargetableArea.EnemyDiscard || Targets.Area == TargetableArea.FriendlyDiscard)
        {
            var discard = player.Discard;
            discard.RemoveCard(card.InstanceId);
        }
        else
        {
            var list = GameState.FindCardStack(card);
            List<Card> list2 = null;
            Card? card2;
            if (list == null || list.Count == 0)
            {
                return;
            }

            card2 = list[0].RemoveCard(card.InstanceId, owner);
            list2 = card2.GetSecrets();
            if (list2 != null)
            {
                for (var num = list2.Count - 1; num >= 0; num--)
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
                    for (var num2 = list2.Count - 1; num2 >= 0; num2--)
                    {
                        list2[num2].Discard(GameState.Players);
                        GameState.SecretDestroyed(list2[num2], source);
                    }
                }

                card2.ResetCard();
                player.Hand.Cards.Add(card2);
                var cardTransitionCcgEvent = new CardTransitionCcgEvent(CcgEventType.CardUnsummon, card2.InstanceId,
                    card2.ActiveData.Owner, 0, 0, false, Region.NumRegions, 0, 0)
                {
                    TemplateId = card2.TemplateId,
                    Rank = card2.Rank,
                    EffectId = EffectTraitId,
                    TraitId = TraitParentId
                };
                GameState.AddCcgEventLog(cardTransitionCcgEvent);
            }
        }

        card.ResetCard();
        player.Hand.Cards.Add(card);
        var cardTransitionCcgEvent2 = new CardTransitionCcgEvent(CcgEventType.CardUnsummon, card.InstanceId,
            card.ActiveData.Owner, 0, 0, false, Region.NumRegions, 0, 0)
        {
            TemplateId = card.TemplateId,
            Rank = card.Rank,
            EffectId = EffectTraitId,
            TraitId = TraitParentId
        };
        GameState.AddCcgEventLog(cardTransitionCcgEvent2);
    }
}