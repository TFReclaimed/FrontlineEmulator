namespace Frontline.Battle;

public class Deck
{
    public List<Card> Cards { get; set; }

    public sbyte Count { get; set; }

    public virtual void Shuffle(bool skip)
    {
        if (!skip)
        {
            for (int num = Cards.Count - 1; num > 1; num--)
            {
                int index = Random.Shared.Next(0, num);
                Card value = Cards[num];
                Cards[num] = Cards[index];
                Cards[index] = value;
            }
        }
    }

    public Card DrawCard(CCG game)
    {
        int num = Cards.Count - 1;
        if (num < 0)
        {
            return null;
        }

        Card card = Cards[num];
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
        for (int i = 0; i < Cards.Count; i++)
        {
            if (Cards[i].InstanceId == cardId)
            {
                return Cards[i];
            }
        }

        return null;
    }
}