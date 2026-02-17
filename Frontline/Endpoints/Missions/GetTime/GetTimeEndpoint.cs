using FastEndpoints;

namespace Frontline.Endpoints.Missions.GetTime;

public class GetTimeEndpoint : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Get("/Missions/v1/servertime");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        await Send.StringAsync(time);
    }
}