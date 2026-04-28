using Microsoft.Extensions.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Metaspesa.Infrastructure.UnitTests.Secrets;

public static class SecretsLoaderWorkerTests {
  private static IConfigurationRoot BuildConfig(string secretName = "jwt-key") =>
    new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?> {
        ["Jwt:KeySecretName"] = secretName,
      })
      .Build();

  public class StartAsync {
    [Fact(DisplayName = "Sets Jwt:Key in configuration when vault returns a secret")]
    public async Task StartAsync_SetsJwtKey_WhenVaultReturnsSecret() {
      // Arrange
      IConfigurationRoot config = BuildConfig();
      ISecretVault vault = Substitute.For<ISecretVault>();
      vault.ReadAsync("jwt-key", Arg.Any<CancellationToken>()).Returns("super-secret-value");
      SecretsLoaderWorker worker = new(config, vault);

      // Act
      await worker.StartAsync(TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal("super-secret-value", config["Jwt:Key"]);
    }

    [Fact(DisplayName = "Does not set Jwt:Key when vault returns null")]
    public async Task StartAsync_DoesNotSetJwtKey_WhenVaultReturnsNull() {
      // Arrange
      IConfigurationRoot config = BuildConfig();
      ISecretVault vault = Substitute.For<ISecretVault>();
      vault.ReadAsync("jwt-key", Arg.Any<CancellationToken>())
        .Returns((string?)null);
      SecretsLoaderWorker worker = new(config, vault);

      // Act
      await worker.StartAsync(TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(config["Jwt:Key"]);
    }

    [Fact(DisplayName = "Propagates exception thrown by the vault")]
    public async Task StartAsync_PropagatesException_WhenVaultThrows() {
      // Arrange
      IConfigurationRoot config = BuildConfig();
      InvalidOperationException expectedException = new("vault unavailable");
      ISecretVault vault = Substitute.For<ISecretVault>();
      vault.ReadAsync("jwt-key", Arg.Any<CancellationToken>())
        .ThrowsAsync(expectedException);
      SecretsLoaderWorker worker = new(config, vault);

      // Act
      Exception? thrown = await Record.ExceptionAsync(() =>
        worker.StartAsync(TestContext.Current.CancellationToken));

      // Assert
      Assert.Same(expectedException, thrown);
    }

    [Fact(DisplayName = "Overwrites an existing Jwt:Key value with the value from vault")]
    public async Task StartAsync_OverwritesExistingJwtKey_WithVaultValue() {
      // Arrange
      IConfigurationRoot config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> {
          ["Jwt:KeySecretName"] = "jwt-key",
          ["Jwt:Key"] = "old-key",
        })
        .Build();
      ISecretVault vault = Substitute.For<ISecretVault>();
      vault.ReadAsync("jwt-key", Arg.Any<CancellationToken>()).Returns("new-key");
      SecretsLoaderWorker worker = new(config, vault);

      // Act
      await worker.StartAsync(TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal("new-key", config["Jwt:Key"]);
    }
  }

  public class StopAsync {
    [Fact(DisplayName = "Completes without error")]
    public async Task StopAsync_CompletesSuccessfully() {
      // Arrange
      ISecretVault vault = Substitute.For<ISecretVault>();
      SecretsLoaderWorker worker = new(BuildConfig(), vault);

      // Act
      Exception? thrown = await Record.ExceptionAsync(() =>
        worker.StopAsync(TestContext.Current.CancellationToken));

      // Assert
      Assert.Null(thrown);
    }
  }
}
