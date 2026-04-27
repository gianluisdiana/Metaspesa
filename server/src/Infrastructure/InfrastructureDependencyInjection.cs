using System.Security.Cryptography;
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
    services.AddOpenTelemetry()
      .WithTracing(tracing => tracing.AddSource(SecretsLoaderWorker.ActivitySourceName));

    services.AddSingleton<ISecretVault, LocalSecretVault>();
    services.AddHostedService<SecretsLoaderWorker>();

    services.AddSingleton<ITokenProvider, JwtTokenProvider>();
    services.AddSingleton<IHasher, Pbkdf2Hasher>();

    services.AddJwt();

    return services;
  }

  private static IServiceCollection AddJwt(
    this IServiceCollection services
  ) {
    services.AddOptionsWithValidateOnStart<JwtOptions>()
      .BindConfiguration("Jwt")
      .Validate(o => !string.IsNullOrWhiteSpace(o.KeySecretName), "Jwt:KeySecretName must be provided")
      .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer must be provided")
      .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience must be provided")
      .Validate(o => o.ExpirationMinutes > 0, "Jwt:ExpirationMinutes must be greater than 0");

    services
      .AddAuthorization()
      .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer();

    services
      .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
      .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptions) =>
        bearerOptions.TokenValidationParameters = new TokenValidationParameters {
          ValidIssuer = jwtOptions.Value.Issuer,
          ValidAudience = jwtOptions.Value.Audience,
          IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtOptions.Value.Key!)),
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
        });

    return services;
  }
}
