using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Database.Entities;
using Metaspesa.Database.Repositories;
using Metaspesa.Domain.Markets;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.IntegrationTests.Markets;

[Collection("Database")]
public class PostgreSqlMarketRepositoryTest : IAsyncLifetime {
  private readonly DatabaseFixture _fixture;
  private readonly MainContext _context;
  private readonly PostgreSqlMarketRepository _marketRepository;

  public PostgreSqlMarketRepositoryTest(DatabaseFixture fixture) {
    _fixture = fixture;
    _context = fixture.CreateContext();
    _marketRepository = new PostgreSqlMarketRepository(_context);
  }

  public async ValueTask InitializeAsync() {
    CancellationToken cancellationToken = TestContext.Current.CancellationToken;
    await _fixture.DeleteShoppingProductReferencesAsync(cancellationToken);
    await _context.PriceSnapshots.ExecuteDeleteAsync(cancellationToken);
    await _context.ProductFormats.ExecuteDeleteAsync(cancellationToken);
    await _context.Products.ExecuteDeleteAsync(cancellationToken);
    await _context.ProductBrands.ExecuteDeleteAsync(cancellationToken);
    await _context.SuperMarkets.ExecuteDeleteAsync(cancellationToken);
    await EnsureUnitOfMeasureAsync("l");
    await EnsureUnitOfMeasureAsync("kg");
  }

  public async ValueTask DisposeAsync() {
    await _context.DisposeAsync();
    GC.SuppressFinalize(this);
  }

  [Fact(DisplayName = "Persists and reconstructs market aggregate")]
  public async Task Repository_PersistsAndLoads_MarketAggregate() {
    await _marketRepository.AddMarketsAsync(
      [new MarketName("Mercadona")],
      TestContext.Current.CancellationToken);

    List<Market> markets = await _marketRepository.GetMarketsAsync(
      TestContext.Current.CancellationToken);

    Market market = Assert.Single(markets);
    Assert.NotEqual(Guid.Empty, market.Id.Value);
    Assert.Equal(new MarketName("Mercadona"), market.Name);
    Assert.Null(market.LogoUrl);
  }

  [Fact(DisplayName = "Reports supported unit ignoring case")]
  public async Task Repository_ChecksSupportedUnit_IgnoringCase() {
    bool supported =
      await _marketRepository.CheckUnitOfMeasureIsSupportedAsync(
      "L", TestContext.Current.CancellationToken);

    Assert.True(supported);
  }

  [Fact(DisplayName = "Rejects unsupported unit code")]
  public async Task Repository_ReturnsFalse_ForUnsupportedUnit() {
    bool supported = await _marketRepository.CheckUnitOfMeasureIsSupportedAsync(
      "missing-unit", TestContext.Current.CancellationToken);

    Assert.False(supported);
  }

  [Fact(DisplayName = "Maps market logo in summaries")]
  public async Task Repository_MapsLogo_InMarketSummary() {
    _context.SuperMarkets.Add(new SuperMarketDbEntity {
      Id = Guid.CreateVersion7(),
      Name = "Mercadona",
      LogoUrl = "https://example.test/mercadona.png",
    });
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

    IReadOnlyCollection<MarketSummary> summaries =
      await _marketRepository.GetMarketSummariesAsync(
        TestContext.Current.CancellationToken);

    MarketSummary summary = Assert.Single(summaries);
    Assert.NotEqual(Guid.Empty, summary.Id);
    Assert.Equal(new Uri("https://example.test/mercadona.png"), summary.LogoUrl);
  }

  [Fact(DisplayName = "Deletes only requested market")]
  public async Task Repository_DeleteMarketsAsync_PreservesOtherMarkets() {
    await _marketRepository.AddMarketsAsync(
      [new MarketName("Delete me"), new MarketName("Keep me")],
      TestContext.Current.CancellationToken);

    await _marketRepository.DeleteMarketsAsync(
      [new MarketName("Delete me")],
      TestContext.Current.CancellationToken);

    List<Market> markets = await _marketRepository.GetMarketsAsync(
      TestContext.Current.CancellationToken);
    Assert.Equal(new MarketName("Keep me"), Assert.Single(markets).Name);
  }

  private async Task EnsureUnitOfMeasureAsync(string code) {
    bool exists = await _context.UnitsOfMeasure.AnyAsync(
      unit => unit.Code == code,
      TestContext.Current.CancellationToken);
    if (exists) {
      return;
    }

    _context.UnitsOfMeasure.Add(new UnitOfMeasureDbEntity {
      Id = Guid.CreateVersion7(),
      Code = code,
      Name = $"Test unit {code}",
    });
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
  }
}