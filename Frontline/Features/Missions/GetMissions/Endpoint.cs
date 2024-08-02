using FastEndpoints;

namespace Frontline.Features.Missions.GetMissions;

public class Endpoint : EndpointWithoutRequest<GetMissionsResponse>
{
    public override void Configure()
    {
        Get("/Missions/v1/missions");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var pvePath = Path.Combine(AppContext.BaseDirectory, "PvEData.json");
        var json = await File.ReadAllTextAsync(pvePath, ct);
        
        var response = new GetMissionsResponse
        {
            Version = "1.0",
            Data = json
        };

        await SendAsync(response);
    }
}