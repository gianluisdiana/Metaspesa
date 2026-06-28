using Metaspesa.Database.Entities;
using Metaspesa.Database.Repositories;

namespace Metaspesa.Database.IntegrationTests.Shopping;

public static class PostgreSqlProductRepositoryTests {
  [Collection("Database")]
  public class CheckProductExistsAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlProductRepository _repository;

    public CheckProductExistsAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlProductRepository(
        _context);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task<int> SeedPriceSnapshotAsync() {
      var market = new SuperMarketDbEntity { Name = $"Test market {Guid.CreateVersion7()}" };
      var brand = new ProductBrandDbEntity { Name = $"Test brand {Guid.CreateVersion7()}" };
      var unit = new UnitOfMeasureDbEntity {
        Code = $"u{Guid.CreateVersion7():N}"[..8],
        Name = $"Test unit {Guid.CreateVersion7()}",
      };
      var product = new ProductDbEntity {
        Name = $"Test product {Guid.CreateVersion7()}",
        SuperMarket = market,
        Brand = brand,
      };
      var format = new ProductFormatDbEntity {
        Product = product,
        Quantity = 1,
        UnitOfMeasure = unit,
        ImageUrl = "https://example.test/product.png",
      };
      var history = new PriceSnapshotDbEntity {
        ProductFormat = format,
        PriceAmount = 1.25m,
        ObservedAt = DateTime.UtcNow,
      };

      _context.PriceSnapshots.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return format.Id;
    }

    [Fact(
      DisplayName = "Returns true when product format reference exists")]
    public async Task CheckProductExistsAsync_ReturnsTrue_WhenProductFormatReferenceExists() {
      // Arrange
      int referenceUid = await SeedPriceSnapshotAsync();

      // Act
      bool result = await _repository.CheckProductExistsAsync(
        referenceUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }

    [Fact(
      DisplayName = "Returns false when product format reference does not exist")]
    public async Task CheckProductExistsAsync_ReturnsFalse_WhenProductFormatReferenceDoesNotExist() {
      // Arrange
      const int MissingReferenceUid = -1;

      // Act
      bool result = await _repository.CheckProductExistsAsync(
        MissingReferenceUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.False(result);
    }
  }
}
