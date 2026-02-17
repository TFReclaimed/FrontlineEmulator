using FastEndpoints;

namespace Frontline.Endpoints.Missions.GetSupply;

public class GetSupplyEndpoint : EndpointWithoutRequest<GetSupplyResponse>
{
    public override void Configure()
    {
        Post("/Missions/v1/supply");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new GetSupplyResponse
        {
            LastSupplySync = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")
        };

        await Send.OkAsync(response);
    }
}