using FastEndpoints;
using Frontline.Extensions;
using Frontline.Services;
using Microsoft.AspNetCore.HttpLogging;

namespace Frontline.Features.Session.Polling;

public class Endpoint : EndpointWithoutRequest<PollingResponse>
{
    private readonly IUserService _userService;

    public Endpoint(IUserService userService)
    {
        _userService = userService;
    }

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
        var userId = this.GetUserId();

        var response = new PollingResponse
        {
            ChangeCounter = _userService.GetChangeCounter(userId)
        };
        
        await SendAsync(response);
    }
}