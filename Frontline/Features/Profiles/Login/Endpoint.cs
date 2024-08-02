using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Repositories;
using Frontline.Options;
using Microsoft.Extensions.Options;

namespace Frontline.Features.Profiles.Login;

public class Endpoint : Endpoint<LoginRequest, PlayerProfile, Mapper>
{
    private readonly IPlayerRepository _playerRepository;
    
    private readonly IOptions<JwtOptions> _jwtOptions;

    public Endpoint(IPlayerRepository playerRepository, IOptions<JwtOptions> jwtOptions)
    {
        _playerRepository = playerRepository;
        _jwtOptions = jwtOptions;
    }

    public override void Configure()
    {
        Post("/virtu/accts");
        AllowFormData(urlEncoded: true);
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        Logger.LogInformation("Login request received. Type: {LoginType}, AuthId: {AuthId}, Password {Password}, DeviceId: {DeviceId}, DeviceType: {DeviceType}",
            req.Param.LoginType, req.Param.AuthId, req.Param.Password, req.Param.DeviceId, req.Param.DeviceType);

        // Works fine until I reverse engineer Nexon's API
        const long startingId = 12530000000025341;

        var authId = long.Parse(req.Param.AuthId.Trim('"'));
        var userId = (int) (authId - startingId);
        
        var player = await _playerRepository.GetOrCreatePlayerAsync(userId);

        var jwtToken = JwtBearer.CreateToken(o =>
        {
            o.SigningKey = _jwtOptions.Value.Key;
            o.ExpireAt = DateTime.UtcNow.AddDays(7);
            o.User["UserId"] = player.Id.ToString();
        });

        var profile = Map.FromEntity(player);
        profile.SessionId = jwtToken;
        
        await SendAsync(profile);
    }
}