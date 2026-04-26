using Metaspesa.Database.Entities;
using Metaspesa.Database.Repositories;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Metaspesa.Database.IntegrationTests.Markets;

public static class PostgreSqlMarketRepositoryTests {
  [Collection("Database")]
  public class GetBrandsAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public GetBrandsAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context, NullLogger<PostgreSqlMarketRepository>.Instance);
    }

    public async ValueTask InitializeAsync() {
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.ProductBrands.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns empty list when no brands exist")]
    public async Task Repository_ReturnsEmptyList_WhenNoBrandsExist() {
      // Arrange & Act
      List<ProductBrand> result = await _repository.GetBrandsAsync(
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns all brands")]
    public async Task Repository_ReturnsAllBrands() {
      // Arrange
      await _repository.AddBrandsAsync(
        [new ProductBrand("Nike"), new ProductBrand("Adidas")],
        TestContext.Current.CancellationToken);

      // Act
      List<ProductBrand> result = await _repository.GetBrandsAsync(
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(2, result.Count);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Maps brand name correctly")]
    public async Task Repository_MapsBrandName_Correctly() {
      // Arrange
      await _repository.AddBrandsAsync(
        [new ProductBrand("Puma")],
        TestContext.Current.CancellationToken);

      // Act
      List<ProductBrand> result = await _repository.GetBrandsAsync(
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal("Puma", result.Single().Name);
    }
  }

  [Collection("Database")]
  public class AddBrandsTests : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public AddBrandsTests(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context, NullLogger<PostgreSqlMarketRepository>.Instance);
    }

    public async ValueTask InitializeAsync() {
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.ProductBrands.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Persists new brands to the database")]
    public async Task Repository_PersistsNewBrands_ToDatabase() {
      // Act
      await _repository.AddBrandsAsync(
        [new ProductBrand("Nike"), new ProductBrand("Adidas")],
        TestContext.Current.CancellationToken);

      // Assert
      List<string> names = await _context.ProductBrands
        .AsNoTracking()
        .Select(b => b.Name)
        .ToListAsync(TestContext.Current.CancellationToken);
      Assert.Contains("Nike", names);
      Assert.Contains("Adidas", names);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Persists brand name correctly")]
    public async Task Repository_PersistsBrandName_Correctly() {
      // Act
      await _repository.AddBrandsAsync(
        [new ProductBrand("Puma")],
        TestContext.Current.CancellationToken);

      // Assert
      ProductBrandDbEntity brand = await _context.ProductBrands
        .AsNoTracking()
        .FirstAsync(
          b => b.Name == "Puma",
          TestContext.Current.CancellationToken);
      Assert.Equal("Puma", brand.Name);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Persists multiple brands in single call")]
    public async Task Repository_PersistsMultipleBrands_InSingleCall() {
      // Act
      await _repository.AddBrandsAsync(
        [
          new ProductBrand("BrandA"),
          new ProductBrand("BrandB"),
          new ProductBrand("BrandC"),
        ],
        TestContext.Current.CancellationToken);

      // Assert
      int count = await _context.ProductBrands
        .AsNoTracking()
        .CountAsync(
          b => new[] { "BrandA", "BrandB", "BrandC" }.Contains(b.Name),
          TestContext.Current.CancellationToken);
      Assert.Equal(3, count);
    }
  }

  [Collection("Database")]
  public class GetMarketsAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public GetMarketsAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context, NullLogger<PostgreSqlMarketRepository>.Instance);
    }

    public async ValueTask InitializeAsync() {
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.SuperMarkets.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns empty list when no markets exist")]
    public async Task Repository_ReturnsEmptyList_WhenNoMarketsExist() {
      // Arrange & Act
      List<Market> result = await _repository.GetMarketsAsync(
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns all markets")]
    public async Task Repository_ReturnsAllMarkets() {
      // Arrange
      await _repository.AddMarketsAsync(
        [new Market("Walmart", []), new Market("Carrefour", [])],
        TestContext.Current.CancellationToken);

      // Act
      List<Market> result = await _repository.GetMarketsAsync(
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(2, result.Count);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Maps market name correctly")]
    public async Task Repository_MapsMarketName_Correctly() {
      // Arrange
      await _repository.AddMarketsAsync(
        [new Market("Lidl", [])],
        TestContext.Current.CancellationToken);

      // Act
      List<Market> result = await _repository.GetMarketsAsync(
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal("Lidl", result.Single().Name);
    }
  }

  [Collection("Database")]
  public class AddMarketsTests : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public AddMarketsTests(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context, NullLogger<PostgreSqlMarketRepository>.Instance);
    }

    public async ValueTask InitializeAsync() {
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.SuperMarkets.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Persists new markets to the database")]
    public async Task Repository_PersistsNewMarkets_ToDatabase() {
      // Act
      await _repository.AddMarketsAsync(
        [new Market("Walmart", []), new Market("Carrefour", [])],
        TestContext.Current.CancellationToken);

      // Assert
      List<string> names = await _context.SuperMarkets
        .AsNoTracking()
        .Select(m => m.Name)
        .ToListAsync(TestContext.Current.CancellationToken);
      Assert.Contains("Walmart", names);
      Assert.Contains("Carrefour", names);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Persists market name correctly")]
    public async Task Repository_PersistsMarketName_Correctly() {
      // Act
      await _repository.AddMarketsAsync(
        [new Market("Lidl", [])],
        TestContext.Current.CancellationToken);

      // Assert
      SuperMarketDbEntity market = await _context.SuperMarkets
        .AsNoTracking()
        .FirstAsync(
          m => m.Name == "Lidl",
          TestContext.Current.CancellationToken);
      Assert.Equal("Lidl", market.Name);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Persists multiple markets in single call")]
    public async Task Repository_PersistsMultipleMarkets_InSingleCall() {
      // Act
      await _repository.AddMarketsAsync(
        [
          new Market("MarketA", []),
          new Market("MarketB", []),
          new Market("MarketC", []),
        ],
        TestContext.Current.CancellationToken);

      // Assert
      int count = await _context.SuperMarkets
        .AsNoTracking()
        .CountAsync(
          m => new[] { "MarketA", "MarketB", "MarketC" }.Contains(m.Name),
          TestContext.Current.CancellationToken);
      Assert.Equal(3, count);
    }
  }

  [Collection("Database")]
  public class AddProductsTests : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;
    private const string MarketName = "TestMarketForProducts";
    private const string BrandName = "TestBrandForProducts";

    public AddProductsTests(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context, NullLogger<PostgreSqlMarketRepository>.Instance);
    }

    public async ValueTask InitializeAsync() {
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);

      bool marketExists = await _context.SuperMarkets
        .AnyAsync(m => m.Name == MarketName, TestContext.Current.CancellationToken);
      if (!marketExists) {
        _context.SuperMarkets.Add(new SuperMarketDbEntity { Name = MarketName });
      }

      bool brandExists = await _context.ProductBrands
        .AnyAsync(b => b.Name == BrandName, TestContext.Current.CancellationToken);
      if (!brandExists) {
        _context.ProductBrands.Add(new ProductBrandDbEntity { Name = BrandName });
      }

      if (_context.ChangeTracker.HasChanges()) {
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      }
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private static Market MakeMarket(string productName, float price, string quantity) =>
      new Market(MarketName, [
        new MarketProduct(productName, quantity, new Price(price), new ProductBrand(BrandName))
      ]);

    [Fact(
      Explicit = true,
      DisplayName = "Creates new product when it does not exist")]
    public async Task Repository_CreatesNewProduct_WhenItDoesNotExist() {
      // Act
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProduct1", 1.99f, "1kg"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Assert
      ProductDbEntity? dbProduct = await _context.Products
        .AsNoTracking()
        .FirstOrDefaultAsync(
          p => p.Name == "IntegrationProduct1",
          TestContext.Current.CancellationToken);
      Assert.NotNull(dbProduct);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Adds history entry for product")]
    public async Task Repository_AddsHistoryEntry_ForProduct() {
      // Act
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProduct2", 2.50f, "500ml"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Assert
      int historyCount = await _context.ProductsHistory
        .AsNoTracking()
        .CountAsync(
          h => h.Product.Name == "IntegrationProduct2",
          TestContext.Current.CancellationToken);
      Assert.Equal(1, historyCount);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Uses provided registered_at for history entry")]
    public async Task Repository_UsesProvidedRegisteredAt_ForHistoryEntry() {
      // Arrange
      var registeredAt = new DateOnly(2024, 1, 15);
      var expectedCreatedAt = registeredAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

      // Act
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProduct3", 3.00f, "1L"),
        registeredAt,
        TestContext.Current.CancellationToken);

      // Assert
      ProductsHistoryDbEntity history = await _context.ProductsHistory
        .AsNoTracking()
        .FirstAsync(
          h => h.Product.Name == "IntegrationProduct3",
          TestContext.Current.CancellationToken);
      Assert.Equal(expectedCreatedAt, history.CreatedAt);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Reuses existing product and adds new history entry")]
    public async Task Repository_ReusesExistingProduct_AddsNewHistoryEntry() {
      // Arrange
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProduct4", 1.00f, "1pc"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Act — add same product again with different price
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProduct4", 1.50f, "1pc"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Assert — 1 product, 2 history entries
      int productCount = await _context.Products
        .AsNoTracking()
        .CountAsync(p => p.Name == "IntegrationProduct4", TestContext.Current.CancellationToken);
      int historyCount = await _context.ProductsHistory
        .AsNoTracking()
        .CountAsync(
          h => h.Product.Name == "IntegrationProduct4",
          TestContext.Current.CancellationToken);
      Assert.Equal(1, productCount);
      Assert.Equal(2, historyCount);
    }
  }
}
