using FastEndpoints;

namespace Frontline.Features.Session.Info;

public class Endpoint : EndpointWithoutRequest<SessionInfoResponse>
{
    public override void Configure()
    {
        Get("/session/info");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new SessionInfoResponse
        {
            CurrentGameInstance = "0"
        };
        
        await SendAsync(response, cancellation: ct);
    }
}