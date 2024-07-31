using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IMissionRepository
{
    Task<ActiveMissionEntity?> GetActiveMissionAsync(int userId, string key);
    Task<FinishedMissionEntity?> GetFinishedMissionAsync(int userId, string key);
    Task<List<ActiveMissionEntity>> GetActiveMissionsAsync(int userId);
    Task<List<FinishedMissionEntity>> GetFinishedMissionsAsync(int userId);
    Task AddActiveMissionAsync(ActiveMissionEntity mission);
    Task AddFinishedMissionAsync(FinishedMissionEntity mission);
    Task DeleteActiveMissionAsync(ActiveMissionEntity mission);
    Task<bool> HasCompletedMissionsAsync(int userId, List<string> keys);
    Task<bool> IsCardOnMissionAsync(int userId, List<int> itemIds);
}

public class MissionRepository : IMissionRepository
{
    private readonly AppDb _db;

    public MissionRepository(AppDb db)
    {
        _db = db;
    }

    public async Task<ActiveMissionEntity?> GetActiveMissionAsync(int userId, string key)
    {
        return await _db.ActiveMissions
            .Include(mission => mission.RequiredCardItem)
            .Include(mission => mission.BonusCard1Item)
            .Include(mission => mission.BonusCard2Item)
            .Where(mission => mission.UserId == userId && mission.MissionKey == key)
            .FirstOrDefaultAsync();
    }

    public async Task<FinishedMissionEntity?> GetFinishedMissionAsync(int userId, string key)
    {
        return await _db.FinishedMissions
            .Where(mission => mission.UserId == userId && mission.MissionKey == key)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ActiveMissionEntity>> GetActiveMissionsAsync(int userId)
    {
        return await _db.ActiveMissions
            .Include(mission => mission.RequiredCardItem)
            .Include(mission => mission.BonusCard1Item)
            .Include(mission => mission.BonusCard2Item)
            .Where(mission => mission.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<FinishedMissionEntity>> GetFinishedMissionsAsync(int userId)
    {
        return await _db.FinishedMissions
            .Where(mission => mission.UserId == userId)
            .ToListAsync();
    }

    public async Task AddActiveMissionAsync(ActiveMissionEntity mission)
    {
        _db.ActiveMissions.Add(mission);
        await _db.SaveChangesAsync();
    }

    public async Task AddFinishedMissionAsync(FinishedMissionEntity mission)
    {
        _db.FinishedMissions.Add(mission);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteActiveMissionAsync(ActiveMissionEntity mission)
    {
        _db.ActiveMissions.Remove(mission);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> HasCompletedMissionsAsync(int userId, List<string> keys)
    {
        return await _db.FinishedMissions
            .Where(mission => mission.UserId == userId && keys.Contains(mission.MissionKey))
            .CountAsync() == keys.Count;
    }

    public async Task<bool> IsCardOnMissionAsync(int userId, List<int> itemIds)
    {
        return await _db.ActiveMissions
            .Where(mission => mission.UserId == userId)
            .AnyAsync(mission =>
                itemIds.Contains(mission.RequiredCardItemId)
                || (mission.BonusCard1ItemId != null && itemIds.Contains(mission.BonusCard1ItemId.Value))
                || (mission.BonusCard2ItemId != null && itemIds.Contains(mission.BonusCard2ItemId.Value)));
    }
}