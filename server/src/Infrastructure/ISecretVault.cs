namespace Metaspesa.Infrastructure;

internal interface ISecretVault {
  Task<string?> ReadAsync(
    string name, CancellationToken cancellationToken = default);
}
