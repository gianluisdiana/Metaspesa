using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
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
        new MarketProduct(
          productName,
          new ProductBrand(BrandName),
          [new ProductFormat(quantity, new Price(price))])
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

  [Collection("Database")]
  public class AddMarketProductsReturnsTests : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;
    private const string MarketName = "TestMarketForReturns";
    private const string BrandName = "TestBrandForReturns";

    public AddMarketProductsReturnsTests(DatabaseFixture fixture) {
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

    private static Market MakeMarket(params string[] productNames) =>
      new(MarketName, [
        ..productNames.Select(n => new MarketProduct(
          n,
          new ProductBrand(BrandName),
          [new ProductFormat("1kg", new Price(1.00f))]
        ))
      ]);

    [Fact(
      Explicit = true,
      DisplayName = "Returns IDs of newly created products")]
    public async Task Repository_ReturnsNewProductIds_WhenProductsAreNew() {
      // Act
      IReadOnlyCollection<int> result = await _repository.AddMarketProductsAsync(
        MakeMarket("ReturnProduct1", "ReturnProduct2"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(2, result.Count);
      Assert.All(result, id => Assert.True(id > 0));
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns empty collection when all products already exist")]
    public async Task Repository_ReturnsEmptyCollection_WhenAllProductsAlreadyExist() {
      // Arrange — seed the product first
      await _repository.AddMarketProductsAsync(
        MakeMarket("ExistingReturnProduct"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Act — same product again
      IReadOnlyCollection<int> result = await _repository.AddMarketProductsAsync(
        MakeMarket("ExistingReturnProduct"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result);
    }
  }

  [Collection("Database")]
  public class DeleteBrandsTests : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public DeleteBrandsTests(DatabaseFixture fixture) {
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
      DisplayName = "Deletes specified brands")]
    public async Task Repository_DeletesSpecifiedBrands() {
      // Arrange
      await _repository.AddBrandsAsync(
        [new ProductBrand("BrandToDelete"), new ProductBrand("BrandToKeep")],
        TestContext.Current.CancellationToken);

      // Act
      await _repository.DeleteBrandsAsync(
        ["BrandToDelete"],
        TestContext.Current.CancellationToken);

      // Assert
      bool exists = await _context.ProductBrands
        .AsNoTracking()
        .AnyAsync(b => b.Name == "BrandToDelete", TestContext.Current.CancellationToken);
      Assert.False(exists);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Does not delete other brands")]
    public async Task Repository_DoesNotDeleteOtherBrands() {
      // Arrange
      await _repository.AddBrandsAsync(
        [new ProductBrand("BrandToDelete2"), new ProductBrand("BrandToKeep2")],
        TestContext.Current.CancellationToken);

      // Act
      await _repository.DeleteBrandsAsync(
        ["BrandToDelete2"],
        TestContext.Current.CancellationToken);

      // Assert
      bool exists = await _context.ProductBrands
        .AsNoTracking()
        .AnyAsync(b => b.Name == "BrandToKeep2", TestContext.Current.CancellationToken);
      Assert.True(exists);
    }
  }

  [Collection("Database")]
  public class DeleteMarketsTests : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public DeleteMarketsTests(DatabaseFixture fixture) {
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
      DisplayName = "Deletes specified markets")]
    public async Task Repository_DeletesSpecifiedMarkets() {
      // Arrange
      await _repository.AddMarketsAsync(
        [new Market("MarketToDelete", []), new Market("MarketToKeep", [])],
        TestContext.Current.CancellationToken);

      // Act
      await _repository.DeleteMarketsAsync(
        ["MarketToDelete"],
        TestContext.Current.CancellationToken);

      // Assert
      bool exists = await _context.SuperMarkets
        .AsNoTracking()
        .AnyAsync(m => m.Name == "MarketToDelete", TestContext.Current.CancellationToken);
      Assert.False(exists);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Does not delete other markets")]
    public async Task Repository_DoesNotDeleteOtherMarkets() {
      // Arrange
      await _repository.AddMarketsAsync(
        [new Market("MarketToDelete2", []), new Market("MarketToKeep2", [])],
        TestContext.Current.CancellationToken);

      // Act
      await _repository.DeleteMarketsAsync(
        ["MarketToDelete2"],
        TestContext.Current.CancellationToken);

      // Assert
      bool exists = await _context.SuperMarkets
        .AsNoTracking()
        .AnyAsync(m => m.Name == "MarketToKeep2", TestContext.Current.CancellationToken);
      Assert.True(exists);
    }
  }

  [Collection("Database")]
  public class DeleteProductsTests : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;
    private const string MarketName = "TestMarketForDeleting";
    private const string BrandName = "TestBrandForDeleting";

    public DeleteProductsTests(DatabaseFixture fixture) {
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

    private static Market MakeMarket(string productName) =>
      new(MarketName, [
        new MarketProduct(
          productName,
          new ProductBrand(BrandName),
          [new ProductFormat("1kg", new Price(1.00f))]
        )
      ]);

    [Fact(
      Explicit = true,
      DisplayName = "Deletes specified products")]
    public async Task Repository_DeletesSpecifiedProducts() {
      // Arrange
      IReadOnlyCollection<int> ids = await _repository.AddMarketProductsAsync(
        MakeMarket("DeleteProduct1"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);
      int id = ids.Single();

      // Act
      await _repository.DeleteProductsAsync([id], TestContext.Current.CancellationToken);

      // Assert
      bool exists = await _context.Products
        .AsNoTracking()
        .AnyAsync(p => p.Id == id, TestContext.Current.CancellationToken);
      Assert.False(exists);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Does not delete other products")]
    public async Task Repository_DoesNotDeleteOtherProducts() {
      // Arrange
      IReadOnlyCollection<int> idsToDelete = await _repository.AddMarketProductsAsync(
        MakeMarket("DeleteProduct2"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);
      IReadOnlyCollection<int> idsToKeep = await _repository.AddMarketProductsAsync(
        MakeMarket("KeepProduct2"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Act
      await _repository.DeleteProductsAsync(idsToDelete, TestContext.Current.CancellationToken);

      // Assert
      bool exists = await _context.Products
        .AsNoTracking()
        .AnyAsync(p => p.Id == idsToKeep.Single(), TestContext.Current.CancellationToken);
      Assert.True(exists);
    }
  }

  [Collection("Database")]
  public class GetProductsAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;
    private const string MarketA = "GetProductsMarketA";
    private const string MarketB = "GetProductsMarketB";
    private const string BrandA = "GetProductsBrandA";
    private const string BrandB = "GetProductsBrandB";

    public GetProductsAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context, NullLogger<PostgreSqlMarketRepository>.Instance);
    }

    public async ValueTask InitializeAsync() {
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);

      foreach (string name in new[] { MarketA, MarketB }) {
        bool exists = await _context.SuperMarkets
          .AnyAsync(m => m.Name == name, TestContext.Current.CancellationToken);
        if (!exists) {
          _context.SuperMarkets.Add(new SuperMarketDbEntity { Name = name });
        }
      }

      foreach (string name in new[] { BrandA, BrandB }) {
        bool exists = await _context.ProductBrands
          .AnyAsync(b => b.Name == name, TestContext.Current.CancellationToken);
        if (!exists) {
          _context.ProductBrands.Add(new ProductBrandDbEntity { Name = name });
        }
      }

      if (_context.ChangeTracker.HasChanges()) {
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      }
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task SeedProductWithHistoryAsync(
      string marketName, string brandName, string productName,
      float price, string quantity, DateTime createdAt
    ) {
      int marketId = await _context.SuperMarkets
        .Where(m => m.Name == marketName)
        .Select(m => m.Id)
        .SingleAsync(TestContext.Current.CancellationToken);

      int brandId = await _context.ProductBrands
        .Where(b => b.Name == brandName)
        .Select(b => b.Id)
        .SingleAsync(TestContext.Current.CancellationToken);

      ProductDbEntity? existing = await _context.Products
        .FirstOrDefaultAsync(
          p => p.Name == productName && p.SuperMarketId == marketId && p.BrandId == brandId,
          TestContext.Current.CancellationToken);

      if (existing is null) {
        existing = new ProductDbEntity {
          Name = productName,
          SuperMarketId = marketId,
          BrandId = brandId,
        };
        _context.Products.Add(existing);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      }

      _context.ProductsHistory.Add(new ProductsHistoryDbEntity {
        ProductId = existing.Id,
        Price = price,
        Quantity = quantity,
        CreatedAt = createdAt,
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static GetMarketProductsFilter Filter(
      string? market = null, string? brand = null, string? segment = null,
      Pagination? pagination = null
    ) => new(market, brand, segment, pagination);

    private static GetMarketProductsFilter FilterWithPage(
      string? market = null, string? brand = null, string? segment = null,
      int page = 1, int pageSize = 100
    ) => new(market, brand, segment, new Pagination(page, pageSize));

    [Fact(
      Explicit = true,
      DisplayName = "Returns empty values when no products exist")]
    public async Task Repository_ReturnsEmptyValues_WhenNoProductsExist() {
      // Arrange & Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(), TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result.Values);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns zero total count when no products exist")]
    public async Task Repository_ReturnsZeroTotalCount_WhenNoProductsExist() {
      // Arrange & Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(), TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(0, result.TotalCount);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns products without filter")]
    public async Task Repository_ReturnsProducts_WithoutFilter() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche Entera GP", 0.89f, "1L", now);
      await SeedProductWithHistoryAsync(MarketB, BrandB, "Pan Blanco GP", 1.20f, "500g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(), TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result.Values.SelectMany(m => m.Products).Count() >= 2);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns total count without filter")]
    public async Task Repository_ReturnsTotalCount_WithoutFilter() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche Entera TCF", 0.89f, "1L", now);
      await SeedProductWithHistoryAsync(MarketB, BrandB, "Pan Blanco TCF", 1.20f, "500g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(), TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result.TotalCount >= 2);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Groups products under their market")]
    public async Task Repository_GroupsProducts_UnderTheirMarket() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche GroupTest", 0.89f, "1L", now);
      await SeedProductWithHistoryAsync(MarketB, BrandB, "Pan GroupTest", 1.20f, "500g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(), TestContext.Current.CancellationToken);

      // Assert
      Market marketA = result.Values.Single(m => m.Name == MarketA);
      Assert.Contains(marketA.Products, p => p.Name == "Leche GroupTest");
    }

    [Fact(
      Explicit = true,
      DisplayName = "Filters by market name returns only matching market (case-insensitive)")]
    public async Task Repository_FiltersByMarketName_ReturnsOnlyMatchingMarket() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche MarketFilter", 0.89f, "1L", now);
      await SeedProductWithHistoryAsync(MarketB, BrandB, "Pan MarketFilter", 1.20f, "500g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(market: MarketA.ToUpperInvariant()), TestContext.Current.CancellationToken);

      // Assert
      Assert.Single(result.Values);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Filters by market name returns correct market name (case-insensitive)")]
    public async Task Repository_FiltersByMarketName_ReturnsCorrectMarketName() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche MarketNameFilter", 0.89f, "1L", now);
      await SeedProductWithHistoryAsync(MarketB, BrandB, "Pan MarketNameFilter", 1.20f, "500g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(market: MarketA.ToUpperInvariant()), TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(MarketA, result.Values.Single().Name);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Filters by brand name (case-insensitive)")]
    public async Task Repository_FiltersByBrandName_CaseInsensitive() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche BrandFilter", 0.89f, "1L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandB, "Pan BrandFilter", 1.20f, "500g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(brand: BrandA.ToUpperInvariant()), TestContext.Current.CancellationToken);

      // Assert
      IEnumerable<MarketProduct> products = result.Values.SelectMany(m => m.Products);
      Assert.All(products, p => Assert.Equal(BrandA, p.Brand.Name));
    }

    [Fact(
      Explicit = true,
      DisplayName = "Filters by name segment (case-insensitive contains)")]
    public async Task Repository_FiltersByNameSegment_CaseInsensitiveContains() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche Entera SegFilter", 0.89f, "1L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Pan Blanco SegFilter", 1.20f, "500g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(segment: "leche entera segfilter"), TestContext.Current.CancellationToken);

      // Assert
      MarketProduct product = result.Values.SelectMany(m => m.Products).Single();
      Assert.Equal("Leche Entera SegFilter", product.Name);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns only one format from the latest history date per product")]
    public async Task Repository_ReturnsOnlyOneFormat_FromLatestHistoryDate() {
      // Arrange
      DateTime older = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      DateTime newer = new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche HistoryTest", 0.79f, "1L", older);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche HistoryTest", 0.89f, "1L", newer);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(segment: "Leche HistoryTest"), TestContext.Current.CancellationToken);

      // Assert
      MarketProduct product = result.Values.SelectMany(m => m.Products).Single();
      Assert.Single(product.Formats);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns format with price from the latest history date per product")]
    public async Task Repository_ReturnsLatestPrice_PerProduct() {
      // Arrange
      DateTime older = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      DateTime newer = new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche LatestPriceTest", 0.79f, "1L", older);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche LatestPriceTest", 0.89f, "1L", newer);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(segment: "Leche LatestPriceTest"), TestContext.Current.CancellationToken);

      // Assert
      MarketProduct product = result.Values.SelectMany(m => m.Products).Single();
      Assert.Equal(0.89f, product.Formats.Single().Price.Value, precision: 2);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Excludes products with no history")]
    public async Task Repository_ExcludesProducts_WithNoHistory() {
      // Arrange
      int marketId = await _context.SuperMarkets
        .Where(m => m.Name == MarketA)
        .Select(m => m.Id)
        .SingleAsync(TestContext.Current.CancellationToken);
      int brandId = await _context.ProductBrands
        .Where(b => b.Name == BrandA)
        .Select(b => b.Id)
        .SingleAsync(TestContext.Current.CancellationToken);

      _context.Products.Add(new ProductDbEntity {
        Name = "NoHistoryProduct",
        SuperMarketId = marketId,
        BrandId = brandId,
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(segment: "NoHistoryProduct"), TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result.Values);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Respects pagination — total count reflects all matching products")]
    public async Task Repository_Pagination_TotalCountReflectsAllMatches() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche Page1", 1.00f, "1L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche Page2", 1.10f, "1L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche Page3", 1.20f, "1L", now);

      // Act — page 2 with pageSize 1, filtered by segment "Leche Page"
      PagedResult<Market> result = await _repository.GetProductsAsync(
        FilterWithPage(segment: "Leche Page", page: 2, pageSize: 1),
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(3, result.TotalCount);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Respects pagination — returns single product on page 2 with page size 1")]
    public async Task Repository_Pagination_ReturnsSingleProductOnSecondPage() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche PgB1", 1.00f, "1L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche PgB2", 1.10f, "1L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche PgB3", 1.20f, "1L", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        FilterWithPage(segment: "Leche PgB", page: 2, pageSize: 1),
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Single(result.Values.SelectMany(m => m.Products));
    }
  }

  [Collection("Database")]
  public class DeleteProductsHistoryForMarketsTests : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;
    private const string MarketName = "TestMarketForHistory";
    private const string OtherMarketName = "OtherMarketForHistory";
    private const string BrandName = "TestBrandForHistory";

    public DeleteProductsHistoryForMarketsTests(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context, NullLogger<PostgreSqlMarketRepository>.Instance);
    }

    public async ValueTask InitializeAsync() {
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);

      foreach (string name in new[] { MarketName, OtherMarketName }) {
        bool exists = await _context.SuperMarkets
          .AnyAsync(m => m.Name == name, TestContext.Current.CancellationToken);
        if (!exists) {
          _context.SuperMarkets.Add(new SuperMarketDbEntity { Name = name });
        }
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

    private static Market MakeMarket(string marketName, string productName) =>
      new(marketName, [
        new MarketProduct(
          productName,
          new ProductBrand(BrandName),
          [new ProductFormat("1kg", new Price(1.00f))]
        )
      ]);

    [Fact(
      Explicit = true,
      DisplayName = "Deletes history for specified markets at specified date")]
    public async Task Repository_DeletesHistory_ForSpecifiedMarketsAtDate() {
      // Arrange
      var date = new DateOnly(2024, 6, 1);
      await _repository.AddMarketProductsAsync(
        MakeMarket(MarketName, "HistoryProduct1"),
        date,
        TestContext.Current.CancellationToken);

      // Act
      await _repository.DeleteProductsHistoryForMarketsAsync(
        [MarketName],
        date,
        TestContext.Current.CancellationToken);

      // Assert
      var expectedDate = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
      bool historyExists = await _context.ProductsHistory
        .AsNoTracking()
        .AnyAsync(
          h => h.Product.SuperMarket.Name == MarketName && h.CreatedAt == expectedDate,
          TestContext.Current.CancellationToken);
      Assert.False(historyExists);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Does not delete history for other markets")]
    public async Task Repository_DoesNotDeleteHistory_ForOtherMarkets() {
      // Arrange
      var date = new DateOnly(2024, 6, 2);
      await _repository.AddMarketProductsAsync(
        MakeMarket(MarketName, "HistoryProduct2"),
        date,
        TestContext.Current.CancellationToken);
      await _repository.AddMarketProductsAsync(
        MakeMarket(OtherMarketName, "OtherHistoryProduct2"),
        date,
        TestContext.Current.CancellationToken);

      // Act
      await _repository.DeleteProductsHistoryForMarketsAsync(
        [MarketName],
        date,
        TestContext.Current.CancellationToken);

      // Assert
      var expectedDate = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
      bool otherHistoryExists = await _context.ProductsHistory
        .AsNoTracking()
        .AnyAsync(
          h => h.Product.SuperMarket.Name == OtherMarketName && h.CreatedAt == expectedDate,
          TestContext.Current.CancellationToken);
      Assert.True(otherHistoryExists);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Does not delete history for same market at different date")]
    public async Task Repository_DoesNotDeleteHistory_ForSameMarketAtDifferentDate() {
      // Arrange
      var targetDate = new DateOnly(2024, 6, 3);
      var otherDate = new DateOnly(2024, 6, 4);
      await _repository.AddMarketProductsAsync(
        MakeMarket(MarketName, "HistoryProduct3"),
        targetDate,
        TestContext.Current.CancellationToken);
      await _repository.AddMarketProductsAsync(
        MakeMarket(MarketName, "HistoryProduct3b"),
        otherDate,
        TestContext.Current.CancellationToken);

      // Act
      await _repository.DeleteProductsHistoryForMarketsAsync(
        [MarketName],
        targetDate,
        TestContext.Current.CancellationToken);

      // Assert
      var otherExpectedDate = otherDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
      bool otherHistoryExists = await _context.ProductsHistory
        .AsNoTracking()
        .AnyAsync(
          h => h.Product.SuperMarket.Name == MarketName && h.CreatedAt == otherExpectedDate,
          TestContext.Current.CancellationToken);
      Assert.True(otherHistoryExists);
    }
  }
}
