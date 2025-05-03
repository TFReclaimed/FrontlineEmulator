using System.Diagnostics.CodeAnalysis;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;
using Frontline.Game;

namespace Frontline.Services;

public interface IGameService
{
    CcgGame CreateGame(int userId, VersusType type);
    CcgGame? GetGame(Guid gameId);
    CcgGame? GetEmptyGame(VersusType type);
    Task BeginGame(CcgGame game, int player2Id);
    void DeleteGame(Guid gameId);
    bool IsPlayerInGame(int userId, [NotNullWhen(true)] out CcgGame? game);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> _logger;
    
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    private readonly Dictionary<Guid, CcgGame> _games = new();
    
    private readonly Lock _lock = new();

    public GameService(ILogger<GameService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public CcgGame CreateGame(int userId, VersusType type)
    {
        _logger.LogInformation("Creating new game for user {UserId} of type {GameType}.", userId, type);
        
        var game = new CcgGame(Guid.NewGuid(), userId, type);

        lock (_lock)
        {
            _games.Add(game.Id, game);
        }
        
        return game;
    }

    public CcgGame? GetGame(Guid gameId)
    {
        lock (_lock)
        {
            return _games.GetValueOrDefault(gameId);
        }
    }

    public CcgGame? GetEmptyGame(VersusType type)
    {
        lock (_lock)
        {
            return _games.Values.FirstOrDefault(g => !g.IsFull && g.VersusType == type);
        }
    }

    public async Task BeginGame(CcgGame game, int player2Id)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var playerRepository = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var inventoryRepository = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();

        var player1Entity = await playerRepository.GetPlayerAsync(game.Player1Id);
        var player2Entity = await playerRepository.GetPlayerAsync(player2Id);
        
        if (player1Entity is null || player2Entity is null)
        {
            throw new Exception("Player 1 or 2 not found.");
        }
        
        var player1Dropship = await inventoryRepository.GetDropshipItems(player1Entity.Id, player1Entity.DropshipId);
        var player2Dropship = await inventoryRepository.GetDropshipItems(player2Entity.Id, player2Entity.DropshipId);

        GetCardSets(player1Dropship, out var player1Deck,
            out var player1Support, out var player1Commander);
        GetCardSets(player2Dropship, out var player2Deck,
            out var player2Support, out var player2Commander);
        
        if (player1Deck.Count == 0 || player1Support.Count == 0 || player1Commander.ItemId == 0 ||
            player2Deck.Count == 0 || player2Support.Count == 0 || player2Commander.ItemId == 0)
        {
            throw new Exception("Player 1 or 2 has an invalid deck.");
        }
        
        _logger.LogInformation("Beginning game {GameId} between players {Player1Id} and {Player2Id}.",
            game.Id, game.Player1Id, player2Id);
        
        game.BeginGame(player1Entity, player1Deck, player1Support, player1Commander,
            player2Entity, player2Deck, player2Support, player2Commander);
    }

    public void DeleteGame(Guid gameId)
    {
        lock (_lock)
        {
            _games.Remove(gameId);
        }
    }

    public bool IsPlayerInGame(int userId, [NotNullWhen(true)] out CcgGame? game)
    {
        lock (_lock)
        {
            game = _games.Values.FirstOrDefault(g => g.Player1Id == userId || g.Player2Id == userId);
            return game != null;
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
}