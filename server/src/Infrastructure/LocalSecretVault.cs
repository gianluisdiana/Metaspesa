using System.Diagnostics;

namespace Metaspesa.Infrastructure;

internal class LocalSecretVault : ISecretVault {
  private const string DockerSecretsDirectory = "/run/secrets";
  private static readonly ActivitySource ActivitySource = new(SecretsLoaderWorker.ActivitySourceName);

  public async Task<string?> ReadAsync(
    string name, CancellationToken cancellationToken = default
  ) {
    using Activity? activity = ActivitySource.StartActivity(
      "secrets.local_vault", ActivityKind.Internal);
    activity?.SetTag("secret.name", name);

    string path = Path.Combine(DockerSecretsDirectory, name);

    if (!File.Exists(path)) {
      activity?.SetTag("secret.found", false);
      return null;
    }
    activity?.SetTag("secret.found", true);

    return (await File.ReadAllTextAsync(path, cancellationToken)).Trim();
  }
}
