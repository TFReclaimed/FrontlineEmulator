using FastEndpoints;
using Frontline.Game;

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
        if (RulesetParser.RulesetJson is null)
        {
            await Send.NotFoundAsync();
            return;
        }

        await Send.StringAsync(RulesetParser.RulesetJson, contentType: "application/json");
    }
}