using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public abstract class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly AppDb Db;

    protected RepositoryBase(AppDb db)
    {
        Db = db;
    }

    public async Task<T?> GetByIdAsync(params object[] keyValues)
    {
        return await Db.FindAsync<T>(keyValues);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Db.Set<T>().ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await Db.AddAsync(entity);
        await Db.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await Db.AddRangeAsync(entities);
        await Db.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        Db.Update(entity);
        await Db.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        Db.UpdateRange(entities);
        await Db.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        Db.Remove(entity);
        await Db.SaveChangesAsync();
    }
}