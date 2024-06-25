using Frontline.Data.Entities;

namespace Frontline.Data.Repositories;

public interface IPlayerRepository
{
    Task<PlayerEntity?> GetPlayerAsync(int id);
    Task<PlayerEntity> GetOrCreatePlayerAsync(int id);
    Task UpdatePlayerAsync(PlayerEntity player);
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

    public async Task<PlayerEntity> GetOrCreatePlayerAsync(int id)
    {
        var player = await GetPlayerAsync(id);
        if (player is not null)
        {
            return player;
        }
        
        player = new PlayerEntity
        {
            Credits = 100,
            Supply = 900,
            Trophies = 25,
            Tokens = 25
        };
        
        _db.Players.Add(player);
        await _db.SaveChangesAsync();

        return player;
    }

    public async Task UpdatePlayerAsync(PlayerEntity player)
    {
        _db.Update(player);
        await _db.SaveChangesAsync();
    }
}