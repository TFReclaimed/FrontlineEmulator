using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpLogging;

namespace Frontline.Features.Session.Ping;

public class Endpoint : EndpointWithoutRequest<Ok>
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
        await SendOkAsync();
    }
}