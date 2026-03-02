namespace Frontline.Battle.Matchmaking;

public class MatchmakingTicket
{
    public int UserId { get; }
    public VersusType VersusType { get; }
    public int? OpponentId { get; }
    public DateTime CreationUtc { get; }

    public MatchmakingTicket(int userId, VersusType versusType, int? opponentId)
    {
        UserId = userId;
        VersusType = versusType;
        OpponentId = opponentId;
        CreationUtc = DateTime.UtcNow;
    }
}