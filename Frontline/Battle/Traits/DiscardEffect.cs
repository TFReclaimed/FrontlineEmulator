using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.Traits;

public class DiscardEffect : BaseTraitEffect
{
    public sbyte NumberOfCards { get; set; } = 1;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var owner = card.ActiveData.Owner;
        var player = GameState.Players[owner];
        var num = player.Hand.Cards.Count;
        int[] array = null;
        Card card2 = null;
        MulliganDrawCcgEventCardData[] array2 = null;
        if (num == 0)
        {
            return;
        }

        var b = NumberOfCards;
        if (NumberOfCards > 0 && active.DataValue > 0)
        {
            b = (sbyte) active.DataValue;
        }

        if (b > num)
        {
            array = new int[num];
            array2 = new MulliganDrawCcgEventCardData[num];
            for (var i = 0; i < num; i++)
            {
                card2 = player.Hand.Cards[i];
                array[i] = card2.InstanceId;
                array2[i] = new MulliganDrawCcgEventCardData(card2.InstanceId, card2.TemplateId, card2.Rank);
            }
        }
        else
        {
            var num2 = 0;
            var num3 = 0;
            var flag = false;
            array = new int[b];
            array2 = new MulliganDrawCcgEventCardData[b];
            while (num2 < b)
            {
                flag = false;
                num3 = GameState.GetGame().GetServerIntValue(0, num);
                card2 = player.Hand.Cards[num3];
                num3 = card2.InstanceId;
                for (var j = 0; j < num2; j++)
                {
                    if (array[j] == num3)
                    {
                        flag = true;
                    }
                }

                if (!flag)
                {
                    array[num2] = num3;
                    array2[num2] = new MulliganDrawCcgEventCardData(card2.InstanceId, card2.TemplateId, card2.Rank);
                    num2++;
                }
            }

            num = b;
        }

        var discardEffectCCGEvent = new DiscardEffectCcgEvent(owner, array2);
        discardEffectCCGEvent.EffectId = EffectTraitId;
        discardEffectCCGEvent.TraitId = TraitParentId;
        GameState.AddCCGEventLog(discardEffectCCGEvent);
        GameState.DoCardDiscard(owner, array);
        GameState.CardDiscardEffect(owner, num);
    }
}