using Metaspesa.RestApi.Errors;
using Metaspesa.RestApi.Security;

namespace Metaspesa.RestApi;

internal static class ServiceCollectionExtensions {
  public static IServiceCollection AddRestApi(
    this IServiceCollection services, IConfiguration configuration
  ) {
    string[] allowedOrigins = configuration
      .GetSection("BrowserSecurity:AllowedOrigins")
      .Get<string[]>() ?? [];

    services.AddProblemDetails();
    services.AddExceptionHandler<RestExceptionHandler>();
    services.AddRequestDecompression();
    services.AddOptionsWithValidateOnStart<BrowserSecurityOptions>()
      .BindConfiguration("BrowserSecurity")
      .Validate(
        options => options.AllowedOrigins.Length > 0,
        "At least one browser origin must be configured");
    services.AddCors(options => options.AddDefaultPolicy(policy =>
      policy.WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

    return services;
  }
}