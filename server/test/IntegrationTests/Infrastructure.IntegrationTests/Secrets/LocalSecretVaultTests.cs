namespace Metaspesa.Infrastructure.IntegrationTests.Secrets;

public static class LocalSecretVaultTests {
  private static readonly LocalSecretVault Vault = new();

  public class ReadAsync {
    [Fact(DisplayName = "Returns null when secret file does not exist")]
    public async Task ReadAsync_ReturnsNull_WhenFileDoesNotExist() {
      // Act
      string? result = await Vault.ReadAsync(
        "nonexistent-secret-xyz", TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns file content when secret exists (requires /run/secrets mount)")]
    public async Task ReadAsync_ReturnsContent_WhenFileExists() {
      // Act
      string? result = await Vault.ReadAsync(
        "jwt-key", TestContext.Current.CancellationToken);

      // Assert
      Assert.NotNull(result);
      Assert.NotEmpty(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Trims whitespace from file content (requires /run/secrets mount)")]
    public async Task ReadAsync_TrimsWhitespace_FromContent() {
      // Act
      string? result = await Vault.ReadAsync(
        "jwt-key", TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(result!.Trim(), result);
    }
  }
}
