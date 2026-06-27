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

    private async Task<int> SeedProductHistoryAsync() {
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
      var history = new ProductsHistoryDbEntity {
        Product = product,
        ProductFormat = format,
        Price = 1.25m,
        CreatedAt = DateTime.UtcNow,
      };

      _context.ProductsHistory.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return history.Id;
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns true when product history reference exists")]
    public async Task CheckProductExistsAsync_ReturnsTrue_WhenProductHistoryReferenceExists() {
      // Arrange
      int referenceUid = await SeedProductHistoryAsync();

      // Act
      bool result = await _repository.CheckProductExistsAsync(
        referenceUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns false when product history reference does not exist")]
    public async Task CheckProductExistsAsync_ReturnsFalse_WhenProductHistoryReferenceDoesNotExist() {
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
