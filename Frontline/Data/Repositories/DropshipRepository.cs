using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IDropshipRepository : IRepository<DropshipEntity>
{
    Task<List<DropshipEntity>> GetDropshipItems(int userId);
    Task ClearDropshipItemsAsync(int userId, int dropshipId);
}

public class DropshipRepository : RepositoryBase<DropshipEntity>, IDropshipRepository
{
    public DropshipRepository(AppDb db) : base(db)
    {
    }

    public async Task<List<DropshipEntity>> GetDropshipItems(int userId)
    {
        return await Db.Dropships
            .Include(dropship => dropship.Item)
            .Where(dropship => dropship.UserId == userId)
            .ToListAsync();
    }

    public async Task ClearDropshipItemsAsync(int userId, int dropshipId)
    {
        await Db.Dropships
            .Where(dropshipItem => dropshipItem.UserId == userId && dropshipItem.DropshipId == dropshipId)
            .ExecuteDeleteAsync();
    }
}