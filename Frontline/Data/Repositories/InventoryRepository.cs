using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IInventoryRepository
{
    Task<ItemEntity?> GetItemAsync(int userId, int itemId);
    List<ItemEntity> GetItems(int userId, int maxItems = 0);
    Task AddItemAsync(int userId, ItemEntity item);
    Task AddItemsAsync(int userId, List<ItemEntity> items);
    Task UpdateItemAsync(ItemEntity item);
    Task RemoveItemAsync(ItemEntity item);
}

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDb _db;

    public InventoryRepository(AppDb db)
    {
        _db = db;
    }

    public Task<ItemEntity?> GetItemAsync(int userId, int itemId)
    {
        return _db.Items
            .Where(item => item.UserId == userId && item.ItemId == itemId)
            .FirstOrDefaultAsync();
    }

    public List<ItemEntity> GetItems(int userId, int maxItems = 0)
    {
        if (maxItems <= 0)
        {
            return _db.Items
                .Where(item => item.UserId == userId)
                .OrderBy(item => item.ItemId)
                .ToList();
        }
        
        return _db.Items
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.ItemId)
            .Take(maxItems)
            .ToList();
    }

    public async Task AddItemAsync(int userId, ItemEntity item)
    {
        item.UserId = userId;
        item.ItemId = GetNextItemIdForUser(userId);
        
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task AddItemsAsync(int userId, List<ItemEntity> items)
    {
        var nextItemId = GetNextItemIdForUser(userId);
        
        foreach (var item in items)
        {
            item.UserId = userId;
            item.ItemId = nextItemId;
            nextItemId++;
        }
        
        _db.Items.AddRange(items);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(ItemEntity item)
    {
        _db.Items.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(ItemEntity item)
    {
        _db.Items.Remove(item);
        await _db.SaveChangesAsync();
    }
    
    private int GetNextItemIdForUser(int userId)
    {
        var maxItemId = _db.Items
            .Where(item => item.UserId == userId)
            .Max(item => (int?) item.ItemId) ?? 0;
        return maxItemId + 1;
    }
}