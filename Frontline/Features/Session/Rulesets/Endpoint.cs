using FastEndpoints;

namespace Frontline.Features.Session.Rulesets;

public class Endpoint : EndpointWithoutRequest<RulesetPathResponse>
{
    public override void Configure()
    {
        Get("/session/rulesets/0");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new RulesetPathResponse
        {
            Uri = "http://192.168.0.219/Data/Rulesets",
            Version = 0,
        };
        
        await SendAsync(response);
    }
}