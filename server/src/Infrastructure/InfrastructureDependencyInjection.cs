using Metaspesa.Application.Abstractions.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

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

    services.AddAuthorization();

    return services;
  }
}
