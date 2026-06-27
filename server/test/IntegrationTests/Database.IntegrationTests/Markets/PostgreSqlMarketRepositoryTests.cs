using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Database.Entities;
using Metaspesa.Database.Repositories;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.IntegrationTests.Markets;

public static class PostgreSqlMarketRepositoryTests {
  private static ProductFormat MakeProductFormat(
    decimal price = 1.00m,
    float quantity = 1,
    string unitOfMeasure = "kg",
    Uri? imageUrl = null
  ) => new(
    new AQuantity(quantity, unitOfMeasure),
    new Price(price),
    imageUrl);

  private static async Task EnsureUnitOfMeasureAsync(
    MainContext context,
    string code,
    string? name = null
  ) {
    bool exists = await context.UnitsOfMeasure
      .AnyAsync(u => u.Code == code, TestContext.Current.CancellationToken);
    if (exists) {
      return;
    }

    context.UnitsOfMeasure.Add(new UnitOfMeasureDbEntity {
      Code = code,
      Name = name ?? $"Test unit {code}"
    });
    await context.SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Collection("Database")]
  public class GetBrandsAsync : IAsyncLifetime {
    private readonly DatabaseFixture _fixture;
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public GetBrandsAsync(DatabaseFixture fixture) {
      _fixture = fixture;
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public async ValueTask InitializeAsync() {
      await _fixture.DeleteShoppingProductReferencesAsync(TestContext.Current.CancellationToken);
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.ProductBrands.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      DisplayName = "Returns empty list when no brands exist")]
    public async Task Repository_ReturnsEmptyList_WhenNoBrandsExist() {
      // Arrange & Act
      List<ProductBrand> result = await _repository.GetBrandsAsync(
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result);
    }

