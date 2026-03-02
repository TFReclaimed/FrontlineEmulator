using Frontline.Battle.GameEvents;

namespace Frontline.Endpoints.Game.GetGameEvents;

public class GetGameEventsRequest
{
    public Guid GameId { get; set; }
    public required GameEventsRequestParams Param { get; set; }
}

public class GameEventsRequestParams
{
    public int StartIndex { get; set; }
}

public class GetGameEventsResponse
{
    public required List<GameEventParams> Events { get; set; }
}