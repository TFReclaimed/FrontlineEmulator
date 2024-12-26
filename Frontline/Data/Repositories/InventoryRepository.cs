using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IInventoryRepository
{
    Task<ItemEntity?> GetItemAsync(int userId, int itemId);
    List<ItemEntity> GetItems(int userId, int maxItems = 0);
    List<ItemEntity> GetItems(int userId, List<int> itemIds);
    List<DropshipEntity> GetDropshipItems(int userId);
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

    public Task<ItemEntity?> GetItemAsync(int userId, int itemId)
    {
        return _db.Items
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
                    .Select(dropshipItem => dropshipItem.DropshipId)
                    .FirstOrDefault(),
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

    public List<ItemEntity> GetItems(int userId, int maxItems = 0)
    {
        if (maxItems <= 0)
        {
            return _db.Items
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
                        .Select(dropshipItem => dropshipItem.DropshipId)
                        .FirstOrDefault(),
                    CurrentMission = _db.ActiveMissions
                        .Where(mission => mission.UserId == userId
                                          && (mission.RequiredCardItemId == item.ItemId
                                              || mission.BonusCard1ItemId == item.ItemId
                                              || mission.BonusCard2ItemId == item.ItemId))
                        .Select(mission => mission.MissionKey)
                        .FirstOrDefault()
                })
                .ToList();
        }
        
        return _db.Items
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
                    .Select(dropshipItem => dropshipItem.DropshipId)
                    .FirstOrDefault(),
                CurrentMission = _db.ActiveMissions
                    .Where(mission => mission.UserId == userId
                                      && (mission.RequiredCardItemId == item.ItemId
                                          || mission.BonusCard1ItemId == item.ItemId
                                          || mission.BonusCard2ItemId == item.ItemId))
                    .Select(mission => mission.MissionKey)
                    .FirstOrDefault()
            })
            .ToList();
    }

    public List<ItemEntity> GetItems(int userId, List<int> itemIds)
    {
        return _db.Items
            .Where(item => item.UserId == userId && itemIds.Contains(item.ItemId))
            .ToList();
    }

    public List<DropshipEntity> GetDropshipItems(int userId)
    {
        return _db.Dropships
            .Include(dropship => dropship.Item)
            .Where(dropship => dropship.UserId == userId)
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

    public async Task AddDropshipItemsAsync(int userId, List<DropshipEntity> dropshipItems)
    {
        foreach (var dropshipItem in dropshipItems)
        {
            _db.Dropships.Add(dropshipItem);
        }
        
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

    private int GetNextItemIdForUser(int userId)
    {
        var maxItemId = _db.Items
            .Where(item => item.UserId == userId)
            .Max(item => (int?) item.ItemId) ?? 0;
        return maxItemId + 1;
    }
}