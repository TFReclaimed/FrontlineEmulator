using FastEndpoints;
using Frontline.Options;
using Microsoft.Extensions.Options;

namespace Frontline.Features.Session.Rulesets;

public class Endpoint : EndpointWithoutRequest<RulesetPathResponse>
{
    private readonly IOptions<UrlOptions> _urlOptions;

    public Endpoint(IOptions<UrlOptions> urlOptions)
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
        
        await SendAsync(response);
    }
}