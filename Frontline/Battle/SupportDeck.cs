using System.Text.Json.Serialization;
using Frontline.Battle.CcgEvents;
using Frontline.Game;

namespace Frontline.Battle;

public class SupportDeck : Deck
{
    public sbyte CurrentSupport { get; set; }

    public Card? Repeater { get; set; }

    public Card? Ultimate { get; set; }

    public bool CanRepeat { get; set; }

    [JsonPropertyName("noshuffle")]
    public bool NoShuffle { get; set; }

    private readonly CCG _gameState;

    public SupportDeck(CCG gameState, List<Card> cards, sbyte playerIndex, bool skipShuffle)
        : base(cards)
    {
        _gameState = gameState;

        for (var i = 0; i < Cards.Count; i++)
        {
            Cards[i] = Cards[i].GenerateAndInit(_gameState);
            Cards[i].ActiveData.Owner = playerIndex;
            Cards[i].Setup();
        }

        Repeater = Cards[0];
        Ultimate = Cards[^1];
        NoShuffle = skipShuffle;
        CurrentSupport = (sbyte) Cards.Count;
    }

    public override void Shuffle(bool skip)
    {
        if (!skip)
        {
            base.Shuffle(skip);
        }
        else if (CurrentSupport == Cards.Count - 1)
        {
            var index = Cards.Count - 1;
            var item = Cards[index];
            Cards.RemoveAt(index);
            Cards.Insert(0, item);
        }
    }

    public void NewTurn(sbyte commandAccum)
    {
        CanRepeat = true;
        DrawCard(commandAccum, true);
    }

    public void DrawCard(sbyte commandAccum, bool isNewTurn)
    {
        if (Cards.Count <= 0)
        {
            return;
        }

        if (CanRepeat && Repeater != null)
        {
            CanRepeat = false;
            var hasRepeater = Cards.Any(t => t.TemplateId == Repeater.TemplateId);

            if (!hasRepeater)
            {
                var cardTemplate = RulesetParser.GetCardTemplate(Repeater.TemplateId, Repeater.Rank);
                if (cardTemplate == null)
                {
                    return;
                }

                var card = cardTemplate.GenerateCard(_gameState);
                card.InstanceId = _gameState.GetNextSummonInstanceId();
                card.ActiveData.Owner = Repeater.ActiveData.Owner;
                card.Setup();
                card.InitActiveData();
                _gameState.Logger.Debug("**** SupportDeck.DrawCard - Spanwed New Card * " + card.InstanceId);
                Cards.Add(card);
            }
        }

        Shuffle(NoShuffle);
        CurrentSupport = (sbyte) (Cards.Count - 1);
        if (Ultimate != null && Ultimate.GetTemplate().Cost == commandAccum)
        {
            for (int num = CurrentSupport; num >= 0; num--)
            {
                if (Cards[num].TemplateId == Ultimate.TemplateId)
                {
                    (Cards[num], Cards[CurrentSupport]) = (Cards[CurrentSupport], Cards[num]);
                    break;
                }
            }

            Ultimate = null;
        }
        else
        {
            for (int num2 = CurrentSupport; num2 >= 0; num2--)
            {
                if (Cards[num2].GetTemplate().Cost <= commandAccum)
                {
                    (Cards[num2], Cards[CurrentSupport]) = (Cards[CurrentSupport], Cards[num2]);
                    break;
                }
            }
        }

        var card2 = Cards[CurrentSupport];
        var cardDrawEvent = new CardDrawCcgEvent(CcgEventType.SupportDraw, card2.InstanceId,
            card2.ActiveData.Owner, card2.TemplateId, card2.Rank);
        _gameState.AddCCGEventLog(cardDrawEvent);
        _gameState.CardDrawn(card2, false, isNewTurn);
    }

    public Card? GetCurrent()
    {
        if (CurrentSupport >= 0 && CurrentSupport < Cards.Count)
        {
            return Cards[CurrentSupport];
        }

        return null;
    }

    public Card? DeployCard(int cardId)
    {
        Card? result = null;
        if (CurrentSupport < Cards.Count)
        {
            var card = Cards[CurrentSupport];
            if (card.InstanceId == cardId)
            {
                result = card;
                Cards.RemoveAt(CurrentSupport);
                CurrentSupport = (sbyte) Cards.Count;
            }
        }

        return result;
    }
}