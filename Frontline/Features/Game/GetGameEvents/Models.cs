using Frontline.Game;

namespace Frontline.Features.Game.GetGameEvents;

public class GameEventsRequest
{
    public Guid GameId { get; set; }
    public required GameEventsRequestParams Param { get; set; }
}

public class GameEventsRequestParams
{
    public int StartIndex { get; set; }
}

public class GameEventsResponse
{
    public required List<GameEventParams> Events { get; set; }
}