using FastEndpoints;

namespace Frontline.Features.Session.Game.CancelRequest;

public class CancelGameRequest
{
    [FromHeader(IsRequired = false)]
    public string Cookie { get; set; } = string.Empty;
}