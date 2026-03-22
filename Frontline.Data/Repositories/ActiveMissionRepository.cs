using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IActiveMissionRepository : IRepository<ActiveMissionEntity>
{
    Task<ActiveMissionEntity?> GetActiveMissionAsync(int userId, string key);
    Task<List<ActiveMissionEntity>> GetActiveMissionsByUserIdAsync(int userId);
    Task<bool> IsCardOnMissionAsync(int userId, List<int> itemIds);
}

public class ActiveMissionRepository : RepositoryBase<ActiveMissionEntity>, IActiveMissionRepository
{
    public ActiveMissionRepository(AppDb db) : base(db)
    {
    }

    public async Task<ActiveMissionEntity?> GetActiveMissionAsync(int userId, string key)
    {
        return await Db.ActiveMissions
            .Include(mission => mission.RequiredCardItem)
            .Include(mission => mission.BonusCard1Item)
            .Include(mission => mission.BonusCard2Item)
            .Where(mission => mission.UserId == userId && mission.MissionKey == key)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ActiveMissionEntity>> GetActiveMissionsByUserIdAsync(int userId)
    {
        return await Db.ActiveMissions
            .Include(mission => mission.RequiredCardItem)
            .Include(mission => mission.BonusCard1Item)
            .Include(mission => mission.BonusCard2Item)
            .Where(mission => mission.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> IsCardOnMissionAsync(int userId, List<int> itemIds)
    {
        return await Db.ActiveMissions
            .Where(mission => mission.UserId == userId)
            .AnyAsync(mission =>
                itemIds.Contains(mission.RequiredCardItemId)
                || (mission.BonusCard1ItemId != null && itemIds.Contains(mission.BonusCard1ItemId.Value))
                || (mission.BonusCard2ItemId != null && itemIds.Contains(mission.BonusCard2ItemId.Value)));
    }
}