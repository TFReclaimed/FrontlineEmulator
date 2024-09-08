using System.Text.Json.Serialization;
using FastEndpoints;
using Frontline.Game;

namespace Frontline.Features.Session.Game.RequestGame;

public class RequestGameRequest
{
    [QueryParam]
    public required GameRequestParams Param { get; set; }
}

public class RequestGameResponse
{
    [ToHeader("set-cookie")]
    public string MatchmakingCookie { get; set; } = string.Empty;
}

public class GameRequestParams
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VersusType GameType { get; set; }
    public int OpponentId { get; set; }
}