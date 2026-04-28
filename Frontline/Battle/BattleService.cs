using System.Diagnostics.CodeAnalysis;
using Frontline.Battle.CcgEvents;
using Frontline.Battle.GameEvents;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Services;
using Frontline.Xmpp;

namespace Frontline.Battle;

public interface IBattleService
{
    int GetBattleCount();
    CcgGame? GetBattle(Guid gameId);
    Task CreateBattle(int player1Id, int player2Id, VersusType versusType);
    bool IsPlayerInGame(int userId, [NotNullWhen(true)] out CcgGame? game);
    void CleanupStaleBattles();
}

public class BattleService : IBattleService
{
    private readonly ILogger<BattleService> _logger;

    private readonly ILoggerFactory _loggerFactory;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly IWebHostEnvironment _environment;
    
    private readonly IXmppServer _xmppServer;

    private readonly Dictionary<Guid, CcgGame> _battles = new();
    
    private readonly Dictionary<Guid, DateTime> _toRemove = new();

    private readonly Lock _lock = new();

    public BattleService(ILogger<BattleService> logger, ILoggerFactory loggerFactory,
        IServiceScopeFactory serviceScopeFactory, IWebHostEnvironment environment,
        IXmppServer xmppServer)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _environment = environment;
        _xmppServer = xmppServer;
    }

    public int GetBattleCount()
    {
        lock (_lock)
        {
            return _battles.Values.Count(b => !b.GameState.IsGameOver());
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
        var player2Entity = await playerRepository.GetByIdAsync(player2Id);

        if (player1Entity == null || player2Entity == null)
        {
            throw new Exception("Player 1 or 2 not found.");
        }

        var player1Dropship = await dropshipRepository.GetDropshipItems(player1Entity.Id, player1Entity.DropshipId);
        var player2Dropship = await dropshipRepository.GetDropshipItems(player2Entity.Id, player2Entity.DropshipId);

        GetCardSets(player1Dropship, out var player1Deck,
            out var player1Support, out var player1Commander);
        GetCardSets(player2Dropship, out var player2Deck,
            out var player2Support, out var player2Commander);

        if (player1Deck.Count == 0 || player1Support.Count == 0 || player1Commander.ItemId == 0 ||
            player2Deck.Count == 0 || player2Support.Count == 0 || player2Commander.ItemId == 0)
        {
            throw new Exception("Player 1 or 2 has an invalid deck.");
        }

        var battle = new CcgGame(player1Id, player2Id, player1Entity.Name, player2Entity.Name, versusType,
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

    public void CleanupStaleBattles()
    {
        lock (_lock)
        {
            if (_battles.Count == 0)
            {
                return;
            }
            
            var now = DateTime.Now;
            var emptyBattles = _toRemove
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            if (emptyBattles.Count > 0)
            {
                foreach (var key in emptyBattles)
                {
                    _battles.Remove(key);
                    _toRemove.Remove(key);
                }
                
                _logger.LogInformation("Cleaned up {Count} empty battles.", emptyBattles.Count);
            }
            
            var staleBattles = _battles.Values
                .Where(b => b.IsStale() && !_toRemove.ContainsKey(b.Id))
                .ToList();

            if (staleBattles.Count == 0)
            {
                return;
            }

            var toRemoveInitialSize = _toRemove.Count;
            
            foreach (var battle in staleBattles)
            {
                battle.LogGameState();
                _toRemove.Add(battle.Id, DateTime.Now.AddSeconds(30));
                
                var player1Connected = _xmppServer.IsClientConnected(battle.Player1Id);
                var player2Connected = _xmppServer.IsClientConnected(battle.Player2Id);

                if (player1Connected != player2Connected && !battle.GameState.IsGameOver())
                {
                    battle.PlayGameEvent(new GameEventParams
                    {
                        PlayerIndex = (sbyte) (player1Connected ? 1 : 0),
                        GameEvent = GameEvent.Surrender
                    });
                }
            }
            
            _logger.LogInformation("Marked {Count} stale battles for deletion.", _toRemove.Count - toRemoveInitialSize);
        }
    }

    private void GetCardSets(List<DropshipEntity> dropship, out List<ItemEntity> deck, out List<ItemEntity> support,
        out ItemEntity commander)
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

        var player1Entity = await playerRepository.GetByIdAsync(battle.Player1Id);
        var player2Entity = await playerRepository.GetByIdAsync(battle.Player2Id);

        if (player1Entity == null || player2Entity == null)
        {
            _logger.LogError("Player 1 or 2 not found when processing battle rewards.");
            return;
        }

        await ProcessUserRewards(player1Entity, battle.GameState.Rewards[0], playerRepository, userService);
        await ProcessUserRewards(player2Entity, battle.GameState.Rewards[1], playerRepository, userService);

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

    private async Task ProcessUserRewards(PlayerEntity player, Rewards rewards, IPlayerRepository playerRepository,
        IUserService userService)
    {
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