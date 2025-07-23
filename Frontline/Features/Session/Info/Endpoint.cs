using FastEndpoints;
using Frontline.Extensions;
using Frontline.Services;

namespace Frontline.Features.Session.Info;

public class Endpoint : EndpointWithoutRequest<SessionInfoResponse>
{
    private readonly IUserService _userService;

    public Endpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Get("/session/info");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = this.GetUserId();

        var response = new SessionInfoResponse
        {
            CurrentGameInstance = "0",
            UserChangeCounter = _userService.GetChangeCounter(userId)
        };
        
        await Send.OkAsync(response);
    }
}