namespace Metaspesa.MigrationService;

internal static class SeedProfile {
  internal static bool IsIntegration(string? value) =>
    string.Equals(value, "integration", StringComparison.Ordinal);
}