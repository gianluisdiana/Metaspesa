using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Metaspesa.Database.IntegrationTests;

[CollectionDefinition("Database")]
public sealed class DatabaseCollectionFixture : ICollectionFixture<DatabaseFixture>;

public sealed class DatabaseFixture : IAsyncLifetime {
  private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
    .Build();

  public async ValueTask InitializeAsync() {
    await _container.StartAsync();
    await using MainContext context = CreateContext();
    await context.Database.MigrateAsync();
  }

  public async ValueTask DisposeAsync() => await _container.DisposeAsync();

  internal MainContext CreateContext() {
    DbContextOptions<MainContext> options = new DbContextOptionsBuilder<MainContext>()
      .UseNpgsql(_container.GetConnectionString())
      .Options;
    return new MainContext(options);
  }
}