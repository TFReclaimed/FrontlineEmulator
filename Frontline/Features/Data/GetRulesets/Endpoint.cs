using FastEndpoints;

namespace Frontline.Features.Data.GetRulesets;

public class Endpoint : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Get("/Data/Rulesets");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var rulesetPath = Path.Combine(AppContext.BaseDirectory, "ruleset.json");
        string json;
        
        if (File.Exists(rulesetPath))
        {
            json = await File.ReadAllTextAsync(rulesetPath, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendStringAsync(json, contentType: "application/json", cancellation: ct);
    }
}