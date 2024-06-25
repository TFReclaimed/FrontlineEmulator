using FastEndpoints;
using Microsoft.AspNetCore.HttpLogging;

namespace Frontline.Features.Session.Polling;

public class Endpoint : EndpointWithoutRequest<PollingResponse>
{
    public override void Configure()
    {
        Get("/session/polling");
        Options(b =>
        {
            b.WithHttpLogging(HttpLoggingFields.None);
        });
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new PollingResponse
        {
            ChangeCounter = 0
        };
        
        await SendAsync(response, cancellation: ct);
    }
}