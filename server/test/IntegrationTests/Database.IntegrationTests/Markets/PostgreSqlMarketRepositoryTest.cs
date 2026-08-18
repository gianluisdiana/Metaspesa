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
    Assert.True(market.Id.Value > 0);
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

  private async Task EnsureUnitOfMeasureAsync(string code) {
    bool exists = await _context.UnitsOfMeasure.AnyAsync(
      unit => unit.Code == code,
      TestContext.Current.CancellationToken);
    if (exists) {
      return;
    }

    _context.UnitsOfMeasure.Add(new UnitOfMeasureDbEntity {
      Code = code,
      Name = $"Test unit {code}",
    });
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
  }
}
