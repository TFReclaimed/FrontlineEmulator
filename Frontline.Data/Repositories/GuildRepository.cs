using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IGuildRepository : IRepository<GuildEntity>
{
    Task<GuildEntity?> GetWithMembersAsync(Guid guildId);
    Task<GuildEntity?> GetPlayerGuildAsync(int playerId);
    Task<List<GuildEntity>> SearchGuildsAsync(int page, int maxCount, string search);
}

public class GuildRepository : RepositoryBase<GuildEntity>, IGuildRepository
{
    public GuildRepository(AppDb db) : base(db)
    {
    }

    public async Task<GuildEntity?> GetWithMembersAsync(Guid guildId)
    {
        return await Db.Guilds
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == guildId);
    }

    public async Task<GuildEntity?> GetPlayerGuildAsync(int playerId)
    {
        return await Db.GuildMembers
            .Where(m => m.UserId == playerId)
            .Include(m => m.Guild)
            .ThenInclude(g => g!.Members)
            .Select(m => m.Guild)
            .FirstOrDefaultAsync();
    }

    public async Task<List<GuildEntity>> SearchGuildsAsync(int page, int maxCount, string search)
    {
        return await Db.Guilds
            .Where(g => g.Name.Contains(search))
            .Include(g => g.Members)
            .ThenInclude(m => m.Player)
            .OrderBy(g => g.Name)
            .Skip(page * maxCount)
            .Take(maxCount)
            .ToListAsync();
    }
}