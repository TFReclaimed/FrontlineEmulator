namespace Frontline.Battle;

public class CardCollection
{
    public List<Card> Cards { get; set; } = [];

    public CardCollection(sbyte drawCount, Deck? deck, CcgGameState gameState, sbyte playerIndex)
    {
        if (drawCount <= 0)
        {
            return;
        }

        Cards = new List<Card>(drawCount);
        for (var i = 0; i < drawCount; i++)
        {
            DrawFromDeck(deck!, gameState, playerIndex);
        }
    }

    public Card? DrawFromDeck(Deck deck, CcgGameState gameState, sbyte playerIndex)
    {
        var card = deck.DrawCard(gameState);
        if (card != null)
        {
            Cards.Add(card);
            card.ActiveData.Owner = playerIndex;
            card.Xp = 0;
        }

        return card;
    }

    public Card? FindCard(int cardId)
    {
        return Cards.FirstOrDefault(card => card.InstanceId == cardId);
    }

    public Card? RemoveCard(int cardId)
    {
        for (var i = 0; i < Cards.Count; i++)
        {
            var card = Cards[i];
            if (card.InstanceId == cardId)
            {
                Cards.RemoveAt(i);
                return card;
            }
        }

        return null;
    }

    public List<Card> GetHighestCostCards(int numCards)
    {
        if (numCards > Cards.Count)
        {
            return Cards;
        }

        var cards = new List<Card>();
        cards.AddRange(Cards);
        cards.Sort(Card.SortByCommandCostDescending);

        var finalCards = new List<Card>();
        for (var i = 0; i < numCards; i++)
        {
            var card = cards[i];
            finalCards.Add(card);
        }

        return finalCards;
    }
}