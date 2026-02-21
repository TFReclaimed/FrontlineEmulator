using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class SupportDeck : Deck
{
    public sbyte currentSupport;

    public Card repeater;

    public Card ultimate;

    public bool canRepeat;

    public bool noshuffle;

    private readonly CCG _gameState;

    public SupportDeck(CCG gameState)
    {
        _gameState = gameState;
    }

    public void Create(List<Card> support, CCG game, sbyte playerIndex, bool skipShuffle)
    {
        cards = support;
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i] = cards[i].GenerateAndInit(game);
            cards[i].activeData.owner = playerIndex;
            cards[i].Setup();
        }

        count = (sbyte) cards.Count;
        repeater = cards[0];
        ultimate = cards[cards.Count - 1];
        noshuffle = skipShuffle;
        if (!noshuffle)
        {
            Shuffle(noshuffle);
        }

        currentSupport = (sbyte) cards.Count;
    }

    public override void Shuffle(bool skip)
    {
        if (!skip)
        {
            base.Shuffle(skip);
        }
        else if (currentSupport == cards.Count - 1)
        {
            int index = cards.Count - 1;
            Card item = cards[index];
            cards.RemoveAt(index);
            cards.Insert(0, item);
        }
    }

    public void Init(CCG game, sbyte playerIndex)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i] = cards[i].GenerateAndInit(game);
            cards[i].activeData.owner = playerIndex;
        }

        if (repeater != null)
        {
            repeater.Init();
        }

        if (ultimate != null)
        {
            ultimate.Init();
        }
    }

    public void InitActiveData()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].InitActiveData();
        }
    }

    public void NewTurn(sbyte commandAccum)
    {
        canRepeat = true;
        DrawCard(commandAccum, true);
    }

    public void DrawCard(sbyte commandAccum, bool isNewTurn)
    {
        if (cards.Count <= 0)
        {
            return;
        }

        if (canRepeat && repeater != null)
        {
            canRepeat = false;
            bool flag = false;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].templateId == repeater.templateId)
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                CardTemplate cardTemplate = RulesetParser.GetCardTemplate(repeater.templateId, repeater.rank);
                if (cardTemplate == null)
                {
                    return;
                }

                Card card = cardTemplate.GenerateCard(_gameState);
                card.instanceId = _gameState.GetNextSummonInstanceId();
                card.activeData.owner = repeater.activeData.owner;
                card.Setup();
                card.InitActiveData();
                Console.WriteLine("**** SupportDeck.DrawCard - Spanwed New Card * " + card.instanceId);
                cards.Add(card);
            }
        }

        Shuffle(noshuffle);
        currentSupport = (sbyte) (cards.Count - 1);
        if (ultimate != null && ultimate.GetTemplate().Cost == commandAccum)
        {
            for (int num = currentSupport; num >= 0; num--)
            {
                if (cards[num].templateId == ultimate.templateId)
                {
                    Card value = cards[num];
                    cards[num] = cards[currentSupport];
                    cards[currentSupport] = value;
                    break;
                }
            }

            ultimate = null;
        }
        else
        {
            for (int num2 = currentSupport; num2 >= 0; num2--)
            {
                if (cards[num2].GetTemplate().Cost <= commandAccum)
                {
                    Card value2 = cards[num2];
                    cards[num2] = cards[currentSupport];
                    cards[currentSupport] = value2;
                    break;
                }
            }
        }

        Card card2 = cards[currentSupport];
        CardDrawCCGEvent logData = new CardDrawCCGEvent(CCGEventType.SupportDraw, card2.instanceId,
            card2.activeData.owner, card2.templateId, card2.rank);
        _gameState.AddCCGEventLog(logData);
        _gameState.CardDrawn(card2, false, isNewTurn);
    }

    public Card GetCurrent()
    {
        if (currentSupport >= 0 && currentSupport < cards.Count)
        {
            return cards[currentSupport];
        }

        return null;
    }

    public Card DeployCard(int cardId)
    {
        Card result = null;
        if (currentSupport < cards.Count)
        {
            Card card = cards[currentSupport];
            if (card.instanceId == cardId)
            {
                result = card;
                cards.RemoveAt(currentSupport);
                count--;
                currentSupport = (sbyte) cards.Count;
            }
        }

        return result;
    }
}