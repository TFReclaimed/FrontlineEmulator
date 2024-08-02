using FastEndpoints;

namespace Frontline.Features.Missions.GetTime;

public class Endpoint : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Get("/Missions/v1/servertime");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        await SendStringAsync(time);
    }
}