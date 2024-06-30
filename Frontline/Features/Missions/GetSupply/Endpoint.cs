using FastEndpoints;

namespace Frontline.Features.Missions.GetSupply;

public class Endpoint : EndpointWithoutRequest<GetSupplyResponse>
{
    public override void Configure()
    {
        Post("/Missions/v1/supply");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new GetSupplyResponse
        {
            LastSupplySync = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        };

        await SendAsync(response, cancellation: ct);
    }
}