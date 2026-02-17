using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IInventoryRepository
{
    Task<ItemEntity?> GetItemAsync(int userId, int itemId);
    Task<List<ItemEntity>> GetItems(int userId, int maxItems = 0);
    Task<List<ItemEntity>> GetItems(int userId, List<int> itemIds);
    Task<int> GetItemCountAsync(int userId);
    Task<List<DropshipEntity>> GetDropshipItems(int userId);
    Task AddItemAsync(int userId, ItemEntity item);
    Task AddItemsAsync(int userId, List<ItemEntity> items);
    Task AddDropshipItemsAsync(int userId, List<DropshipEntity> dropshipItems);
    Task UpdateItemAsync(ItemEntity item);
    Task RemoveItemAsync(ItemEntity item);
    Task ClearDropshipItemsAsync(int userId, int dropshipId);
    Task<bool> HasItemsAsync(int userId, List<int> itemIds);
}

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDb _db;

    public InventoryRepository(AppDb db)
    {
        _db = db;
    }

    public async Task<ItemEntity?> GetItemAsync(int userId, int itemId)
    {
        return await _db.Items
            .Where(item => item.UserId == userId && item.ItemId == itemId)
            .Select(item => new ItemEntity
            {
                UserId = item.UserId,
                ItemId = item.ItemId,
                TemplateId = item.TemplateId,
                Xp = item.Xp,
                Rank = item.Rank,
                Casualty = item.Casualty,
                DropshipId = _db.Dropships
                    .Where(dropshipItem => dropshipItem.UserId == userId
                                         && dropshipItem.ItemId == item.ItemId)
                    .Select(dropshipItem => (int?) dropshipItem.DropshipId)
                    .FirstOrDefault() ?? -1,
                CurrentMission = _db.ActiveMissions
                    .Where(mission => mission.UserId == userId
                                      && (mission.RequiredCardItemId == item.ItemId
                                          || mission.BonusCard1ItemId == item.ItemId
                                          || mission.BonusCard2ItemId == item.ItemId))
                    .Select(mission => mission.MissionKey)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<ItemEntity>> GetItems(int userId, int maxItems = 0)
    {
        if (maxItems <= 0)
        {
            return await _db.Items
                .Where(item => item.UserId == userId)
                .OrderBy(item => item.ItemId)
                .Select(item => new ItemEntity
                {
                    UserId = item.UserId,
                    ItemId = item.ItemId,
                    TemplateId = item.TemplateId,
                    Xp = item.Xp,
                    Rank = item.Rank,
                    Casualty = item.Casualty,
                    DropshipId = _db.Dropships
                        .Where(dropshipItem => dropshipItem.UserId == userId
                                             && dropshipItem.ItemId == item.ItemId)
                        .Select(dropshipItem => (int?) dropshipItem.DropshipId)
                        .FirstOrDefault() ?? -1,
                    CurrentMission = _db.ActiveMissions
                        .Where(mission => mission.UserId == userId
                                          && (mission.RequiredCardItemId == item.ItemId
                                              || mission.BonusCard1ItemId == item.ItemId
                                              || mission.BonusCard2ItemId == item.ItemId))
                        .Select(mission => mission.MissionKey)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }
        
        return await _db.Items
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.ItemId)
            .Take(maxItems)
            .Select(item => new ItemEntity
            {
                UserId = item.UserId,
                ItemId = item.ItemId,
                TemplateId = item.TemplateId,
                Xp = item.Xp,
                Rank = item.Rank,
                Casualty = item.Casualty,
                DropshipId = _db.Dropships
                    .Where(dropshipItem => dropshipItem.UserId == userId
                                         && dropshipItem.ItemId == item.ItemId)
                    .Select(dropshipItem => (int?) dropshipItem.DropshipId)
                    .FirstOrDefault() ?? -1,
                CurrentMission = _db.ActiveMissions
                    .Where(mission => mission.UserId == userId
                                      && (mission.RequiredCardItemId == item.ItemId
                                          || mission.BonusCard1ItemId == item.ItemId
                                          || mission.BonusCard2ItemId == item.ItemId))
                    .Select(mission => mission.MissionKey)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<List<ItemEntity>> GetItems(int userId, List<int> itemIds)
    {
        return await _db.Items
            .Where(item => item.UserId == userId && itemIds.Contains(item.ItemId))
            .ToListAsync();
    }

    public async Task<int> GetItemCountAsync(int userId)
    {
        return await _db.Items
            .Where(item => item.UserId == userId)
            .CountAsync();
    }

    public async Task<List<DropshipEntity>> GetDropshipItems(int userId)
    {
        return await _db.Dropships
            .Include(dropship => dropship.Item)
            .Where(dropship => dropship.UserId == userId)
            .ToListAsync();
    }

    public async Task AddItemAsync(int userId, ItemEntity item)
    {
        item.UserId = userId;
        item.ItemId = await GetNextItemIdForUser(userId);
        
        await _db.Items.AddAsync(item);
        await _db.SaveChangesAsync();
    }

    public async Task AddItemsAsync(int userId, List<ItemEntity> items)
    {
        var nextItemId = await GetNextItemIdForUser(userId);
        
        foreach (var item in items)
        {
            item.UserId = userId;
            item.ItemId = nextItemId;
            nextItemId++;
        }
        
        await _db.Items.AddRangeAsync(items);
        await _db.SaveChangesAsync();
    }

    public async Task AddDropshipItemsAsync(int userId, List<DropshipEntity> dropshipItems)
    {
        await _db.Dropships.AddRangeAsync(dropshipItems);
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

    public async Task ClearDropshipItemsAsync(int userId, int dropshipId)
    {
        await _db.Dropships
            .Where(dropshipItem => dropshipItem.UserId == userId && dropshipItem.DropshipId == dropshipId)
            .ExecuteDeleteAsync();
    }

    public async Task<bool> HasItemsAsync(int userId, List<int> itemIds)
    {
        return await _db.Items
            .Where(item => item.UserId == userId && itemIds.Contains(item.ItemId))
            .CountAsync() == itemIds.Count;
    }

    private async Task<int> GetNextItemIdForUser(int userId)
    {
        var maxItemId = await _db.Items
            .Where(item => item.UserId == userId)
            .MaxAsync(item => (int?) item.ItemId) ?? 0;
        return maxItemId + 1;
    }
}