    [Fact(
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
    private readonly DatabaseFixture _fixture;
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public AddBrandsTests(DatabaseFixture fixture) {
      _fixture = fixture;
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public async ValueTask InitializeAsync() {
      await _fixture.DeleteShoppingProductReferencesAsync(TestContext.Current.CancellationToken);
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.ProductBrands.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
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
    private readonly DatabaseFixture _fixture;
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public GetMarketsAsync(DatabaseFixture fixture) {
      _fixture = fixture;
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public async ValueTask InitializeAsync() {
      await _fixture.DeleteShoppingProductReferencesAsync(TestContext.Current.CancellationToken);
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.SuperMarkets.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      DisplayName = "Returns empty list when no markets exist")]
    public async Task Repository_ReturnsEmptyList_WhenNoMarketsExist() {
      // Arrange & Act
      List<Market> result = await _repository.GetMarketsAsync(
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result);
    }

    [Fact(
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
    private readonly DatabaseFixture _fixture;
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public AddMarketsTests(DatabaseFixture fixture) {
      _fixture = fixture;
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public async ValueTask InitializeAsync() {
      await _fixture.DeleteShoppingProductReferencesAsync(TestContext.Current.CancellationToken);
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.SuperMarkets.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
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
    private readonly DatabaseFixture _fixture;
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;
    private const string MarketName = "TestMarketForProducts";
    private const string BrandName = "TestBrandForProducts";

    public AddProductsTests(DatabaseFixture fixture) {
      _fixture = fixture;
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public async ValueTask InitializeAsync() {
      await _fixture.DeleteShoppingProductReferencesAsync(TestContext.Current.CancellationToken);
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

      await EnsureUnitOfMeasureAsync(_context, "kg");
      await EnsureUnitOfMeasureAsync(_context, "ml");
      await EnsureUnitOfMeasureAsync(_context, "L");
      await EnsureUnitOfMeasureAsync(_context, "pc");
      await EnsureUnitOfMeasureAsync(_context, "g");
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private static Market MakeMarket(
      string productName,
      decimal price,
      float quantity,
      string unitOfMeasure
    ) =>
      new Market(MarketName, [
        new MarketProduct(
          productName,
          new ProductBrand(BrandName),
          [MakeProductFormat(price, quantity, unitOfMeasure)])
      ]);

    [Fact(
      DisplayName = "Creates new product when it does not exist")]
    public async Task Repository_CreatesNewProduct_WhenItDoesNotExist() {
      // Act
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProduct1", 1.99m, 1, "kg"),
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
      DisplayName = "Adds history entry for product")]
    public async Task Repository_AddsHistoryEntry_ForProduct() {
      // Act
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProduct2", 2.50m, 500, "ml"),
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
      DisplayName = "Stores product format quantity separately from unit")]
    public async Task Repository_StoresProductFormatQuantity_SeparatelyFromUnit() {
      // Act
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProductFormatQuantity", 2.50m, 500, "ml"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Assert
      decimal quantity = await _context.ProductFormats
        .AsNoTracking()
        .Where(f => f.Product.Name == "IntegrationProductFormatQuantity")
        .Select(f => f.Quantity)
        .SingleAsync(TestContext.Current.CancellationToken);
      Assert.Equal(500m, quantity);
    }

    [Fact(
      DisplayName = "Stores product format unit of measure by code")]
    public async Task Repository_StoresProductFormatUnitOfMeasure_ByCode() {
      // Act
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProductFormatUnit", 2.50m, 500, "ml"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Assert
      string unitCode = await _context.ProductFormats
        .AsNoTracking()
        .Where(f => f.Product.Name == "IntegrationProductFormatUnit")
        .Select(f => f.UnitOfMeasure.Code)
        .SingleAsync(TestContext.Current.CancellationToken);
      Assert.Equal("ml", unitCode);
    }

    [Fact(
      DisplayName = "Links history entry to stored product format")]
    public async Task Repository_LinksHistoryEntry_ToStoredProductFormat() {
      // Act
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProductHistoryFormat", 2.50m, 500, "ml"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Assert
      int productFormatId = await _context.ProductFormats
        .AsNoTracking()
        .Where(f => f.Product.Name == "IntegrationProductHistoryFormat")
        .Select(f => f.Id)
        .SingleAsync(TestContext.Current.CancellationToken);
      int historyFormatId = await _context.ProductsHistory
        .AsNoTracking()
        .Where(h => h.Product.Name == "IntegrationProductHistoryFormat")
        .Select(h => h.ProductFormatId)
        .SingleAsync(
          TestContext.Current.CancellationToken);
      Assert.Equal(productFormatId, historyFormatId);
    }

    [Fact(
      DisplayName = "Uses provided registered_at for history entry")]
    public async Task Repository_UsesProvidedRegisteredAt_ForHistoryEntry() {
      // Arrange
      var registeredAt = new DateOnly(2024, 1, 15);
      var expectedCreatedAt = registeredAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

      // Act
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProduct3", 3.00m, 1, "L"),
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
      DisplayName = "Reuses existing product and adds new history entry")]
    public async Task Repository_ReusesExistingProduct_AddsNewHistoryEntry() {
      // Arrange
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProduct4", 1.00m, 1, "pc"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Act — add same product again with different price
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProduct4", 1.50m, 1, "pc"),
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

    [Fact(
      DisplayName = "Reuses product format when quantity and unit are unchanged")]
    public async Task Repository_ReusesProductFormat_WhenQuantityAndUnitAreUnchanged() {
      // Arrange
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProductFormatReuse", 1.00m, 1, "pc"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Act
      await _repository.AddMarketProductsAsync(
        MakeMarket("IntegrationProductFormatReuse", 1.50m, 1, "pc"),
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Assert
      List<int> formatIds = await _context.ProductsHistory
        .AsNoTracking()
        .Where(h => h.Product.Name == "IntegrationProductFormatReuse")
        .Select(h => h.ProductFormatId)
        .ToListAsync(TestContext.Current.CancellationToken);
      Assert.Single(formatIds.Distinct());
    }

    [Fact(
      DisplayName = "Creates distinct product formats for different quantity or unit")]
    public async Task Repository_CreatesDistinctFormats_ForDifferentQuantityOrUnit() {
      // Arrange
      var market = new Market(MarketName, [
        new MarketProduct(
          "IntegrationProduct5",
          new ProductBrand(BrandName),
          [
            MakeProductFormat(1.00m, 1, "kg"),
            MakeProductFormat(1.50m, 500, "g"),
          ])
      ]);

      // Act
      await _repository.AddMarketProductsAsync(
        market,
        DateOnly.FromDateTime(DateTime.Today),
        TestContext.Current.CancellationToken);

      // Assert
      List<(decimal Quantity, string Unit)> formats = await _context.ProductFormats
        .AsNoTracking()
        .Where(f => f.Product.Name == "IntegrationProduct5")
        .Include(f => f.UnitOfMeasure)
        .OrderBy(f => f.Quantity)
        .Select(f => new ValueTuple<decimal, string>(f.Quantity, f.UnitOfMeasure.Code))
        .ToListAsync(TestContext.Current.CancellationToken);

      Assert.Equal([(1m, "kg"), (500m, "g")], formats);
    }
  }

  [Collection("Database")]
  public class AddMarketProductsTests : IAsyncLifetime {
    private readonly DatabaseFixture _fixture;
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;
    private const string MarketName = "TestMarketForReturns";
    private const string BrandName = "TestBrandForReturns";

    public AddMarketProductsTests(DatabaseFixture fixture) {
      _fixture = fixture;
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public async ValueTask InitializeAsync() {
      await _fixture.DeleteShoppingProductReferencesAsync(TestContext.Current.CancellationToken);
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

      await EnsureUnitOfMeasureAsync(_context, "kg");
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
          [MakeProductFormat()]
        ))
      ]);

    [Fact(
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
    private readonly DatabaseFixture _fixture;
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public DeleteBrandsTests(DatabaseFixture fixture) {
      _fixture = fixture;
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public async ValueTask InitializeAsync() {
      await _fixture.DeleteShoppingProductReferencesAsync(TestContext.Current.CancellationToken);
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.ProductBrands.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
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
    private readonly DatabaseFixture _fixture;
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public DeleteMarketsTests(DatabaseFixture fixture) {
      _fixture = fixture;
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public async ValueTask InitializeAsync() {
      await _fixture.DeleteShoppingProductReferencesAsync(TestContext.Current.CancellationToken);
      await _context.ProductsHistory.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.Products.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.SuperMarkets.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
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
    private readonly DatabaseFixture _fixture;
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;
    private const string MarketName = "TestMarketForDeleting";
    private const string BrandName = "TestBrandForDeleting";

    public DeleteProductsTests(DatabaseFixture fixture) {
      _fixture = fixture;
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public async ValueTask InitializeAsync() {
      await _fixture.DeleteShoppingProductReferencesAsync(TestContext.Current.CancellationToken);
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

      await EnsureUnitOfMeasureAsync(_context, "kg");
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
          [MakeProductFormat()]
        )
      ]);

    [Fact(
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
    private readonly DatabaseFixture _fixture;
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;
    private const string MarketA = "GetProductsMarketA";
    private const string MarketB = "GetProductsMarketB";
    private const string BrandA = "GetProductsBrandA";
    private const string BrandB = "GetProductsBrandB";

    public GetProductsAsync(DatabaseFixture fixture) {
      _fixture = fixture;
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public async ValueTask InitializeAsync() {
      await _fixture.DeleteShoppingProductReferencesAsync(TestContext.Current.CancellationToken);
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

      await EnsureUnitOfMeasureAsync(_context, "L");
      await EnsureUnitOfMeasureAsync(_context, "g");
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task SeedProductWithHistoryAsync(
      string marketName, string brandName, string productName,
      decimal price, float quantity, string unitOfMeasure, DateTime createdAt
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

      int unitOfMeasureId = await _context.UnitsOfMeasure
        .Where(u => u.Code == unitOfMeasure)
        .Select(u => u.Id)
        .SingleAsync(TestContext.Current.CancellationToken);
      decimal quantityValue = (decimal)quantity;

      ProductFormatDbEntity? productFormat = await _context.ProductFormats
        .FirstOrDefaultAsync(
          f => f.ProductId == existing.Id &&
            f.Quantity == quantityValue &&
            f.UnitOfMeasureId == unitOfMeasureId,
          TestContext.Current.CancellationToken);
      if (productFormat is null) {
        productFormat = new ProductFormatDbEntity {
          ProductId = existing.Id,
          Quantity = quantityValue,
          UnitOfMeasureId = unitOfMeasureId,
          ImageUrl = "https://example.com/product.png"
        };
        _context.ProductFormats.Add(productFormat);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      }

      _context.ProductsHistory.Add(new ProductsHistoryDbEntity {
        ProductId = existing.Id,
        ProductFormatId = productFormat.Id,
        Price = price,
        CreatedAt = createdAt,
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static GetMarketProductsFilter Filter(
      string? market = null, string? brandSegment = null, string? segment = null,
      Pagination? pagination = null
    ) => new(market, brandSegment, segment, pagination);

    private static GetMarketProductsFilter FilterWithPage(
      string? market = null, string? brandSegment = null, string? segment = null,
      int page = 1, int pageSize = 100
    ) => new(market, brandSegment, segment, new Pagination(page, pageSize));

    [Fact(
      DisplayName = "Returns empty values when no products exist")]
    public async Task Repository_ReturnsEmptyValues_WhenNoProductsExist() {
      // Arrange & Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(), TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result.Values);
    }

    [Fact(
      DisplayName = "Returns zero total count when no products exist")]
    public async Task Repository_ReturnsZeroTotalCount_WhenNoProductsExist() {
      // Arrange & Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(), TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(0, result.TotalCount);
    }

    [Fact(
      DisplayName = "Returns products without filter")]
    public async Task Repository_ReturnsProducts_WithoutFilter() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche Entera GP", 0.89m, 1, "L", now);
      await SeedProductWithHistoryAsync(MarketB, BrandB, "Pan Blanco GP", 1.20m, 500, "g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(), TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result.Values.SelectMany(m => m.Products).Count() >= 2);
    }

    [Fact(
      DisplayName = "Returns total count without filter")]
    public async Task Repository_ReturnsTotalCount_WithoutFilter() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche Entera TCF", 0.89m, 1, "L", now);
      await SeedProductWithHistoryAsync(MarketB, BrandB, "Pan Blanco TCF", 1.20m, 500, "g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(), TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result.TotalCount >= 2);
    }

    [Fact(
      DisplayName = "Groups products under their market")]
    public async Task Repository_GroupsProducts_UnderTheirMarket() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche GroupTest", 0.89m, 1, "L", now);
      await SeedProductWithHistoryAsync(MarketB, BrandB, "Pan GroupTest", 1.20m, 500, "g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(), TestContext.Current.CancellationToken);

      // Assert
      Market marketA = result.Values.Single(m => m.Name == MarketA);
      Assert.Contains(marketA.Products, p => p.Name == "Leche GroupTest");
    }

    [Fact(
      DisplayName = "Filters by market name returns only matching market (case-insensitive)")]
    public async Task Repository_FiltersByMarketName_ReturnsOnlyMatchingMarket() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche MarketFilter", 0.89m, 1, "L", now);
      await SeedProductWithHistoryAsync(MarketB, BrandB, "Pan MarketFilter", 1.20m, 500, "g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(market: MarketA.ToUpperInvariant()), TestContext.Current.CancellationToken);

      // Assert
      Assert.Single(result.Values);
    }

    [Fact(
      DisplayName = "Filters by market name returns correct market name (case-insensitive)")]
    public async Task Repository_FiltersByMarketName_ReturnsCorrectMarketName() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche MarketNameFilter", 0.89m, 1, "L", now);
      await SeedProductWithHistoryAsync(MarketB, BrandB, "Pan MarketNameFilter", 1.20m, 500, "g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(market: MarketA.ToUpperInvariant()), TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(MarketA, result.Values.Single().Name);
    }

    [Fact(
      DisplayName = "Filters by brand name segment (case-insensitive contains)")]
    public async Task Repository_FiltersByBrandNameSegment_CaseInsensitiveContains() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche", 0.89m, 1, "L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandB, "Pan", 1.20m, 500, "g", now);

      // Act — use a segment that partially matches one brand
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(brandSegment: "BrandA"), TestContext.Current.CancellationToken);

      // Assert
      IEnumerable<MarketProduct> products = result.Values.SelectMany(m => m.Products);
      Assert.Equal(BrandA, products.Single().Brand.Name);
    }

    [Fact(
      DisplayName = "Filters by brand name segment (case-insensitive partial match)")]
    public async Task Repository_FiltersByBrandNameSegment_CaseInsensitivePartialMatch() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche BrandFilter", 0.89m, 1, "L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandB, "Pan BrandFilter", 1.20m, 500, "g", now);

      // Act — use a segment that only matches BrandA
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(brandSegment: BrandA.ToUpperInvariant()), TestContext.Current.CancellationToken);

      // Assert
      IEnumerable<MarketProduct> products = result.Values.SelectMany(m => m.Products);
      Assert.All(products, p => Assert.Equal(BrandA, p.Brand.Name));
    }

    [Fact(
      DisplayName = "Filters by name segment (case-insensitive contains)")]
    public async Task Repository_FiltersByNameSegment_CaseInsensitiveContains() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche Entera SegFilter", 0.89m, 1, "L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Pan Blanco SegFilter", 1.20m, 500, "g", now);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(segment: "leche entera segfilter"), TestContext.Current.CancellationToken);

      // Assert
      MarketProduct product = result.Values.SelectMany(m => m.Products).Single();
      Assert.Equal("Leche Entera SegFilter", product.Name);
    }

    [Fact(
      DisplayName = "Returns only one format from the latest history date per product")]
    public async Task Repository_ReturnsOnlyOneFormat_FromLatestHistoryDate() {
      // Arrange
      DateTime older = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      DateTime newer = new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche HistoryTest", 0.79m, 1, "L", older);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche HistoryTest", 0.89m, 1, "L", newer);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(segment: "Leche HistoryTest"), TestContext.Current.CancellationToken);

      // Assert
      MarketProduct product = result.Values.SelectMany(m => m.Products).Single();
      Assert.Single(product.Formats);
    }

    [Fact(
      DisplayName = "Returns format with price from the latest history date per product")]
    public async Task Repository_ReturnsLatestPrice_PerProduct() {
      // Arrange
      DateTime older = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      DateTime newer = new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche LatestPriceTest", 0.79m, 1, "L", older);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche LatestPriceTest", 0.89m, 1, "L", newer);

      // Act
      PagedResult<Market> result = await _repository.GetProductsAsync(
        Filter(segment: "Leche LatestPriceTest"), TestContext.Current.CancellationToken);

      // Assert
      MarketProduct product = result.Values.SelectMany(m => m.Products).Single();
      Assert.Equal(0.89m, product.Formats.Single().Price.Value, precision: 2);
    }

    [Fact(
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
      DisplayName = "Respects pagination — total count reflects all matching products")]
    public async Task Repository_Pagination_TotalCountReflectsAllMatches() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche Page1", 1.00m, 1, "L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche Page2", 1.10m, 1, "L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche Page3", 1.20m, 1, "L", now);

      // Act — page 2 with pageSize 1, filtered by segment "Leche Page"
      PagedResult<Market> result = await _repository.GetProductsAsync(
        FilterWithPage(segment: "Leche Page", page: 2, pageSize: 1),
        TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(3, result.TotalCount);
    }

    [Fact(
      DisplayName = "Respects pagination — returns single product on page 2 with page size 1")]
    public async Task Repository_Pagination_ReturnsSingleProductOnSecondPage() {
      // Arrange
      DateTime now = DateTime.UtcNow;
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche PgB1", 1.00m, 1, "L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche PgB2", 1.10m, 1, "L", now);
      await SeedProductWithHistoryAsync(MarketA, BrandA, "Leche PgB3", 1.20m, 1, "L", now);

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
    private readonly DatabaseFixture _fixture;
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;
    private const string MarketName = "TestMarketForHistory";
    private const string OtherMarketName = "OtherMarketForHistory";
    private const string BrandName = "TestBrandForHistory";

    public DeleteProductsHistoryForMarketsTests(DatabaseFixture fixture) {
      _fixture = fixture;
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public async ValueTask InitializeAsync() {
      await _fixture.DeleteShoppingProductReferencesAsync(TestContext.Current.CancellationToken);
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

      await EnsureUnitOfMeasureAsync(_context, "kg");
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
          [MakeProductFormat()]
        )
      ]);

    [Fact(
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

  [Collection("Database")]
  public class CheckUnitOfMeasureIsSupportedAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public CheckUnitOfMeasureIsSupportedAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task SeedUnitOfMeasureAsync(string code, string name) {
      bool exists = await _context.UnitsOfMeasure
        .AnyAsync(u => u.Code == code, TestContext.Current.CancellationToken);
      if (exists) {
        return;
      }

      _context.UnitsOfMeasure.Add(new UnitOfMeasureDbEntity {
        Code = code,
        Name = name
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact(
      DisplayName = "Returns true when unit code exists")]
    public async Task Repository_ReturnsTrue_WhenUnitCodeExists() {
      // Arrange
      await SeedUnitOfMeasureAsync("repo_uom", "Repository test unit");

      // Act
      bool result = await _repository.CheckUnitOfMeasureIsSupportedAsync(
        "repo_uom",
        TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }

    [Fact(
      DisplayName = "Returns false when unit code does not exist")]
    public async Task Repository_ReturnsFalse_WhenUnitCodeDoesNotExist() {
      // Act
      bool result = await _repository.CheckUnitOfMeasureIsSupportedAsync(
        "missing_uom",
        TestContext.Current.CancellationToken);

      // Assert
      Assert.False(result);
    }

    [Fact(
      DisplayName = "Matches unit code ignoring case")]
    public async Task Repository_MatchesUnitCode_IgnoringCase() {
      // Arrange
      await SeedUnitOfMeasureAsync("case_uom", "Case-insensitive repository test unit");

      // Act
      bool result = await _repository.CheckUnitOfMeasureIsSupportedAsync(
        "CASE_UOM",
        TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }
  }

  [Collection("Database")]
  public class GetMarketSummariesAsync : IAsyncLifetime {
    private readonly DatabaseFixture _fixture;
    private readonly MainContext _context;
    private readonly PostgreSqlMarketRepository _repository;

    public GetMarketSummariesAsync(DatabaseFixture fixture) {
      _fixture = fixture;
      _context = fixture.CreateContext();
      _repository = new PostgreSqlMarketRepository(
        _context);
    }

    public async ValueTask InitializeAsync() {
      await _fixture.DeleteShoppingProductReferencesAsync(TestContext.Current.CancellationToken);
      await _context.SuperMarkets.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      DisplayName = "Returns empty list when no markets exist")]
    public async Task Repository_ReturnsEmptyList_WhenNoMarketsExist() {
      // Act
      List<MarketSummary> result =
        await _repository.GetMarketSummariesAsync(TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result);
    }

    [Fact(
      DisplayName = "Returns all markets")]
    public async Task Repository_ReturnsAllMarkets() {
      // Arrange
      _context.SuperMarkets.AddRange(
        new SuperMarketDbEntity { Name = "Mercadona" },
        new SuperMarketDbEntity { Name = "Alcampo" });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<MarketSummary> result =
        await _repository.GetMarketSummariesAsync(TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(2, result.Count);
    }

    [Fact(
      DisplayName = "Maps name correctly")]
    public async Task Repository_MapsName_Correctly() {
      // Arrange
      _context.SuperMarkets.Add(new SuperMarketDbEntity { Name = "Lidl" });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<MarketSummary> result =
        await _repository.GetMarketSummariesAsync(TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal("Lidl", result.Single().Name);
    }

    [Fact(
      DisplayName = "Maps logo_url when set")]
    public async Task Repository_MapsLogoUrl_WhenSet() {
      // Arrange
      _context.SuperMarkets.Add(new SuperMarketDbEntity {
        Name = "Carrefour",
        LogoUrl = "https://example.com/carrefour.png"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<MarketSummary> result =
        await _repository.GetMarketSummariesAsync(TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(new Uri("https://example.com/carrefour.png"), result.Single().LogoUrl);
    }

    [Fact(
      DisplayName = "Maps null logo_url when not set")]
    public async Task Repository_MapsNullLogoUrl_WhenNotSet() {
      // Arrange
      _context.SuperMarkets.Add(new SuperMarketDbEntity { Name = "Dia" });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<MarketSummary> result =
        await _repository.GetMarketSummariesAsync(TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result.Single().LogoUrl);
    }
  }
}



