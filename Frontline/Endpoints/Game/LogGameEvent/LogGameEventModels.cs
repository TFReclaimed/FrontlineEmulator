using Frontline.Battle.GameEvents;

namespace Frontline.Endpoints.Game.LogGameEvent;

public class LogGameEventRequest
{
    public Guid GameId { get; set; }
    public required GameEventParams Param { get; set; }
}