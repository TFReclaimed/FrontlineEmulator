using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IFinishedMissionRepository : IRepository<FinishedMissionEntity>
{
    Task<List<FinishedMissionEntity>> GetFinishedMissionsAsync(int userId);
    Task<bool> HasCompletedMissionsAsync(int userId, List<string> keys);
}

public class FinishedMissionRepository : RepositoryBase<FinishedMissionEntity>, IFinishedMissionRepository
{
    public FinishedMissionRepository(AppDb db) : base(db)
    {
    }

    public async Task<List<FinishedMissionEntity>> GetFinishedMissionsAsync(int userId)
    {
        return await Db.FinishedMissions
            .Where(mission => mission.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> HasCompletedMissionsAsync(int userId, List<string> keys)
    {
        return await Db.FinishedMissions
            .Where(mission => mission.UserId == userId && keys.Contains(mission.MissionKey))
            .CountAsync() == keys.Count;
    }
}