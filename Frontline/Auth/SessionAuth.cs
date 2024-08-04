using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using Frontline.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace Frontline.Auth;

public class SessionAuth : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IOptions<JwtOptions> _jwtOptions;
    
    public const string SchemeName = "Session";
    
    public const string SessionIdHeaderName = "Sessionid";

    public SessionAuth(IOptions<JwtOptions> jwtOptions, IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
        _jwtOptions = jwtOptions;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (IsPublicEndpoint())
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        
        var headerPresent = Request.Headers.TryGetValue(SessionIdHeaderName, out var token);
        if (!headerPresent || !IsValidToken(token, out var jwt))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }

        var identity = new ClaimsIdentity(jwt.Claims, SchemeName);
        var principal = new GenericPrincipal(identity, null);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool IsPublicEndpoint()
    {
        return Context
            .GetEndpoint()?
            .Metadata.OfType<AllowAnonymousAttribute>()
            .Any() is null or true;
    }

    private bool IsValidToken(StringValues token, [NotNullWhen(true)] out JwtSecurityToken? jwt)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.Value.Key))
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            jwt = (JwtSecurityToken) validatedToken;
            return true;
        }
        catch
        {
            jwt = null;
            return false;
        }
    }
}