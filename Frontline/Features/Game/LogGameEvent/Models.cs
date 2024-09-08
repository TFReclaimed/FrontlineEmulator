using Frontline.Game;

namespace Frontline.Features.Game.LogGameEvent;

public class GameEventRequest
{
    public Guid GameId { get; set; }
    public required GameEventParams Param { get; set; }
}

public class GameEventResponse
{
    public int SequenceNum { get; set; }
    public GameEventResult? EventResult { get; set; }
}