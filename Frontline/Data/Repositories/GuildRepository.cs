using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IGuildRepository
{
    Task<GuildEntity?> GetGuildAsync(Guid id, bool includeMembers = false);
    Task<List<GuildEntity>> GetGuilds(int page, int maxCount, string search);
    Task<GuildEntity?> GetPlayerGuildAsync(int playerId, bool includeMembers = false);
    Task<GuildMemberEntity?> GetPlayerMembershipAsync(int playerId);
    Task UpdatePlayerMembershipAsync(GuildMemberEntity member);
    Task DeletePlayerMembershipAsync(GuildMemberEntity member);
    Task CreateGuildAsync(GuildEntity guild, GuildMemberEntity member);
    Task UpdateGuildAsync(GuildEntity guild);
    Task JoinGuildAsync(int playerId, Guid guildId);
    Task DeleteGuildAsync(GuildEntity guild);
}

public class GuildRepository : IGuildRepository
{
    private readonly AppDb _db;

    public GuildRepository(AppDb db)
    {
        _db = db;
    }

    public async Task<GuildEntity?> GetGuildAsync(Guid id, bool includeMembers = false)
    {
        if (includeMembers)
        {
            return await _db.Guilds
                .Include(g => g.Members)
                .ThenInclude(m => m.Player)
                .FirstOrDefaultAsync(g => g.Id == id);
        }
        
        return await _db.Guilds.FindAsync(id);
    }

    public async Task<List<GuildEntity>> GetGuilds(int page, int maxCount, string search)
    {
        return await _db.Guilds
            .Where(g => g.Name.Contains(search))
            .OrderBy(g => g.Name)
            .Skip(page * maxCount)
            .Take(maxCount)
            .ToListAsync();
    }

    public async Task<GuildEntity?> GetPlayerGuildAsync(int playerId, bool includeMembers = false)
    {
        if (includeMembers)
        {
            return await _db.GuildMembers
                .Where(m => m.Player!.Id == playerId)
                .Include(m => m.Guild)
                .ThenInclude(g => g!.Members)
                .ThenInclude(m => m.Player)
                .Select(m => m.Guild)
                .FirstOrDefaultAsync();
        }
        
        return await _db.GuildMembers
            .Where(m => m.Player!.Id == playerId)
            .Select(m => m.Guild)
            .FirstOrDefaultAsync();
    }

    public async Task<GuildMemberEntity?> GetPlayerMembershipAsync(int playerId)
    {
        return await _db.GuildMembers
            .FirstOrDefaultAsync(m => m.Player!.Id == playerId);
    }

    public async Task UpdatePlayerMembershipAsync(GuildMemberEntity member)
    {
        _db.GuildMembers.Update(member);
        await _db.SaveChangesAsync();
    }

    public async Task DeletePlayerMembershipAsync(GuildMemberEntity member)
    {
        _db.GuildMembers.Remove(member);
        await _db.SaveChangesAsync();
    }

    public async Task CreateGuildAsync(GuildEntity guild, GuildMemberEntity member)
    {
        await _db.Guilds.AddAsync(guild);
        await _db.SaveChangesAsync();
        
        member.GuildId = guild.Id;
        
        await _db.GuildMembers.AddAsync(member);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateGuildAsync(GuildEntity guild)
    {
        _db.Guilds.Update(guild);
        await _db.SaveChangesAsync();
    }

    public async Task JoinGuildAsync(int playerId, Guid guildId)
    {
        var member = new GuildMemberEntity
        {
            UserId = playerId,
            GuildId = guildId,
            Rank = MemberRank.MEMBER
        };
        
        await _db.GuildMembers.AddAsync(member);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteGuildAsync(GuildEntity guild)
    {
        _db.Guilds.Remove(guild);
        await _db.SaveChangesAsync();
    }
}