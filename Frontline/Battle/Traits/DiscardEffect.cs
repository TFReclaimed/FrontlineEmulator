using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.Traits;

public class DiscardEffect : BaseTraitEffect
{
    public sbyte NumberOfCards { get; set; } = 1;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte owner = card.ActiveData.Owner;
        Player player = GameState.Players[owner];
        int num = player.Hand.Cards.Count;
        int[] array = null;
        Card card2 = null;
        MulliganDrawCCGEventCardData[] array2 = null;
        if (num == 0)
        {
            return;
        }

        sbyte b = NumberOfCards;
        if (NumberOfCards > 0 && active.DataValue > 0)
        {
            b = (sbyte) active.DataValue;
        }

        if (b > num)
        {
            array = new int[num];
            array2 = new MulliganDrawCCGEventCardData[num];
            for (int i = 0; i < num; i++)
            {
                card2 = player.Hand.Cards[i];
                array[i] = card2.InstanceId;
                array2[i] = new MulliganDrawCCGEventCardData(card2.InstanceId, card2.TemplateId, card2.Rank);
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
                card2 = player.Hand.Cards[num3];
                num3 = card2.InstanceId;
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
                    array2[num2] = new MulliganDrawCCGEventCardData(card2.InstanceId, card2.TemplateId, card2.Rank);
                    num2++;
                }
            }

            num = b;
        }

        DiscardEffectCCGEvent discardEffectCCGEvent = new DiscardEffectCCGEvent(owner, array2);
        discardEffectCCGEvent.EffectId = EffectTraitId;
        discardEffectCCGEvent.TraitId = TraitParentId;
        GameState.AddCCGEventLog(discardEffectCCGEvent);
        GameState.DoCardDiscard(owner, array);
        GameState.CardDiscardEffect(owner, num);
    }
}