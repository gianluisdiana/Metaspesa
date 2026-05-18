using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Database.Entities;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.Repositories;

internal partial class PostgreSqlMarketRepository(
  MainContext context
) : IMarketRepository {
  public async Task<List<MarketSummary>> GetMarketSummariesAsync(
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => await context.SuperMarkets
      .Select(m => new MarketSummary(
        m.Name,
        m.LogoUrl == null ? null : new Uri(m.LogoUrl)
      ))
      .ToListAsync(cancellationToken),
    "Couldn't get market summaries.");

  public async Task<PagedResult<Market>> GetProductsAsync(
    GetMarketProductsFilter filter, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    IQueryable<ProductDbEntity> baseQuery = context.Products
      .Include(p => p.Brand)
      .Include(p => p.SuperMarket)
      .Where(p => p.History.Any());

    if (filter.MarketName is not null) {
      baseQuery = baseQuery.Where(p => EF.Functions.ILike(p.SuperMarket.Name, filter.MarketName));
    }
    if (filter.BrandNameSegment is not null) {
      baseQuery = baseQuery.Where(p => EF.Functions.ILike(p.Brand.Name, $"%{filter.BrandNameSegment}%"));
    }
    if (filter.NameSegment is not null) {
      baseQuery = baseQuery.Where(p => EF.Functions.ILike(p.Name, $"%{filter.NameSegment}%"));
    }

    int totalCount = await baseQuery.CountAsync(cancellationToken);

    IQueryable<ProductDbEntity> orderedQuery = baseQuery
      .Include(p => p.History)
      .OrderBy(p => p.SuperMarket.Name).ThenBy(p => p.Name);

    bool isInfinite = filter.Pagination is null || filter.Pagination.IsInfinite;

    List<ProductDbEntity> entities = isInfinite
      ? await orderedQuery.ToListAsync(cancellationToken)
      : await orderedQuery
          .Skip(filter.Pagination!.Skip)
          .Take(filter.Pagination.Size)
          .ToListAsync(cancellationToken);

    IReadOnlyCollection<Market> markets = [..entities
      .GroupBy(p => p.SuperMarket.Name)
      .Select(g => new Market(
        g.Key,
        [..g.Select(p => {
          DateTime latest = p.History.Max(h => h.CreatedAt);
          return new MarketProduct(
            Name: p.Name,
            Brand: new ProductBrand(p.Brand.Name),
            Formats: [..p.History
              .Where(h => h.CreatedAt == latest)
              .Select(h => new ProductFormat(
                Quantity: h.Quantity,
                Price: new Price(h.Price),
                ImageUrl: h.ImageUrl is null ? null : new Uri(h.ImageUrl, UriKind.Absolute)
              ))]);
        })]
      ))];

    return new PagedResult<Market>(markets, totalCount);
  }, "Couldn't get market products.");

  public async Task<List<Market>> GetMarketsAsync(
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => await context.SuperMarkets
      .Select(m => new Market(m.Name, new List<MarketProduct>()))
      .ToListAsync(cancellationToken),
    "Couldn't get markets.");

  public async Task AddMarketsAsync(
    IReadOnlyCollection<Market> markets, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    context.SuperMarkets.AddRange(
      markets.Select(m => new SuperMarketDbEntity { Name = m.Name }));
    await context.SaveChangesAsync(cancellationToken);
  }, "Couldn't add markets.");

  public async Task<List<ProductBrand>> GetBrandsAsync(
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => await context.ProductBrands
      .Select(b => new ProductBrand(b.Name))
      .ToListAsync(cancellationToken),
    "Couldn't get brands.");

  public async Task AddBrandsAsync(
    IReadOnlyCollection<ProductBrand> brands, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    context.ProductBrands.AddRange(
      brands.Select(b => new ProductBrandDbEntity { Name = b.Name }));
    await context.SaveChangesAsync(cancellationToken);
  }, "Couldn't add brands.");

  public async Task<IReadOnlyCollection<int>> AddMarketProductsAsync(
    Market market,
    DateOnly registeredAt,
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    context.ChangeTracker.AutoDetectChangesEnabled = false;
    context.ChangeTracker.Clear();

    int superMarketId = await context.SuperMarkets
      .Where(s => EF.Functions.ILike(s.Name, market.Name))
      .Select(s => s.Id)
      .SingleAsync(cancellationToken);

    var brandNamesInImport = market.Products
        .Select(p => p.Brand.Name)
        .Distinct()
        .ToList();

    Dictionary<string, int> brandLookup = await context.ProductBrands
        .Where(b => brandNamesInImport.Contains(b.Name))
        .ToDictionaryAsync(b => b.Name, b => b.Id, cancellationToken);

    Dictionary<(string Name, int BrandId), int> existingProducts = await context.Products
        .Where(p => p.SuperMarketId == superMarketId)
        .Select(p => new { p.Id, p.Name, p.BrandId })
        .ToDictionaryAsync(p => (p.Name, p.BrandId), p => p.Id, cancellationToken);

    var history = new List<ProductsHistoryDbEntity>();
    var toInsert = new List<(ProductDbEntity Entity, MarketProduct Source)>();
    var addedProductIds = new List<int>();

    var now = registeredAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    foreach (MarketProduct mp in market.Products) {
      int brandId = brandLookup[mp.Brand.Name];
      (string Name, int brandId) key = (mp.Name, brandId);

      if (existingProducts.TryGetValue(key, out int existingId)) {
        history.AddRange(mp.Formats.Select(f => new ProductsHistoryDbEntity {
          ProductId = existingId,
          Price = f.Price.Value,
          Quantity = f.Quantity,
          ImageUrl = f.ImageUrl?.ToString(),
          CreatedAt = now
        }));
      } else {
        toInsert.Add((new ProductDbEntity {
          Name = mp.Name,
          SuperMarketId = superMarketId,
          BrandId = brandId,
        }, mp));
      }
    }

    if (toInsert.Count > 0) {
      const int batchSize = 1_000;

      for (int i = 0; i < toInsert.Count; i += batchSize) {
        var batch = toInsert.Skip(i).Take(batchSize).ToList();

        await context.Products.AddRangeAsync(batch.Select(x => x.Entity), cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        context.ChangeTracker.Clear();

        addedProductIds.AddRange(batch.Select(x => x.Entity.Id));
        history.AddRange(batch.SelectMany(x => x.Source.Formats.Select(f => new ProductsHistoryDbEntity {
          ProductId = x.Entity.Id,
          Price = f.Price.Value,
          Quantity = f.Quantity,
          ImageUrl = f.ImageUrl?.ToString(),
          CreatedAt = now
        })));
      }
    }

    const int historyBatchSize = 2_000;
    for (int i = 0; i < history.Count; i += historyBatchSize) {
      await context.ProductsHistory.AddRangeAsync(
          history.Skip(i).Take(historyBatchSize), cancellationToken);
      await context.SaveChangesAsync(cancellationToken);
      context.ChangeTracker.Clear();
    }

    return addedProductIds;
  }, "Couldn't add market products.");

  public async Task DeleteProductsAsync(
    IReadOnlyCollection<int> productIds, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () =>
    await context.Products
      .Where(p => productIds.Contains(p.Id))
      .ExecuteDeleteAsync(cancellationToken),
    "Couldn't delete products.");

  public async Task DeleteProductsHistoryForMarketsAsync(
    IReadOnlyCollection<string> marketNames,
    DateOnly registeredAt,
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    var date = registeredAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    IQueryable<ProductsHistoryDbEntity> productsHistoryDbEntities =
      from h in context.ProductsHistory
      join p in context.Products
        on h.ProductId equals p.Id
      join sm in context.SuperMarkets
        on p.SuperMarketId equals sm.Id
      where marketNames.Contains(sm.Name) &&
        h.CreatedAt == date
      select h;

    await productsHistoryDbEntities
      .ExecuteDeleteAsync(cancellationToken);
  }, "Couldn't delete product history.");

  public async Task DeleteMarketsAsync(
    IReadOnlyCollection<string> marketNames, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () =>
    await context.SuperMarkets
      .Where(m => marketNames.Contains(m.Name))
      .ExecuteDeleteAsync(cancellationToken),
    "Couldn't delete markets.");

  public async Task DeleteBrandsAsync(
    IReadOnlyCollection<string> brandNames, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () =>
    await context.ProductBrands
      .Where(b => brandNames.Contains(b.Name))
      .ExecuteDeleteAsync(cancellationToken),
    "Couldn't delete brands.");
}
