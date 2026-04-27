using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Metaspesa.Infrastructure;

internal class SecretsLoaderWorker(
  IConfiguration configuration, ISecretVault vault
) : IHostedService {
  internal const string ActivitySourceName = "Metaspesa.Infrastructure";
  private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

  public async Task StartAsync(CancellationToken cancellationToken) {
    Debug.Assert(!string.IsNullOrEmpty(configuration["Jwt:KeySecretName"]));

    string secretKeyName = configuration["Jwt:KeySecretName"]!;

    using Activity? activity = ActivitySource.StartActivity(
      "secrets.load", ActivityKind.Internal);
    activity?.SetTag("secret.name", secretKeyName);

    try {
      string? secret = await vault.ReadAsync(secretKeyName, cancellationToken);
      if (secret is not null) {
        configuration["Jwt:Key"] = secret;
      }
      activity?.SetStatus(ActivityStatusCode.Ok);
    } catch (Exception ex) {
      activity?.AddException(ex);
      activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
      throw;
    }
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
