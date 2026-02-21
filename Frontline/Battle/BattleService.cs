using System.Diagnostics.CodeAnalysis;
using Frontline.Data.Entities;
using Frontline.Data.Repositories;

namespace Frontline.Battle;

public interface IBattleService
{
    CcgGame? GetBattle(Guid gameId);
    Task CreateBattle(int player1Id, int player2Id, VersusType versusType);
    bool IsPlayerInGame(int userId, [NotNullWhen(true)] out CcgGame? game);
}

public class BattleService : IBattleService
{
    private readonly ILogger<BattleService> _logger;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly Dictionary<Guid, CcgGame> _battles = new();

    private readonly Lock _lock = new();

    public BattleService(ILogger<BattleService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
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

        var battle = new CcgGame(player1Id, player2Id, versusType, [player1Deck, player2Deck],
            [player1Support, player2Support], [player1Commander, player2Commander]);

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