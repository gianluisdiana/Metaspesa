namespace Metaspesa.Infrastructure;

internal class JwtOptions {
  public string? Key { get; init; }
  public required string KeySecretName { get; init; }
  public required string Issuer { get; init; }
  public required string Audience { get; init; }
  public int ExpirationMinutes { get; init; } = 60;
}
