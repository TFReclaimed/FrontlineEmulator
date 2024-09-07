using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IPlayerRepository
{
    Task<PlayerEntity?> GetPlayerAsync(int id);
    Task<(PlayerEntity entity, bool created)> GetOrCreatePlayerAsync(int id);
    Task UpdatePlayerAsync(PlayerEntity player);
    Task<List<PlayerEntity>> GetTopPlayersAsync(int count);
}

public class PlayerRepository : IPlayerRepository
{
    private readonly AppDb _db;
    
    public PlayerRepository(AppDb db)
    {
        _db = db;
    }
    
    public async Task<PlayerEntity?> GetPlayerAsync(int id)
    {
        return await _db.Players.FindAsync(id);
    }

    public async Task<(PlayerEntity entity, bool created)> GetOrCreatePlayerAsync(int id)
    {
        var player = await GetPlayerAsync(id);
        if (player is not null)
        {
            return (player, false);
        }
        
        player = new PlayerEntity
        {
            Credits = 100,
            Supply = 900,
            Trophies = 25,
            Tokens = 25,
            Xp = 325
        };
        
        _db.Players.Add(player);
        await _db.SaveChangesAsync();

        return (player, true);
    }

    public async Task UpdatePlayerAsync(PlayerEntity player)
    {
        _db.Update(player);
        await _db.SaveChangesAsync();
    }

    public async Task<List<PlayerEntity>> GetTopPlayersAsync(int count)
    {
        var players = await _db.Players
            .Where(p => p.Name != string.Empty)
            .OrderByDescending(p => p.Trophies)
            .Take(count)
            .Select(p => new 
            {
                Player = p,
                GuildName = _db.GuildMembers
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