using System.Text.Json.Serialization;

namespace Frontline.Battle;

public class Deck
{
    [JsonInclude]
    public readonly List<Card> Cards;

    [JsonInclude]
    public sbyte Count => (sbyte) Cards.Count;

    public Deck(List<Card> cards)
    {
        Cards = cards;
    }

    public virtual void Shuffle(bool skip)
    {
        if (skip)
        {
            return;
        }

        for (var num = Cards.Count - 1; num > 1; num--)
        {
            var index = Random.Shared.Next(0, num);
            (Cards[num], Cards[index]) = (Cards[index], Cards[num]);
        }
    }

    public Card? DrawCard(CCG game)
    {
        var num = Cards.Count - 1;
        if (num < 0)
        {
            return null;
        }

        var card = Cards[num];
        Cards.RemoveAt(num);
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
    }

    public Card? FindCard(int cardId)
    {
        return Cards.FirstOrDefault(card => card.InstanceId == cardId);
    }
}