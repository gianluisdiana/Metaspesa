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
  public string GenerateToken(User user) {
    Debug.Assert(user is not null);

    JwtOptions jwtOptions = options.Value;
    SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));
    SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

    Claim[] claims = [
      new(ClaimTypes.Name, user.Username),
      new(ClaimTypes.Role, user.Role.ToString()),
    ];

    JwtSecurityToken token = new(
      issuer: jwtOptions.Issuer,
      audience: jwtOptions.Audience,
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes),
      signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}
