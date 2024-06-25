using FastEndpoints;

namespace Frontline.Features.Guilds.GetGiftStatus;

public class Endpoint : EndpointWithoutRequest<GuildGiftStatusResponse>
{
    public override void Configure()
    {
        Get("/Dealership/v1/guild");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new GuildGiftStatusResponse
        {
            Time = DateTime.Now.Subtract(TimeSpan.FromHours(5))
        };
        
        await SendAsync(response, cancellation: ct);
    }
}