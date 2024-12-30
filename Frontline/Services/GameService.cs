using System.Diagnostics.CodeAnalysis;
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

    public GameService(ILogger<GameService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public CcgGame CreateGame(int userId, VersusType type)
    {
        _logger.LogInformation("Creating new game for user {UserId} of type {GameType}.", userId, type);
        
        var game = new CcgGame(Guid.NewGuid(), userId, type);
        _games.Add(game.Id, game);
        return game;
    }

    public CcgGame? GetGame(Guid gameId)
    {
        return _games.GetValueOrDefault(gameId);
    }

    public CcgGame? GetEmptyGame(VersusType type)
    {
        return _games.Values.FirstOrDefault(g => !g.IsFull && g.VersusType == type);
    }

    public async Task BeginGame(CcgGame game, int player2Id)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var playerRepository = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();

        var player1Entity = await playerRepository.GetPlayerAsync(game.Player1Id);
        var player2Entity = await playerRepository.GetPlayerAsync(player2Id);
        
        if (player1Entity is null || player2Entity is null)
        {
            throw new Exception("Player 1 or 2 not found.");
        }
        
        _logger.LogInformation("Beginning game {GameId} between players {Player1Id} and {Player2Id}.",
            game.Id, game.Player1Id, player2Id);
        
        game.BeginGame(player1Entity, player2Entity);
    }

    public void DeleteGame(Guid gameId)
    {
        _games.Remove(gameId);
    }

    public bool IsPlayerInGame(int userId, [NotNullWhen(true)] out CcgGame? game)
    {
        game = _games.Values.FirstOrDefault(g => g.Player1Id == userId || g.Player2Id == userId);
        return game != null;
    }
}