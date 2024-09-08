using System.Diagnostics.CodeAnalysis;
using Frontline.Game;

namespace Frontline.Services;

public interface IGameService
{
    CcgGame CreateGame(int userId, VersusType type);
    CcgGame? GetGame(Guid gameId);
    CcgGame? GetEmptyGame(VersusType type);
    void DeleteGame(Guid gameId);
    bool IsPlayerInGame(int userId, [NotNullWhen(true)] out CcgGame? game);
}

public class GameService : IGameService
{
    private readonly Dictionary<Guid, CcgGame> _games = new();

    public CcgGame CreateGame(int userId, VersusType type)
    {
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