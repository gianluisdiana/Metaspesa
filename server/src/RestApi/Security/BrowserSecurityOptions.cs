namespace Metaspesa.RestApi.Security;

internal sealed class BrowserSecurityOptions {
  public string[] AllowedOrigins { get; init; } = [];
}