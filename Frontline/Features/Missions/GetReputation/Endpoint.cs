using FastEndpoints;

namespace Frontline.Features.Missions.GetReputation;

public class Endpoint : EndpointWithoutRequest<List<ReputationInfo>>
{
    public override void Configure()
    {
        Get("/Missions/v1/reputation");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new List<ReputationInfo>
        {
            
        };

        await SendAsync(response);
    }
}