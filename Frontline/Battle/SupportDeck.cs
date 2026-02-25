using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class SupportDeck : Deck
{
    public sbyte CurrentSupport { get; set; }

    public Card Repeater { get; set; }

    public Card Ultimate { get; set; }

    public bool CanRepeat { get; set; }

    public bool Noshuffle { get; set; }

    private readonly CCG _gameState;

    public SupportDeck(CCG gameState)
    {
        _gameState = gameState;
    }

    public void Create(List<Card> support, CCG game, sbyte playerIndex, bool skipShuffle)
    {
        Cards = support;
        for (int i = 0; i < Cards.Count; i++)
        {
            Cards[i] = Cards[i].GenerateAndInit(game);
            Cards[i].ActiveData.Owner = playerIndex;
            Cards[i].Setup();
        }

        Count = (sbyte) Cards.Count;
        Repeater = Cards[0];
        Ultimate = Cards[Cards.Count - 1];
        Noshuffle = skipShuffle;
        if (!Noshuffle)
        {
            Shuffle(Noshuffle);
        }

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
            int index = Cards.Count - 1;
            Card item = Cards[index];
            Cards.RemoveAt(index);
            Cards.Insert(0, item);
        }
    }

    public void Init(CCG game, sbyte playerIndex)
    {
        for (int i = 0; i < Cards.Count; i++)
        {
            Cards[i] = Cards[i].GenerateAndInit(game);
            Cards[i].ActiveData.Owner = playerIndex;
        }

        if (Repeater != null)
        {
            Repeater.Init();
        }

        if (Ultimate != null)
        {
            Ultimate.Init();
        }
    }

    public void InitActiveData()
    {
        for (int i = 0; i < Cards.Count; i++)
        {
            Cards[i].InitActiveData();
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
            bool flag = false;
            for (int i = 0; i < Cards.Count; i++)
            {
                if (Cards[i].TemplateId == Repeater.TemplateId)
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                CardTemplate cardTemplate = RulesetParser.GetCardTemplate(Repeater.TemplateId, Repeater.Rank);
                if (cardTemplate == null)
                {
                    return;
                }

                Card card = cardTemplate.GenerateCard(_gameState);
                card.InstanceId = _gameState.GetNextSummonInstanceId();
                card.ActiveData.Owner = Repeater.ActiveData.Owner;
                card.Setup();
                card.InitActiveData();
                Console.WriteLine("**** SupportDeck.DrawCard - Spanwed New Card * " + card.InstanceId);
                Cards.Add(card);
            }
        }

        Shuffle(Noshuffle);
        CurrentSupport = (sbyte) (Cards.Count - 1);
        if (Ultimate != null && Ultimate.GetTemplate().Cost == commandAccum)
        {
            for (int num = CurrentSupport; num >= 0; num--)
            {
                if (Cards[num].TemplateId == Ultimate.TemplateId)
                {
                    Card value = Cards[num];
                    Cards[num] = Cards[CurrentSupport];
                    Cards[CurrentSupport] = value;
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
                    Card value2 = Cards[num2];
                    Cards[num2] = Cards[CurrentSupport];
                    Cards[CurrentSupport] = value2;
                    break;
                }
            }
        }

        Card card2 = Cards[CurrentSupport];
        CardDrawCCGEvent logData = new CardDrawCCGEvent(CcgEventType.SupportDraw, card2.InstanceId,
            card2.ActiveData.Owner, card2.TemplateId, card2.Rank);
        _gameState.AddCCGEventLog(logData);
        _gameState.CardDrawn(card2, false, isNewTurn);
    }

    public Card GetCurrent()
    {
        if (CurrentSupport >= 0 && CurrentSupport < Cards.Count)
        {
            return Cards[CurrentSupport];
        }

        return null;
    }

    public Card DeployCard(int cardId)
    {
        Card result = null;
        if (CurrentSupport < Cards.Count)
        {
            Card card = Cards[CurrentSupport];
            if (card.InstanceId == cardId)
            {
                result = card;
                Cards.RemoveAt(CurrentSupport);
                Count--;
                CurrentSupport = (sbyte) Cards.Count;
            }
        }

        return result;
    }
}