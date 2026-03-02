using FastEndpoints;
using Frontline.Battle;

namespace Frontline.Endpoints.Session.Game.RequestGame;

public class RequestGameRequest
{
    [QueryParam]
    public required GameRequestParams Param { get; set; }
}

public class GameRequestParams
{
    public VersusType GameType { get; set; }
    public int OpponentId { get; set; }
}