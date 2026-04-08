using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Frontline.Auth;

public class SessionAuth : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ITokenValidator _tokenValidator;
    
    public const string SchemeName = "Session";
    
    public const string SessionIdHeaderName = "Sessionid";

    public SessionAuth(ITokenValidator tokenValidator, IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
        _tokenValidator = tokenValidator;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (IsPublicEndpoint())
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        
        var headerPresent = Request.Headers.TryGetValue(SessionIdHeaderName, out var token);
        if (!headerPresent || !_tokenValidator.IsValidToken(token.ToString(), out var jwt))
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
}