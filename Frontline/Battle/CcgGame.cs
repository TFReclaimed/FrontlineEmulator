using Frontline.Battle.GameEvents;
using Frontline.Data.Entities;
using Frontline.Endpoints.Session.Rulesets;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class CcgGame
{
    public readonly Guid Id;

    public readonly int Player1Id;

    public readonly int Player2Id;

    public readonly List<GameEventParams> GameEvents = [];

    public CCG GameState { get; set; }

    public RulesetPathResponse RulesetPath { get; set; }

    public int GameChangeCounter { get; private set; }

    public int CurrentEventCount { get; private set; }

    public event Action<CcgGame>? OnBattleFinished;

    private readonly bool _isProduction;

    private readonly DateTime _creationTime;

    public CcgGame(int player1Id, int player2Id, string player1Name, string player2Name, VersusType versusType,
        List<ItemEntity>[] deckEntities, List<ItemEntity>[] supportEntities, List<ItemEntity> commanderEntities,
        bool production)
    {
        Id = Guid.NewGuid();
        Player1Id = player1Id;
        Player2Id = player2Id;

        GameState = new CCG(this, Id, 1, versusType);

        var deckCards = new List<List<Card>>();
        for (var i = 0; i < 2; i++)
        {
            var playerDeckCards = new List<Card>();
            foreach (var deckEntity in deckEntities[i])
            {
                var template = RulesetParser.GetCardTemplate(deckEntity.TemplateId)!;

                if (template is UnitCardTemplate)
                {
                    playerDeckCards.Add(new UnitCard(GameState, deckEntity));
                }
                else
                {
                    playerDeckCards.Add(new Card(GameState, deckEntity));
                }
            }

            deckCards.Add(playerDeckCards);
        }

        var supportCards = new List<List<Card>>();
        for (var i = 0; i < 2; i++)
        {
            var playerSupportCards = new List<Card>();
            foreach (var supportEntity in supportEntities[i])
            {
                var template = RulesetParser.GetCardTemplate(supportEntity.TemplateId)!;

                if (template is UnitCardTemplate)
                {
                    playerSupportCards.Add(new UnitCard(GameState, supportEntity));
                }
                else
                {
                    playerSupportCards.Add(new Card(GameState, supportEntity));
                }
            }

            supportCards.Add(playerSupportCards);
        }

        var commanders = new List<CommanderCard>();
        foreach (var commanderEntity in commanderEntities)
        {
            var card = new CommanderCard(GameState, commanderEntity);
            card.Setup();
            commanders.Add(card);
        }

        GameState.CreatePlayers([player1Id, player2Id], [player1Name, player2Name],
            deckCards, supportCards, commanders);

        RulesetPath = new RulesetPathResponse
        {
            Uri = null,
            Version = 0
        };

        _isProduction = production;
        _creationTime = DateTime.UtcNow;
    }

    public bool IsPlayerInGame(int userId)
    {
        return Player1Id == userId || Player2Id == userId;
    }

    public bool IsStale()
    {
        var now = DateTime.UtcNow;

        if (GameState.PlayerTurnStart == 0)
        {
            return (now - _creationTime).TotalMinutes > 2;
        }

        var playerTurnStart = DateTimeOffset.FromUnixTimeMilliseconds(GameState.PlayerTurnStart);
        return (now - playerTurnStart).TotalMinutes > 4;
    }

    public void PlayGameEvent(GameEventParams gameEventParams)
    {
        var result = gameEventParams.ReplayEvent(this);
        gameEventParams.EventResult = result;
        GameEvents.Add(gameEventParams);
        GameChangeCounter++;
        CurrentEventCount++;
    }

    public void EndGame()
    {
        if (!GameState.IsGameOver())
        {
            return;
        }

        OnBattleFinished?.Invoke(this);
    }

    public int Deploy(sbyte playerIndex, int cardId, sbyte targetIndex, int targetId, TargetableArea area,
        Region target, sbyte slotIndex, sbyte pushDir)
    {
        var flag = area != TargetableArea.AnyAreas;
        var flag2 = area == TargetableArea.CurrentRegion;
        var flag3 = target != Region.NumRegions && slotIndex != -1;
        var flag4 = GameState.CanDeploy(playerIndex, cardId, area, target, slotIndex, pushDir);
        Console.WriteLine("GAME DEPLOY - {0} {1} {2} {3} {4} {5} {6}", playerIndex, cardId, targetIndex, targetId, area,
            target, slotIndex);
        if (flag && (!flag2 || flag3) && flag4)
        {
            GameState.GetCCGEventLog().Clear();
            if (GameState.Deploy(playerIndex, cardId, targetIndex, targetId, area, target, slotIndex, pushDir,
                    null))
            {
                return 1;
            }

            return 0;
        }

        Console.WriteLine(
            "DEPLOY FAILED - Game.Deploy isSpecificArea-{0} isCurrentRegion-{1} isSlotSpecified-{2} canDeploy-{3}",
            flag, flag2, flag3, flag4);
        return 0;
    }

    public int Move(sbyte playerIndex, int cardId, Region target, sbyte slotIndex, sbyte pushDir)
    {
        Console.WriteLine("GAME MOVE - {0} {1} {2} {3}", playerIndex, cardId, target, slotIndex);
        if (target != Region.NumRegions && slotIndex != -1 &&
            GameState.CanMove(playerIndex, cardId, target, slotIndex, pushDir))
        {
            GameState.GetCCGEventLog().Clear();
            if (GameState.Move(playerIndex, cardId, target, slotIndex, pushDir, null))
            {
                return 1;
            }

            return 0;
        }

        Console.WriteLine("MOVE FAILED - GameState.CanMove failed");
        return 0;
    }

    public int Attack(sbyte playerIndex, int cardId, sbyte ownerId, int targetId)
    {
        Console.WriteLine("GAME ATTACK - {0} {1} {2} {3}", playerIndex, cardId, ownerId, targetId);
        if (GameState.CanAttack(playerIndex, cardId, ownerId, targetId))
        {
            GameState.GetCCGEventLog().Clear();
            if (GameState.Attack(playerIndex, cardId, ownerId, targetId))
            {
                return 1;
            }

            return 0;
        }

        Console.WriteLine("ATTACK FAILED - GameState.CanAttack failed");
        return 0;
    }

    public int ActivateTrait(sbyte playerIndex, int cardId, sbyte ownerId, int targetId, TargetableArea area,
        Region region)
    {
        if (GameState.CanActivate(playerIndex, cardId, ownerId, targetId, area, region))
        {
            GameState.GetCCGEventLog().Clear();
            if (GameState.ActivateTrait(playerIndex, cardId, ownerId, targetId, area, region))
            {
                return 1;
            }
        }

        return 0;
    }

    public int Disembark(sbyte playerIndex, int cardId)
    {
        if (GameState.CanDisembark(playerIndex, cardId))
        {
            GameState.GetCCGEventLog().Clear();
            if (GameState.Disembark(playerIndex, cardId, false, null))
            {
                return 1;
            }
        }

        return 0;
    }

    public int TriggerEndTurnTraits(sbyte playerIndex)
    {
        var player = GameState.GetPlayer(playerIndex);
        if (player != null && player.EndTurnTraitsTriggered)
        {
            return 1;
        }

        if (GameState.CanTriggerEndTurnTraits(playerIndex))
        {
            GameState.GetCCGEventLog().Clear();
            if (GameState.TriggerEndTurnTraits(playerIndex))
            {
                return 1;
            }
        }

        return 0;
    }

    public int EndTurn(sbyte playerIndex, int[] cardsToDiscard)
    {
        if (cardsToDiscard == null)
        {
            return 0;
        }

        if (GameState.CanEndTurn(playerIndex, cardsToDiscard))
        {
            GameState.GetCCGEventLog().Clear();
            if (GameState.EndTurn(playerIndex, cardsToDiscard))
            {
                return 1;
            }
        }

        return 0;
    }

    public int Surrender(sbyte playerIndex)
    {
        if (GameState.CanSurrender(playerIndex))
        {
            GameState.GetCCGEventLog().Clear();
            if (GameState.Surrender(playerIndex))
            {
                return 1;
            }
        }

        return 0;
    }

    public int DoInitialSwap(sbyte playerIndex, int[] cardIdsToReshuffle, int[] deckSwapIndices)
    {
        if (GameState.CanDoInitialSwap(playerIndex, cardIdsToReshuffle))
        {
            GameState.GetCCGEventLog().Clear();

            if (GameState.DoInitialSwap(playerIndex, cardIdsToReshuffle, deckSwapIndices))
            {
                return 1;
            }
        }

        return 0;
    }

    public int DoCardDiscard(sbyte playerIndex, int[] cardIds)
    {
        if (GameState.CanDoDiscard(playerIndex, cardIds))
        {
            if (GameState.DoCardDiscard(playerIndex, cardIds))
            {
                return 1;
            }
        }

        return 0;
    }

    public int Cheat_GiveCardAndCommandPoints(sbyte playerIndex, int cardId, int rank, int commandPoints)
    {
        GameState.GetCCGEventLog().Clear();

        if (_isProduction)
        {
            return 0;
        }

        if (GameState.GiveCardAndCmdPts(playerIndex, cardId, rank, commandPoints))
        {
            return 1;
        }

        return 0;
    }

    public int SendMessage(sbyte playerIndex)
    {
        if (GameState.CanMessage(playerIndex))
        {
            GameState.GetCCGEventLog().Clear();
            return 1;
        }

        return 0;
    }

    public int GetServerIntValue(int min, int max)
    {
        /*if (cachedIntValues != null && cachedIntValues.Count > 0)
        {
            int result = cachedIntValues[0];
            cachedIntValues.RemoveAt(0);
            return result;
        }*/

        if (min == max)
        {
            return min;
        }

        return Random.Shared.Next(min, max);
    }
}