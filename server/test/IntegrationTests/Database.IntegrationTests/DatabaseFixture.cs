using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Metaspesa.Database.IntegrationTests;

[CollectionDefinition("Database")]
public class DatabaseCollectionFixture : ICollectionFixture<DatabaseFixture>;

public class DatabaseFixture : IAsyncLifetime {
  private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
    .Build();

  public async ValueTask InitializeAsync() {
    await _container.StartAsync();
    await using MainContext context = CreateContext();
    await context.Database.MigrateAsync();
  }

  public async ValueTask DisposeAsync() {
    await _container.DisposeAsync();
    GC.SuppressFinalize(this);
  }

  internal MainContext CreateContext() {
    DbContextOptions<MainContext> options = new DbContextOptionsBuilder<MainContext>()
      .UseNpgsql(_container.GetConnectionString())
      .Options;
    return new MainContext(options);
  }

  internal async Task DeleteShoppingProductReferencesAsync(
    CancellationToken cancellationToken
  ) {
    await using MainContext context = CreateContext();
    await context.PurchaseItems.ExecuteDeleteAsync(cancellationToken);
    await context.ShoppingItems.ExecuteDeleteAsync(cancellationToken);
    await context.Purchases.ExecuteDeleteAsync(cancellationToken);
  }
}