using Frontline.Options;
using Frontline.Services;
using Microsoft.Extensions.Options;

namespace Frontline.Battle.Matchmaking;

public interface IMatchmakingService
{
    void Enqueue(int userId, VersusType versusType, int? opponentId = null);
    void Cancel(int userId);
    bool IsUserInQueue(int userId);
    void ProcessQueue();
    void CleanupStaleTickets();
}

public class MatchmakingService : IMatchmakingService
{
    private readonly ILogger<MatchmakingService> _logger;

    private readonly IBattleService _battleService;

    private readonly IUserService _userService;

    private readonly IOptionsMonitor<GameOptions> _gameOptions;

    private readonly Dictionary<int, MatchmakingTicket> _matchmakingQueue = new();

    private readonly Lock _queueLock = new();

    private readonly TimeSpan _ticketTimeout = TimeSpan.FromSeconds(20);

    public MatchmakingService(ILogger<MatchmakingService> logger, IBattleService battleService,
        IUserService userService, IOptionsMonitor<GameOptions> gameOptions)
    {
        _logger = logger;
        _battleService = battleService;
        _userService = userService;
        _gameOptions = gameOptions;
    }

    public void Enqueue(int userId, VersusType versusType, int? opponentId = null)
    {
        var ticket = new MatchmakingTicket(userId, versusType, opponentId);

        lock (_queueLock)
        {
            _matchmakingQueue[userId] = ticket;
            _logger.LogInformation("User {UserId} entered queue for {GameType}.",
                userId, versusType);
        }

        if (versusType == VersusType.PvpCasual && opponentId is > 0)
        {
            TryMatchTargetedCasual(ticket);
        }
    }

    private void TryMatchTargetedCasual(MatchmakingTicket ticket)
    {
        if (!_gameOptions.CurrentValue.EnableMatchmaking)
        {
            return;
        }

        lock (_queueLock)
        {
            if (ticket.OpponentId != null &&
                _matchmakingQueue.TryGetValue(ticket.OpponentId.Value, out var opponentTicket) &&
                opponentTicket.VersusType == VersusType.PvpCasual && opponentTicket.OpponentId == ticket.UserId)
            {
                FinalizeMatch(ticket, opponentTicket).Wait();

                _matchmakingQueue.Remove(ticket.UserId);
                _matchmakingQueue.Remove(opponentTicket.UserId);
            }
        }
    }

    public void Cancel(int userId)
    {
        lock (_queueLock)
        {
            if (_matchmakingQueue.Remove(userId))
            {
                _userService.IncrementChangeCounter(userId);
                _logger.LogInformation("Cancelling matchmaking search for user {UserId}.",
                    userId);
            }
        }
    }

    public bool IsUserInQueue(int userId)
    {
        lock (_queueLock)
        {
            return _matchmakingQueue.ContainsKey(userId);
        }
    }

    public void ProcessQueue()
    {
        if (!_gameOptions.CurrentValue.EnableMatchmaking)
        {
            return;
        }

        lock (_queueLock)
        {
            var rankedTickets = _matchmakingQueue.Values
                .Where(t => t.VersusType == VersusType.PvpRanked)
                .OrderBy(t => t.CreationUtc)
                .ToList();

            while (rankedTickets.Count >= 2)
            {
                var ticket1 = rankedTickets[0];
                var ticket2 = rankedTickets[1];

                FinalizeMatch(ticket1, ticket2).Wait();

                rankedTickets.RemoveRange(0, 2);

                _matchmakingQueue.Remove(ticket1.UserId);
                _matchmakingQueue.Remove(ticket2.UserId);
            }
        }
    }

    public void CleanupStaleTickets()
    {
        lock (_queueLock)
        {
            if (_matchmakingQueue.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var staleTickets = _matchmakingQueue.Values
                .Where(t => now - t.CreationUtc >= _ticketTimeout)
                .Select(t => t.UserId)
                .ToList();

            if (staleTickets.Count == 0)
            {
                return;
            }

            foreach (var ticket in staleTickets)
            {
                _matchmakingQueue.Remove(ticket);
            }

            _logger.LogInformation("Cleaned up {StaleTicketCount} stale matchmaking tickets.",
                staleTickets.Count);
        }
    }

    private async Task FinalizeMatch(MatchmakingTicket ticket1, MatchmakingTicket ticket2)
    {
        try
        {
            if (Random.Shared.Next(2) == 1)
            {
                (ticket1, ticket2) = (ticket2, ticket1);
            }

            await _battleService.CreateBattle(ticket1.UserId, ticket2.UserId, ticket1.VersusType);

            _userService.IncrementChangeCounter(ticket1.UserId);
            _userService.IncrementChangeCounter(ticket2.UserId);

            _logger.LogInformation("Matched users {UserId1} and {UserId2} for {GameType}.",
                ticket1.UserId, ticket2.UserId, ticket1.VersusType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing match between users {UserId1} and {UserId2}.",
                ticket1.UserId, ticket2.UserId);
        }
    }
}