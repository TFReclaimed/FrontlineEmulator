using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Frontline.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Frontline.Auth;

public interface ITokenValidator
{
    bool IsValidToken(string token, [NotNullWhen(true)] out JwtSecurityToken? jwt);
}

public class TokenValidator : ITokenValidator
{
    private readonly IOptions<JwtOptions> _jwtOptions;

    public TokenValidator(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }

    public bool IsValidToken(string token, [NotNullWhen(true)] out JwtSecurityToken? jwt)
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