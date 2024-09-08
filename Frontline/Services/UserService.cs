namespace Frontline.Services;

public interface IUserService
{
    int GetChangeCounter(int userId);
    void IncrementChangeCounter(int userId);
}

public class UserService : IUserService
{
    private readonly Dictionary<int, int> _changeCounters = new();
    
    public int GetChangeCounter(int userId)
    {
        return _changeCounters.GetValueOrDefault(userId);
    }

    public void IncrementChangeCounter(int userId)
    {
        _changeCounters.TryAdd(userId, 0);
        _changeCounters[userId]++;
    }
}