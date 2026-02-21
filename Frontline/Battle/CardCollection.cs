namespace Frontline.Battle;

public class CardCollection
{
    public List<Card> cards;

    public void Create(sbyte drawCount, Deck theDeck, CCG game, sbyte playerIndex)
    {
        if (drawCount > 0)
        {
            cards = new List<Card>(drawCount);
            for (int i = 0; i < drawCount; i++)
            {
                DrawFromDeck(theDeck, game, playerIndex);
            }
        }
        else
        {
            cards = new List<Card>();
        }
    }

    public void Init(CCG game)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i] = cards[i].GenerateAndInit(game);
        }
    }

    public void InitActiveData()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].InitActiveData();
        }
    }

    public Card DrawFromDeck(Deck theDeck, CCG game, sbyte playerIndex)
    {
        Card card = theDeck.DrawCard(game);
        if (card != null)
        {
            cards.Add(card);
            card.activeData.owner = playerIndex;
            card.xp = 0;
        }

        return card;
    }

    public Card FindCard(int cardId)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            if (card.instanceId == cardId)
            {
                return card;
            }
        }

        return null;
    }

    public Card RemoveCard(int cardId)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            if (card.instanceId == cardId)
            {
                cards.RemoveAt(i);
                return card;
            }
        }

        return null;
    }
}