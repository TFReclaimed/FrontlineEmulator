namespace Frontline.Battle;

public class Deck
{
    public List<Card> Cards { get; set; }

    public sbyte Count { get; set; }

    public virtual void Shuffle(bool skip)
    {
        if (!skip)
        {
            for (var num = Cards.Count - 1; num > 1; num--)
            {
                var index = Random.Shared.Next(0, num);
                var value = Cards[num];
                Cards[num] = Cards[index];
                Cards[index] = value;
            }
        }
    }

    public Card DrawCard(CCG game)
    {
        var num = Cards.Count - 1;
        if (num < 0)
        {
            return null;
        }

        var card = Cards[num];
        Cards.RemoveAt(num);
        Count--;
        card = card.GenerateAndInit(game);
        card.Setup();
        card.InitActiveData();
        return card;
    }

    public void InsertCardAtIndex(Card card, int index)
    {
        if (index >= Cards.Count)
        {
            index = Cards.Count - 1;
        }

        if (index < 0)
        {
            index = 0;
        }

        Cards.Insert(index, card);
        Count++;
    }

    public Card FindCard(int cardId)
    {
        for (var i = 0; i < Cards.Count; i++)
        {
            if (Cards[i].InstanceId == cardId)
            {
                return Cards[i];
            }
        }

        return null;
    }
}