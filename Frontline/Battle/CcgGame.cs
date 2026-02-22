using Frontline.Battle.GameEvents;
using Frontline.Data.Entities;
using Frontline.Endpoints.Session.Rulesets;

namespace Frontline.Battle;

public class CcgGame
{
    public readonly Guid Id;

    public readonly int Player1Id;

    public readonly int Player2Id;

    public readonly VersusType VersusType;

    public readonly List<GameEventParams> GameEvents = [];

    public CCG GameState { get; set; }

    public RulesetPathResponse RulesetPath { get; set; }

    public int GameChangeCounter { get; private set; }

    public int CurrentEventCount { get; private set; }

    public event Action<CcgGame>? OnBattleFinished;

    private readonly DateTime _creationTime;

    public CcgGame(int player1Id, int player2Id, string player1Name, string player2Name, VersusType versusType,
        List<ItemEntity>[] deckEntities, List<ItemEntity>[] supportEntities, List<ItemEntity> commanderEntities)
    {
        Id = Guid.NewGuid();
        Player1Id = player1Id;
        Player2Id = player2Id;
        VersusType = versusType;

        GameState = new CCG(this);

        var deckCards = new List<List<Card>>();
        for (int i = 0; i < 2; i++)
        {
            var playerDeckCards = new List<Card>();
            foreach (var deckEntity in deckEntities[i])
            {
                playerDeckCards.Add(new Card(GameState, deckEntity));
            }

            deckCards.Add(playerDeckCards);
        }

        var supportCards = new List<List<Card>>();
        for (int i = 0; i < 2; i++)
        {
            var playerSupportCards = new List<Card>();
            foreach (var supportEntity in supportEntities[i])
            {
                playerSupportCards.Add(new Card(GameState, supportEntity));
            }

            supportCards.Add(playerSupportCards);
        }

        var commanders = new List<Card>();
        foreach (var commanderEntity in commanderEntities)
        {
            var card = new CommanderCard(GameState, commanderEntity);
            card.Setup();
            commanders.Add(card);
        }

        GameState.Create(Id, 1, [player1Id, player2Id], [player1Name, player2Name],
            deckCards, supportCards, commanders, [false, false]);

        RulesetPath = new RulesetPathResponse
        {
            Uri = null, //_urlOptions.Value.RulesetsUrl
            Version = 0
        };

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
        RegionEnum target, sbyte slotIndex, sbyte pushDir, bool remote)
    {
        bool flag = area != TargetableArea.AnyAreas;
        bool flag2 = area == TargetableArea.CurrentRegion;
        bool flag3 = target != RegionEnum.NumRegions && slotIndex != -1;
        bool flag4 = GameState.CanDeploy(playerIndex, cardId, area, target, slotIndex, pushDir, remote);
        Console.WriteLine("GAME DEPLOY - {0} {1} {2} {3} {4} {5} {6}", playerIndex, cardId, targetIndex, targetId, area,
            target, slotIndex);
        if (flag && (!flag2 || flag3) && flag4)
        {
            GameEventRegionTarget gameEventRegionTarget = new GameEventRegionTarget(GameEvent.Deploy, cardId,
                playerIndex, targetId, targetIndex, area, target, slotIndex, pushDir);
            if (remote)
            {
                GameState.GetCCGEventLog().Clear();
                if (GameState.Deploy(playerIndex, cardId, targetIndex, targetId, area, target, slotIndex, pushDir,
                        null))
                {
                    return 1;
                }

                return 0;
            }

            return -1;
        }

        Console.WriteLine(
            "DEPLOY FAILED - Game.Deploy isSpecificArea-{0} isCurrentRegion-{1} isSlotSpecified-{2} canDeploy-{3}",
            flag, flag2, flag3, flag4);
        return 0;
    }

    public int Move(sbyte playerIndex, int cardId, RegionEnum target, sbyte slotIndex, sbyte pushDir, bool remote)
    {
        Console.WriteLine("GAME MOVE - {0} {1} {2} {3}", playerIndex, cardId, target, slotIndex);
        if (target != RegionEnum.NumRegions && slotIndex != -1 &&
            GameState.CanMove(playerIndex, cardId, target, slotIndex, pushDir, remote))
        {
            GameEventRegionTarget gameEventRegionTarget = new GameEventRegionTarget(GameEvent.Move, cardId, playerIndex,
                0, 0, TargetableArea.CurrentRegion, target, slotIndex, pushDir);
            if (remote)
            {
                GameState.GetCCGEventLog().Clear();
                if (GameState.Move(playerIndex, cardId, target, slotIndex, pushDir, null))
                {
                    return 1;
                }

                return 0;
            }

            return -1;
        }

        Console.WriteLine("MOVE FAILED - GameState.CanMove failed");
        return 0;
    }

