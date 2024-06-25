namespace Frontline.Features.Session.Info;

public class SessionInfoResponse
{
    public string CurrentGameInstance { get; set; }
    public int CurrentDeckId { get; set; }
    public int UserChangeCounter { get; set; }
    public long LastSeen { get; set; }
    public int Credits { get; set; }
    public int Supply { get; set; }
    public int Tokens { get; set; }
    public int Trophies { get; set; }
    public int Currency { get; set; }
}