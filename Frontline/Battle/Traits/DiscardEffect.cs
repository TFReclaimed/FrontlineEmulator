using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.Traits;

public class DiscardEffect : BaseTraitEffect
{
    public sbyte numberOfCards = 1;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte owner = card.activeData.owner;
        Player player = GameState.players[owner];
        int num = player.hand.cards.Count;
        int[] array = null;
        Card card2 = null;
        MulliganDrawCCGEventCardData[] array2 = null;
        if (num == 0)
        {
            return;
        }

        sbyte b = numberOfCards;
        if (numberOfCards > 0 && active.dataValue > 0)
        {
            b = (sbyte) active.dataValue;
        }

        if (b > num)
        {
            array = new int[num];
            array2 = new MulliganDrawCCGEventCardData[num];
            for (int i = 0; i < num; i++)
            {
                card2 = player.hand.cards[i];
                array[i] = card2.instanceId;
                array2[i] = new MulliganDrawCCGEventCardData(card2.instanceId, card2.templateId, card2.rank);
            }
        }
        else
        {
            int num2 = 0;
            int num3 = 0;
            bool flag = false;
            array = new int[b];
            array2 = new MulliganDrawCCGEventCardData[b];
            while (num2 < b)
            {
                flag = false;
                num3 = GameState.GetGame().GetServerIntValue(0, num);
                card2 = player.hand.cards[num3];
                num3 = card2.instanceId;
                for (int j = 0; j < num2; j++)
                {
                    if (array[j] == num3)
                    {
                        flag = true;
                    }
                }

                if (!flag)
                {
                    array[num2] = num3;
                    array2[num2] = new MulliganDrawCCGEventCardData(card2.instanceId, card2.templateId, card2.rank);
                    num2++;
                }
            }

            num = b;
        }

        DiscardEffectCCGEvent discardEffectCCGEvent = new DiscardEffectCCGEvent(owner, array2);
        discardEffectCCGEvent.effectId = effectTraitID;
        discardEffectCCGEvent.traitId = traitParentID;
        GameState.AddCCGEventLog(discardEffectCCGEvent);
        GameState.DoCardDiscard(owner, array);
        GameState.CardDiscardEffect(owner, num);
    }
}