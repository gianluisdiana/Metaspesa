using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Metaspesa.Infrastructure;

internal class JwtTokenProvider(IOptions<JwtOptions> options) : ITokenProvider {
  public Token GenerateToken(User user) {
    Debug.Assert(user is not null);
    Debug.Assert(options.Value.Key is not null);

    JwtOptions jwtOptions = options.Value;
    SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtOptions.Key));
    SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
    DateTime expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes);

    Claim[] claims = [
      new(ClaimTypes.NameIdentifier, user.Uid.ToString()),
      new(ClaimTypes.Name, user.Username),
      new(ClaimTypes.Role, user.Role.ToString()),
    ];

    JwtSecurityToken token = new(
      issuer: jwtOptions.Issuer,
      audience: jwtOptions.Audience,
      claims: claims,
      expires: expiresAt,
      signingCredentials: credentials
    );

    return new Token(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
  }
}
