namespace Frontline.Battle;

public class CardCollection
{
    public List<Card> Cards { get; set; }

    public void Create(sbyte drawCount, Deck theDeck, CCG game, sbyte playerIndex)
    {
        if (drawCount > 0)
        {
            Cards = new List<Card>(drawCount);
            for (int i = 0; i < drawCount; i++)
            {
                DrawFromDeck(theDeck, game, playerIndex);
            }
        }
        else
        {
            Cards = new List<Card>();
        }
    }

    public void Init(CCG game)
    {
        for (int i = 0; i < Cards.Count; i++)
        {
            Cards[i] = Cards[i].GenerateAndInit(game);
        }
    }

    public void InitActiveData()
    {
        for (int i = 0; i < Cards.Count; i++)
        {
            Cards[i].InitActiveData();
        }
    }

    public Card DrawFromDeck(Deck theDeck, CCG game, sbyte playerIndex)
    {
        Card card = theDeck.DrawCard(game);
        if (card != null)
        {
            Cards.Add(card);
            card.ActiveData.Owner = playerIndex;
            card.Xp = 0;
        }

        return card;
    }

    public Card FindCard(int cardId)
    {
        for (int i = 0; i < Cards.Count; i++)
        {
            Card card = Cards[i];
            if (card.InstanceId == cardId)
            {
                return card;
            }
        }

        return null;
    }

    public Card RemoveCard(int cardId)
    {
        for (int i = 0; i < Cards.Count; i++)
        {
            Card card = Cards[i];
            if (card.InstanceId == cardId)
            {
                Cards.RemoveAt(i);
                return card;
            }
        }

        return null;
    }
}