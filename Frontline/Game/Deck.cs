using System.Text.Json.Serialization;

namespace Frontline.Game;

public class Deck
{
    public required List<GameCard> Cards { get; set; }
    [JsonInclude]
    public sbyte Count => (sbyte) Cards.Count;

    public GameCard? DrawCard()
    {
        if (Cards.Count == 0)
        {
            return null;
        }
        
        var card = Cards[0];
        Cards.RemoveAt(0);
        
        return card;
    }

    public void InsertCardAtIndex(GameCard card, int index)
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
    }
}

public class SupportDeck : Deck
{
    public sbyte CurrentSupport { get; set; }
    public GameCard? Repeater { get; set; }
    public GameCard? Ultimate { get; set; }
    public bool CanRepeat { get; set; }
    public bool NoShuffle { get; set; }
}

public class CardCollection
{
    public required List<GameCard> Cards { get; set; }

    public GameCard? DrawFromDeck(Deck deck, sbyte playerIndex)
    {
        var card = deck.DrawCard();
        if (card is not null)
        {
            card.ActiveData = new ActiveUnitCardData { Owner = playerIndex };
            Cards.Add(card);
        }

        return card;
    }

    public GameCard? RemoveCard(int cardId)
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
}

public class CardStack
{
    public GameCard? PrimaryCard { get; set; }
}