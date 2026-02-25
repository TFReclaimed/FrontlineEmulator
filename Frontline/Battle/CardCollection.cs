namespace Frontline.Battle;

public class CardCollection
{
    public List<Card> Cards { get; set; }

    public void Create(sbyte drawCount, Deck theDeck, CCG game, sbyte playerIndex)
    {
        if (drawCount > 0)
        {
            Cards = new List<Card>(drawCount);
            for (var i = 0; i < drawCount; i++)
            {
                DrawFromDeck(theDeck, game, playerIndex);
            }
        }
        else
        {
            Cards = [];
        }
    }

    public void Init(CCG game)
    {
        for (var i = 0; i < Cards.Count; i++)
        {
            Cards[i] = Cards[i].GenerateAndInit(game);
        }
    }

    public void InitActiveData()
    {
        foreach (var card in Cards)
        {
            card.InitActiveData();
        }
    }

    public Card? DrawFromDeck(Deck theDeck, CCG game, sbyte playerIndex)
    {
        var card = theDeck.DrawCard(game);
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
}