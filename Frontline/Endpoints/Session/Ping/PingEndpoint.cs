using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpLogging;

namespace Frontline.Endpoints.Session.Ping;

public class PingEndpoint : EndpointWithoutRequest<Ok>
{
    public override void Configure()
    {
        Get("/session/ping");
        AllowAnonymous();
        Options(b =>
        {
            b.WithHttpLogging(HttpLoggingFields.None);
        });
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync();
    }
}