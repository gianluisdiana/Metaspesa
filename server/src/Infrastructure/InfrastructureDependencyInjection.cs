using System.Text;
using Metaspesa.Application.Abstractions.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Metaspesa.Infrastructure;

public static class InfrastructureDependencyInjection {
  public static IServiceCollection AddInfrastructure(
    this IServiceCollection services
  ) {
    services.AddOptionsWithValidateOnStart<JwtOptions>()
      .BindConfiguration("Jwt")
      .Validate(o => !string.IsNullOrWhiteSpace(o.SecretKey))
      .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer))
      .Validate(o => !string.IsNullOrWhiteSpace(o.Audience))
      .Validate(o => o.ExpirationMinutes > 0);

    services.AddSingleton<ITokenProvider, JwtTokenProvider>();
    services.AddSingleton<IHasher, Pbkdf2Hasher>();

    services
      .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer();

    services
      .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
      .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptions) =>
        bearerOptions.TokenValidationParameters = new TokenValidationParameters {
          ValidIssuer = jwtOptions.Value.Issuer,
          ValidAudience = jwtOptions.Value.Audience,
          IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtOptions.Value.SecretKey)),
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
        });

    services.AddAuthorization();

    return services;
  }
}
