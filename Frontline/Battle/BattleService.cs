using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Frontline.Battle.CcgEvents;
using Frontline.Battle.GameEvents;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Services;

namespace Frontline.Battle;

public interface IBattleService
{
    int GetPvpBattleCount();
    int GetAiBattleCount();
    CcgGame? GetBattle(Guid gameId);
    Task CreateBattle(int player1Id, int player2Id, VersusType versusType);
    bool IsPlayerInGame(int userId, [NotNullWhen(true)] out CcgGame? game);
    void ProcessAiTurns();
    void CleanupStaleBattles();
}

public class BattleService : IBattleService
{
    private readonly ILogger<BattleService> _logger;

    private readonly ILoggerFactory _loggerFactory;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly IWebHostEnvironment _environment;

    private readonly Dictionary<Guid, CcgGame> _battles = new();
    
    private readonly Dictionary<Guid, DateTime> _toRemove = new();

    private readonly Lock _lock = new();

    public BattleService(ILogger<BattleService> logger, ILoggerFactory loggerFactory,
        IServiceScopeFactory serviceScopeFactory, IWebHostEnvironment environment)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _environment = environment;
    }

    public int GetPvpBattleCount()
    {
        lock (_lock)
        {
            return _battles.Values.Count(b =>
                b.GameState.GameType != VersusType.PvpAiRemote && !b.GameState.IsGameOver());
        }
    }

    public int GetAiBattleCount()
    {
        lock (_lock)
        {
            return _battles.Values.Count(b =>
                b.GameState.GameType == VersusType.PvpAiRemote && !b.GameState.IsGameOver());
        }
    }

    public CcgGame? GetBattle(Guid gameId)
    {
        lock (_lock)
        {
            return _battles.GetValueOrDefault(gameId);
        }
    }

    public async Task CreateBattle(int player1Id, int player2Id, VersusType versusType)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var playerRepository = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var dropshipRepository = scope.ServiceProvider.GetRequiredService<IDropshipRepository>();

        var player1Entity = await playerRepository.GetByIdAsync(player1Id);
        PlayerEntity? player2Entity = null;

        if (player2Id != -1)
        {
            player2Entity = await playerRepository.GetByIdAsync(player2Id);
        }

        if (player1Entity == null || (player2Id != -1 && player2Entity == null))
        {
            throw new Exception("Player 1 or 2 not found.");
        }

        var player1Name = player1Entity.Name;
        var player2Name = player2Id == -1 ? "<color=red>CLANKER</color>" : player2Entity!.Name;

        var player1Dropship = await dropshipRepository.GetDropshipItems(player1Entity.Id, player1Entity.DropshipId);
        var player2Dropship = player2Id == -1
            ? player1Dropship
            : await dropshipRepository.GetDropshipItems(player2Entity!.Id, player2Entity.DropshipId);

        GetCardSets(player1Dropship, out var player1Deck,
            out var player1Support, out var player1Commander);
        GetCardSets(player2Dropship, out var player2Deck,
            out var player2Support, out var player2Commander);

        if (player1Deck.Count == 0 || player1Support.Count == 0 || player1Commander.ItemId == 0 ||
            player2Deck.Count == 0 || player2Support.Count == 0 || player2Commander.ItemId == 0)
        {
            throw new Exception("Player 1 or 2 has an invalid deck.");
        }

        var battle = new CcgGame(player1Id, player2Id, player1Name, player2Name, versusType,
            [player1Deck, player2Deck], [player1Support, player2Support],
            [player1Commander, player2Commander], _environment.IsProduction(), _loggerFactory);
        battle.OnBattleFinished += OnBattleFinished;

        lock (_lock)
        {
            _battles.Add(battle.Id, battle);
        }
    }

    public bool IsPlayerInGame(int userId, [NotNullWhen(true)] out CcgGame? game)
    {
        lock (_lock)
        {
            CcgGame? potentialGame = null;
            foreach (var battle in _battles.Values)
            {
                if (battle.Player1Id != userId && battle.Player2Id != userId)
                {
                    continue;
                }

                potentialGame = battle;

                if (!battle.GameState.IsGameOver())
                {
                    break;
                }
            }

            game = potentialGame;
            return game is not null;
        }
    }

    public void ProcessAiTurns()
    {
        lock (_lock)
        {
            if (_battles.Count == 0)
            {
                return;
            }

            foreach (var battle in _battles.Values)
            {
                if (battle.GameState.GameType != VersusType.PvpAiRemote)
                {
                    continue;
                }

                if (battle.GameState.IsGameOver())
                {
                    continue;
                }

                if (battle.GameState.PlayerTurn != 1)
                {
                    continue;
                }

                var player = battle.GameState.GetPlayer(1)!;

                try
                {
                    var action = battle.GenerateNextAiAction();
                    if (action == null)
                    {
                        _logger.LogWarning("AI failed to generate action for game {GameId}.",
                            battle.Id);

                        EndBattleTurn(battle, 1, player);
                        continue;
                    }

                    battle.GameState.Logger.Debug(JsonSerializer.Serialize(action));

                    if (action.GameEvent != GameEvent.TriggerEndTurnTraits)
                    {
                        battle.PlayGameEvent(action);
                        continue;
                    }

                    EndBattleTurn(battle, 1, player);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing AI turn for game {GameId}.", battle.Id);

                    EndBattleTurn(battle, 1, player);
                }
            }
        }
    }

    public void CleanupStaleBattles()
    {
        lock (_lock)
        {
            if (_battles.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var markedCount = 0;
            foreach (var battle in _battles.Values)
            {
                if (!battle.GameState.IsGameOver() || _toRemove.ContainsKey(battle.Id))
                {
                    continue;
                }

                _toRemove.Add(battle.Id, now.AddSeconds(30));
                markedCount++;
            }

            if (markedCount > 0)
            {
                _logger.LogInformation("Marked {Count} finished battles for deletion.",
                    markedCount);
            }

            var oldBattles = _toRemove
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            if (oldBattles.Count > 0)
            {
                foreach (var key in oldBattles)
                {
                    _battles[key].LogGameState();
                    _battles.Remove(key);
                    _toRemove.Remove(key);
                }

                _logger.LogInformation("Cleaned up {Count} old battles.",
                    oldBattles.Count);
            }

            var staleTurnBattles = _battles.Values
                .Where(b => b.IsStaleTurn() && !_toRemove.ContainsKey(b.Id))
                .ToList();

            if (staleTurnBattles.Count == 0)
            {
                return;
            }

            foreach (var battle in staleTurnBattles)
            {
                var inactivePlayerIndex = battle.GetInactivePlayerIndex();
                if (inactivePlayerIndex == -1)
                {
                    continue;
                }

                var inactivePlayer = battle.GameState.GetPlayer(inactivePlayerIndex)!;

                if (battle.GameState.PlayerTurnStart == 0)
                {
                    battle.PlayGameEvent(new GameEventMulliganParams
                    {
                        PlayerIndex = inactivePlayerIndex,
                        GameEvent = GameEvent.DoInitialSwap
                    });

                    _logger.LogInformation("Auto-mulliganed player {Player} in game {GameId} due to inactivity.",
                        inactivePlayer.UserId, battle.Id);
                }
                else
                {
                    EndBattleTurn(battle, inactivePlayerIndex, inactivePlayer);

                    _logger.LogInformation("Auto-ended turn for player {Player} in game {GameId} due to inactivity.",
                        inactivePlayer.UserId, battle.Id);
                }
            }
        }
    }

    private static void EndBattleTurn(CcgGame battle, sbyte playerIndex, Player player)
    {
        battle.PlayGameEvent(new GameEventParams
        {
            PlayerIndex = playerIndex,
            GameEvent = GameEvent.TriggerEndTurnTraits
        });

        var maxCardsInHand = battle.GameState.GetGameTemplate().MaxCardsInHand;
        if (player.Hand.Cards.Count > maxCardsInHand)
        {
            var discardCount = player.Hand.Cards.Count - maxCardsInHand;
            var discardCardIds = player
                .GetAutoDiscardCards(discardCount)
                .Select(c => c.InstanceId)
                .ToArray();

            battle.PlayGameEvent(new GameEventDiscardParams
            {
                PlayerIndex = playerIndex,
                HandCardIdsToDiscard = discardCardIds,
                GameEvent = GameEvent.DiscardCard
            });
        }

        battle.PlayGameEvent(new GameEventEndTurnParams
        {
            PlayerIndex = playerIndex,
            GameEvent = GameEvent.EndTurn
        });
    }

    private static void GetCardSets(List<DropshipEntity> dropship, out List<ItemEntity> deck,
        out List<ItemEntity> support, out ItemEntity commander)
    {
        deck = [];
        support = [];
        commander = new ItemEntity();

        foreach (var item in dropship)
        {
            switch (item.SlotIndex)
            {
                case < 30:
                    deck.Add(item.Item!);
                    break;

                case 30:
                    commander = item.Item!;
                    break;

                case > 30:
                    support.Add(item.Item!);
                    break;
            }
        }
    }

    private void OnBattleFinished(CcgGame battle)
    {
        Task.Run(() => OnBattleFinishedAsync(battle));
    }

    private async Task OnBattleFinishedAsync(CcgGame battle)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var playerRepository = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var inventoryRepository = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        await ProcessUserRewards(battle.Player1Id, battle.GameState.Rewards[0], playerRepository, userService);
        await ProcessUserRewards(battle.Player2Id, battle.GameState.Rewards[1], playerRepository, userService);

        foreach (var ccgEvent in battle.GameState.GetCcgEventLog())
        {
            if (ccgEvent is not CardInfoCcgEvent cardInfoEvent)
            {
                continue;
            }

            if (cardInfoEvent.EventType != CcgEventType.CardXpEarned)
            {
                continue;
            }

            var playerId = cardInfoEvent.Owner == 0 ? battle.Player1Id : battle.Player2Id;
            if (playerId == -1)
            {
                continue;
            }

            var itemEntity = await inventoryRepository.GetItemAsync(playerId, cardInfoEvent.InstanceId);
            if (itemEntity == null)
            {
                continue;
            }

            itemEntity.Xp += cardInfoEvent.Data;
            await inventoryRepository.UpdateAsync(itemEntity);
        }

        _logger.LogInformation("Processed battle rewards for game {GameId}.", battle.Id);
    }

    private async Task ProcessUserRewards(int playerId, Rewards rewards, IPlayerRepository playerRepository,
        IUserService userService)
    {
        if (playerId == -1)
        {
            return;
        }

        var player = await playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            _logger.LogError("Player {PlayerId} not found when processing battle rewards.",
                playerId);
            return;
        }

        player.Xp += rewards.PlayerXp;
        player.Trophies += rewards.Trophies;
        player.Credits += rewards.Credits;
        player.Supply += rewards.Supply;
        player.BoosterPackCount += rewards.Boosters;
        player.Tokens += rewards.Tokens;
        player.MatchesPlayed++;
        player.HighestTrophies = Math.Max(player.HighestTrophies, player.Trophies);

        if (rewards.IsWinner)
        {
            player.Wins++;
        }

        await playerRepository.UpdateAsync(player);
        userService.IncrementChangeCounter(player.Id);
    }
}