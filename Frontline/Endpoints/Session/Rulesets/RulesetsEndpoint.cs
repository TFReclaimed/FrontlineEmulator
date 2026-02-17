using FastEndpoints;
using Frontline.Options;
using Microsoft.Extensions.Options;

namespace Frontline.Endpoints.Session.Rulesets;

public class RulesetsEndpoint : EndpointWithoutRequest<RulesetPathResponse>
{
    private readonly IOptions<UrlOptions> _urlOptions;

    public RulesetsEndpoint(IOptions<UrlOptions> urlOptions)
    {
        _urlOptions = urlOptions;
    }

    public override void Configure()
    {
        Get("/session/rulesets/0");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new RulesetPathResponse
        {
            Uri = _urlOptions.Value.RulesetsUrl,
            Version = 0
        };
        
        await Send.OkAsync(response);
    }
}