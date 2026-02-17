using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IPlayerRepository : IRepository<PlayerEntity>
{
    Task<(PlayerEntity entity, bool created)> GetOrCreatePlayerAsync(int id);
    Task<List<PlayerEntity>> GetTopPlayersAsync(int count);
}

public class PlayerRepository : RepositoryBase<PlayerEntity>, IPlayerRepository
{
    public PlayerRepository(AppDb db) : base(db)
    {
    }

    public async Task<(PlayerEntity entity, bool created)> GetOrCreatePlayerAsync(int id)
    {
        var player = await GetByIdAsync(id);
        if (player is not null)
        {
            return (player, false);
        }

        player = new PlayerEntity
        {
            Id = id,
            Credits = 100,
            Supply = 900,
            Trophies = 25,
            Tokens = 25,
            HighestTrophies = 25,
            Xp = 325
        };

        await AddAsync(player);

        return (player, true);
    }

    public async Task<List<PlayerEntity>> GetTopPlayersAsync(int count)
    {
        var players = await Db.Players
            .Where(p => p.Name != string.Empty)
            .OrderByDescending(p => p.Trophies)
            .Take(count)
            .Select(p => new 
            {
                Player = p,
                GuildName = Db.GuildMembers
                    .Where(gm => gm.UserId == p.Id)
                    .Select(gm => gm.Guild!.Name)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return players.Select(p => 
        {
            p.Player.GuildName = p.GuildName ?? string.Empty;
            return p.Player;
        }).ToList();
    }
}