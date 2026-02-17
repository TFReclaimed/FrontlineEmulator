using FastEndpoints;
using Frontline.Extensions;
using Frontline.Services;

namespace Frontline.Endpoints.Session.Info;

public class InfoEndpoint : EndpointWithoutRequest<SessionInfoResponse>
{
    private readonly IUserService _userService;

    public InfoEndpoint(IUserService userService)
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