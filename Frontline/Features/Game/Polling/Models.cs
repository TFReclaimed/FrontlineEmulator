namespace Frontline.Features.Game.Polling;

public class PollingRequest
{
    public Guid GameId { get; set; }
}

public class PollingResponse
{
    public int ChangeCounter { get; set; }
}