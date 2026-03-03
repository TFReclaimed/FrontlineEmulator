using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IChatMessageRepository : IRepository<ChatMessageEntity>
{
    Task<List<ChatMessageEntity>> GetRecentAsync(string room, int count);
    Task DeleteByRoomAsync(string room);
    Task<int> TrimHistoryAsync(int keep);
}

public class ChatMessageRepository : RepositoryBase<ChatMessageEntity>, IChatMessageRepository
{
    public ChatMessageRepository(AppDb db) : base(db)
    {
    }

    public async Task<List<ChatMessageEntity>> GetRecentAsync(string room, int count)
    {
        return await Db.ChatMessages
            .Include(m => m.Player)
            .Where(m => m.Room == room)
            .OrderByDescending(m => m.SentAt)
            .Take(count)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task DeleteByRoomAsync(string room)
    {
        await Db.ChatMessages
            .Where(m => m.Room == room)
            .ExecuteDeleteAsync();
    }

    public async Task<int> TrimHistoryAsync(int keep)
    {
        var rooms = await Db.ChatMessages
            .Select(m => m.Room)
            .Distinct()
            .ToListAsync();

        var totalDeleted = 0;
        foreach (var room in rooms)
        {
            var toDelete = await Db.ChatMessages
                .Where(m => m.Room == room)
                .OrderByDescending(m => m.SentAt)
                .Skip(keep)
                .ToListAsync();

            if (toDelete.Count > 0)
            {
                totalDeleted += toDelete.Count;
                Db.ChatMessages.RemoveRange(toDelete);
            }
        }

        await Db.SaveChangesAsync();
        return totalDeleted;
    }
}