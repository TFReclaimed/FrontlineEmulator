using System.Diagnostics.CodeAnalysis;

namespace Frontline.Battle;

public interface IBattleService
{
    CcgGame? GetBattle(Guid gameId);
    void CreateBattle(int player1Id, int player2Id, VersusType versusType);
    bool IsPlayerInGame(int userId, [NotNullWhen(true)] out CcgGame? game);
}

public class BattleService : IBattleService
{
    private readonly ILogger<BattleService> _logger;

    private readonly Dictionary<Guid, CcgGame> _battles = new();

    private readonly Lock _lock = new();

    public BattleService(ILogger<BattleService> logger)
    {
        _logger = logger;
    }

    public CcgGame? GetBattle(Guid gameId)
    {
        lock (_lock)
        {
            return _battles.GetValueOrDefault(gameId);
        }
    }

    public void CreateBattle(int player1Id, int player2Id, VersusType versusType)
    {
        var battle = new CcgGame(player1Id, player2Id, versusType);

        lock (_lock)
        {
            _battles.Add(battle.Id, battle);
        }
    }

    public bool IsPlayerInGame(int userId, [NotNullWhen(true)] out CcgGame? game)
    {
        lock (_lock)
        {
            game = _battles.Values.FirstOrDefault(b => b.Player1Id == userId || b.Player2Id == userId);
            return game is not null;
        }
    }
}