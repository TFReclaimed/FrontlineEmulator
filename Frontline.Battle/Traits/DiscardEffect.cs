using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.Traits;

public class DiscardEffect : BaseTraitEffect
{
    public sbyte NumberOfCards { get; set; } = 1;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var owner = card.ActiveData.Owner;
        var player = GameState.Players[owner];
        var cardsInHand = player.Hand.Cards.Count;
        int[] cardsToDiscard;
        MulliganDrawCcgEventCardData[] cardDatas;
        if (cardsInHand == 0)
        {
            return;
        }

        var discardCount = NumberOfCards;
        if (NumberOfCards > 0 && active.DataValue > 0)
        {
            discardCount = (sbyte) active.DataValue;
        }

        if (discardCount > cardsInHand)
        {
            cardsToDiscard = new int[cardsInHand];
            cardDatas = new MulliganDrawCcgEventCardData[cardsInHand];
            for (var i = 0; i < cardsInHand; i++)
            {
                var card2 = player.Hand.Cards[i];
                cardsToDiscard[i] = card2.InstanceId;
                cardDatas[i] = new MulliganDrawCcgEventCardData(card2.InstanceId, card2.TemplateId, card2.Rank);
            }
        }
        else
        {
            var discardedCount = 0;
            cardsToDiscard = new int[discardCount];
            cardDatas = new MulliganDrawCcgEventCardData[discardCount];
            while (discardedCount < discardCount)
            {
                var alreadyDiscarded = false;
                var index = GameState.GetGame().GetServerIntValue(0, cardsInHand);
                var card2 = player.Hand.Cards[index];
                var cardId = card2.InstanceId;
                for (var j = 0; j < discardedCount; j++)
                {
                    if (cardsToDiscard[j] == cardId)
                    {
                        alreadyDiscarded = true;
                    }
                }

                if (!alreadyDiscarded)
                {
                    cardsToDiscard[discardedCount] = cardId;
                    cardDatas[discardedCount] = new MulliganDrawCcgEventCardData(card2.InstanceId, card2.TemplateId, card2.Rank);
                    discardedCount++;
                }
            }

            cardsInHand = discardCount;
        }

        var discardEvent = new DiscardEffectCcgEvent(owner, cardDatas)
        {
            EffectId = EffectTraitId,
            TraitId = TraitParentId
        };

        GameState.AddCcgEventLog(discardEvent);
        GameState.DoCardDiscard(owner, cardsToDiscard);
        GameState.CardDiscardEffect(owner, cardsInHand);
    }
}