    public int Attack(sbyte playerIndex, int cardId, sbyte ownerId, int targetId, bool remote)
    {
        Console.WriteLine("GAME ATTACK - {0} {1} {2} {3}", playerIndex, cardId, ownerId, targetId);
        if (GameState.CanAttack(playerIndex, cardId, ownerId, targetId, remote))
        {
            GameEventRegionTarget gameEventRegionTarget = new GameEventRegionTarget(GameEvent.Attack, cardId,
                playerIndex, targetId, ownerId, TargetableArea.AnyAreas, RegionEnum.NumRegions, -1, 0);
            if (remote)
            {
                GameState.GetCCGEventLog().Clear();
                if (GameState.Attack(playerIndex, cardId, ownerId, targetId))
                {
                    return 1;
                }

                return 0;
            }

            return -1;
        }

        Console.WriteLine("ATTACK FAILED - GameState.CanAttack failed");
        return 0;
    }

    public int ActivateTrait(sbyte playerIndex, int cardId, sbyte ownerId, int targetId, TargetableArea area,
        RegionEnum region, bool remote)
    {
        if (GameState.CanActivate(playerIndex, cardId, ownerId, targetId, area, region, remote))
        {
            GameEventRegionTarget gameEventRegionTarget = new GameEventRegionTarget(GameEvent.ActivateTrait, cardId,
                playerIndex, targetId, ownerId, area, region, -1, 0);
            if (remote)
            {
                GameState.GetCCGEventLog().Clear();
                if (GameState.ActivateTrait(playerIndex, cardId, ownerId, targetId, area, region))
                {
                    return 1;
                }

                return 0;
            }

            return -1;
        }

        return 0;
    }

    public int Disembark(sbyte playerIndex, int cardId, bool remote)
    {
        if (GameState.CanDisembark(playerIndex, cardId, remote))
        {
            GameEventCardParams gameEventCardParams = new GameEventCardParams(GameEvent.Disembark, cardId, playerIndex);
            if (remote)
            {
                GameState.GetCCGEventLog().Clear();
                if (GameState.Disembark(playerIndex, cardId, false, null))
                {
                    return 1;
                }

                return 0;
            }

            return -1;
        }

        return 0;
    }

    public int TriggerEndTurnTraits(sbyte playerIndex, bool remote)
    {
        Player player = GameState.GetPlayer(playerIndex);
        if (player != null && player.EndTurnTraitsTriggered)
        {
            return 1;
        }

        if (GameState.CanTriggerEndTurnTraits(playerIndex, remote))
        {
            GameEventParams gameEventParams = new GameEventParams(GameEvent.TriggerEndTurnTraits, playerIndex);
            if (remote)
            {
                GameState.GetCCGEventLog().Clear();
                if (GameState.TriggerEndTurnTraits(playerIndex))
                {
                    return 1;
                }

                return 0;
            }

            return -1;
        }

        return 0;
    }

    public int EndTurn(sbyte playerIndex, bool remote, int[] cardsToDiscard)
    {
        if (cardsToDiscard == null)
        {
            return 0;
        }

        if (GameState.CanEndTurn(playerIndex, remote, cardsToDiscard))
        {
            if (remote)
            {
                GameState.GetCCGEventLog().Clear();
                if (GameState.EndTurn(playerIndex, cardsToDiscard))
                {
                    GameEventEndTurnParams eventParams = new GameEventEndTurnParams(playerIndex, cardsToDiscard);
                    return 1;
                }

                return 0;
            }

            return -1;
        }

        return 0;
    }

    public int Surrender(sbyte playerIndex, bool remote)
    {
        if (GameState.CanSurrender(playerIndex))
        {
            GameEventParams gameEventParams = new GameEventParams(GameEvent.Surrender, playerIndex);
            if (remote)
            {
                GameState.GetCCGEventLog().Clear();
                if (GameState.Surrender(playerIndex))
                {
                    return 1;
                }

                return 0;
            }

            return -1;
        }

        return 0;
    }

    public int DoInitialSwap(sbyte playerIndex, int[] cardIdsToReshuffle, int[] deckSwapIndices, bool remote)
    {
        if (!remote)
        {
            return -1;
        }

        if (GameState.CanDoInitialSwap(playerIndex, cardIdsToReshuffle))
        {
            GameState.GetCCGEventLog().Clear();
            bool flag = GameState.DoInitialSwap(playerIndex, cardIdsToReshuffle, deckSwapIndices, true);

            if (flag)
            {
                return 1;
            }
        }

        return 0;
    }

    public int DoCardDiscard(sbyte playerIndex, int[] cardIds, bool remote)
    {
        if (!remote)
        {
            return -1;
        }

        if (GameState.CanDoDiscard(playerIndex, cardIds))
        {
            bool flag = GameState.DoCardDiscard(playerIndex, cardIds);

            if (flag)
            {
                return 1;
            }
        }

        return 0;
    }

    public int Cheat_GiveCardAndCommandPoints(sbyte playerIndex, int cardId, int rank, int commandPoints, bool remote)
    {
        GameState.GetCCGEventLog().Clear();
        bool flag = GameState.GiveCardAndCmdPts(playerIndex, cardId, rank, commandPoints);

        if (flag)
        {
            return 1;
        }

        return 0;
    }

    public int SendMessage(sbyte playerIndex, sbyte messageId, bool remote)
    {
        if (GameState.CanMessage(playerIndex))
        {
            if (remote)
            {
                GameState.GetCCGEventLog().Clear();
                GameEventMessageParams gameEventMessageParams =
                    new GameEventMessageParams(GameEvent.Message, playerIndex, messageId);

                return 1;
            }

            return -1;
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