namespace Frontline.Battle;

public class Deck
{
    public List<Card> cards;

    public sbyte count;

    public virtual void Shuffle(bool skip)
    {
        if (!skip)
        {
            for (int num = cards.Count - 1; num > 1; num--)
            {
                int index = Random.Shared.Next(0, num);
                Card value = cards[num];
                cards[num] = cards[index];
                cards[index] = value;
            }
        }
    }

    public Card DrawCard(CCG game)
    {
        int num = cards.Count - 1;
        if (num < 0)
        {
            return null;
        }

        Card card = cards[num];
        cards.RemoveAt(num);
        count--;
        card = card.GenerateAndInit(game);
        card.Setup();
        card.InitActiveData();
        return card;
    }

    public void InsertCardAtIndex(Card card, int index)
    {
        if (index >= cards.Count)
        {
            index = cards.Count - 1;
        }

        if (index < 0)
        {
            index = 0;
        }

        cards.Insert(index, card);
        count++;
    }

    public Card FindCard(int cardId)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].instanceId == cardId)
            {
                return cards[i];
            }
        }

        return null;
    }
}