using Metaspesa.Database.Entities;
using Metaspesa.Database.Repositories;
using Metaspesa.Domain.Markets;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.IntegrationTests.Purchasing;

[Collection("Database")]
public class PostgreSqlPurchasePriceSnapshotReaderTests : IAsyncLifetime {
  private readonly DatabaseFixture _fixture;
  private readonly MainContext _context;
  private readonly PostgreSqlPurchasePriceSnapshotReader _reader;

  public PostgreSqlPurchasePriceSnapshotReaderTests(DatabaseFixture fixture) {
    _fixture = fixture;
    _context = fixture.CreateContext();
    _reader = new PostgreSqlPurchasePriceSnapshotReader(_context);
  }

  public async ValueTask InitializeAsync() {
    CancellationToken cancellationToken = TestContext.Current.CancellationToken;
    await _fixture.DeleteShoppingProductReferencesAsync(cancellationToken);
    await _context.PriceSnapshots.ExecuteDeleteAsync(cancellationToken);
    await _context.ProductFormats.ExecuteDeleteAsync(cancellationToken);
    await _context.Products.ExecuteDeleteAsync(cancellationToken);
    await _context.ProductBrands.ExecuteDeleteAsync(cancellationToken);
    await _context.SuperMarkets.ExecuteDeleteAsync(cancellationToken);
  }

  public async ValueTask DisposeAsync() {
    await _context.DisposeAsync();
    GC.SuppressFinalize(this);
  }

  [Fact(DisplayName = "Returns empty result for empty input")]
  public async Task GetLatestAsync_ReturnsEmpty_WhenInputIsEmpty() {
    IReadOnlyDictionary<ProductFormatId, PriceSnapshotId> result =
      await _reader.GetLatestAsync([], TestContext.Current.CancellationToken);

    Assert.Empty(result);
  }

  [Fact(DisplayName = "Returns latest snapshot for every requested format")]
  public async Task GetLatestAsync_ReturnsLatest_ForManyFormats() {
    ProductFormatId milkId = await SeedFormatAsync("Milk");
    ProductFormatId breadId = await SeedFormatAsync("Bread");
    PriceSnapshotId oldMilk = await SeedSnapshotAsync(
      milkId, UtcDate(2026, 8, 18), 1.25m);
    PriceSnapshotId latestMilk = await SeedSnapshotAsync(
      milkId, UtcDate(2026, 8, 19), 1.50m);
    PriceSnapshotId bread = await SeedSnapshotAsync(
      breadId, UtcDate(2026, 8, 18), 2.00m);

    IReadOnlyDictionary<ProductFormatId, PriceSnapshotId> result =
      await _reader.GetLatestAsync(
        [milkId, breadId], TestContext.Current.CancellationToken);

    Assert.Equal(latestMilk, result[milkId]);
    Assert.Equal(bread, result[breadId]);
    Assert.DoesNotContain(oldMilk, result.Values);
  }

  [Fact(DisplayName = "Breaks equal-time tie using highest snapshot ID")]
  public async Task GetLatestAsync_ReturnsHighestId_WhenTimesAreEqual() {
    ProductFormatId formatId = await SeedFormatAsync("Milk");
    DateTime observedAt = UtcDate(2026, 8, 19);
    PriceSnapshotId firstId = await SeedSnapshotAsync(
      formatId, observedAt, 1.25m);
    PriceSnapshotId secondId = await SeedSnapshotAsync(
      formatId, observedAt, 1.50m);
    PriceSnapshotId highestId = firstId.Value.CompareTo(secondId.Value) > 0
      ? firstId
      : secondId;

    IReadOnlyDictionary<ProductFormatId, PriceSnapshotId> result =
      await _reader.GetLatestAsync(
        [formatId], TestContext.Current.CancellationToken);

    Assert.Equal(highestId, result[formatId]);
  }

  [Fact(DisplayName = "Omits requested formats without snapshots")]
  public async Task GetLatestAsync_OmitsFormat_WhenSnapshotIsMissing() {
    ProductFormatId existingId = await SeedFormatAsync("Milk");
    ProductFormatId missingId = await SeedFormatAsync("Bread");
    await SeedSnapshotAsync(existingId, UtcDate(2026, 8, 19), 1.25m);

    IReadOnlyDictionary<ProductFormatId, PriceSnapshotId> result =
      await _reader.GetLatestAsync(
        [existingId, missingId], TestContext.Current.CancellationToken);

    Assert.True(result.ContainsKey(existingId));
    Assert.False(result.ContainsKey(missingId));
  }

  [Fact(DisplayName = "Propagates cancelled snapshot lookup")]
  public async Task GetLatestAsync_PropagatesCancellation() {
    ProductFormatId formatId = await SeedFormatAsync("Milk");
    using var source = new CancellationTokenSource();
    await source.CancelAsync();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
      _reader.GetLatestAsync([formatId], source.Token));
  }

  private async Task<ProductFormatId> SeedFormatAsync(string name) {
    var market = new SuperMarketDbEntity {
      Id = Guid.CreateVersion7(),
      Name = $"Market {Guid.CreateVersion7()}",
    };
    var brand = new ProductBrandDbEntity {
      Id = Guid.CreateVersion7(),
      Name = $"Brand {Guid.CreateVersion7()}",
    };
    var unit = new UnitOfMeasureDbEntity {
      Id = Guid.CreateVersion7(),
      Code = $"u{Guid.CreateVersion7():N}"[..16],
      Name = $"Unit {Guid.CreateVersion7()}",
    };
    var format = new ProductFormatDbEntity {
      Id = Guid.CreateVersion7(),
      Product = new ProductDbEntity {
        Id = Guid.CreateVersion7(),
        Name = name,
        SuperMarket = market,
        Brand = brand,
      },
      Quantity = 1,
      UnitOfMeasure = unit,
      ImageUrl = string.Empty,
    };
    _context.ProductFormats.Add(format);
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    return new ProductFormatId(format.Id);
  }

  private async Task<PriceSnapshotId> SeedSnapshotAsync(
    ProductFormatId formatId,
    DateTime observedAt,
    decimal price
  ) {
    var snapshot = new PriceSnapshotDbEntity {
      Id = Guid.CreateVersion7(),
      ProductFormatId = formatId.Value,
      PriceAmount = price,
      ObservedAt = observedAt,
    };
    _context.PriceSnapshots.Add(snapshot);
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    return new PriceSnapshotId(snapshot.Id);
  }

  private static DateTime UtcDate(int year, int month, int day) =>
    new(year, month, day, 0, 0, 0, DateTimeKind.Utc);
}