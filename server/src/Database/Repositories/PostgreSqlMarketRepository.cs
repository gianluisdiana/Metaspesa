using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Database.Entities;
using Metaspesa.Domain.Markets;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.Repositories;

internal static class IQueryableExtensions {
  public static IQueryable<ProductDbEntity> ApplyFilter(
    this IQueryable<ProductDbEntity> query, GetMarketProductsFilter filter
  ) {
    if (filter.MarketName is not null) {
      query = query.Where(p => EF.Functions.ILike(p.SuperMarket.Name, filter.MarketName));
    }
    if (filter.BrandNameSegment is not null) {
      query = query.Where(p => EF.Functions.ILike(
        p.Brand.Name, $"%{filter.BrandNameSegment}%"));
    }
    if (filter.NameSegment is not null) {
      query = query.Where(p => EF.Functions.ILike(
        p.Name, $"%{filter.NameSegment}%"));
    }

    return query;
  }
}

internal partial class PostgreSqlMarketRepository(
  MainContext context
) : IMarketRepository {
  private const int BatchSize = 1_000;

  private readonly record struct ProductSource(
    int ProductId, MarketProduct Source);
  private readonly record struct ProductInsert(
    ProductDbEntity Entity, MarketProduct Source);
  private readonly record struct ProductFormatKey(
    int ProductId,
    decimal Quantity,
    int UnitOfMeasureId
  );

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
    IQueryable<ProductDbEntity> query = context.Products
      .Include(p => p.Brand)
      .Include(p => p.SuperMarket)
      .Where(p => p.History.Any())
      .ApplyFilter(filter);

    int totalCount = await query.CountAsync(cancellationToken);
    List<ProductDbEntity> entities = await LoadProductPageAsync(
      query, filter, cancellationToken);

    return new PagedResult<Market>(MapMarkets(entities), totalCount);
  }, "Couldn't get market products.");

  private static async Task<List<ProductDbEntity>> LoadProductPageAsync(
    IQueryable<ProductDbEntity> query,
    GetMarketProductsFilter filter,
    CancellationToken cancellationToken
  ) {
    IQueryable<ProductDbEntity> orderedQuery = query
      .Include(p => p.History)
      .ThenInclude(h => h.ProductFormat)
      .ThenInclude(f => f.UnitOfMeasure)
      .OrderBy(p => p.SuperMarket.Name)
      .ThenBy(p => p.Name);

    if (filter.Pagination is null || filter.Pagination.IsInfinite) {
      return await orderedQuery.ToListAsync(cancellationToken);
    }

    return await orderedQuery
      .Skip(filter.Pagination.Skip)
      .Take(filter.Pagination.Size)
      .ToListAsync(cancellationToken);
  }

  private static IReadOnlyCollection<Market> MapMarkets(
    IEnumerable<ProductDbEntity> products
  ) => [.. products
    .GroupBy(p => p.SuperMarket.Name)
    .Select(g => new Market(g.Key, [.. g.Select(p => p.MapToDomain())]))];

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

    int superMarketId = await GetSuperMarketIdAsync(market.Name, cancellationToken);

    Dictionary<string, int> brandLookup = await GetBrandLookupAsync(
      market.Products, cancellationToken);
    Dictionary<(string Name, int BrandId), int> existingProducts =
      await GetExistingProductLookupAsync(superMarketId, cancellationToken);

    (List<ProductSource> productSources, List<int> addedProductIds) =
      await ResolveProductSourcesAsync(
        market.Products,
        superMarketId,
        brandLookup,
        existingProducts,
        cancellationToken);

    Dictionary<string, int> unitLookup = await GetUnitLookupAsync(cancellationToken);
    Dictionary<ProductFormatKey, int> formatLookup = await ResolveProductFormatsAsync(
      productSources, unitLookup, cancellationToken);

    var now = registeredAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    List<ProductsHistoryDbEntity> history = BuildProductHistory(
      productSources, unitLookup, formatLookup, now);
    await AddProductHistoryAsync(history, cancellationToken);

    return addedProductIds;
  }, "Couldn't add market products.");

  private async Task<int> GetSuperMarketIdAsync(
    string marketName,
    CancellationToken cancellationToken
  ) => await context.SuperMarkets
    .Where(s => EF.Functions.ILike(s.Name, marketName))
    .Select(s => s.Id)
    .SingleAsync(cancellationToken);

  private async Task<Dictionary<string, int>> GetBrandLookupAsync(
    IReadOnlyCollection<MarketProduct> products,
    CancellationToken cancellationToken
  ) {
    List<string> brandNames = [.. products.Select(p => p.Brand.Name).Distinct()];

    return await context.ProductBrands
      .Where(b => brandNames.Contains(b.Name))
      .ToDictionaryAsync(b => b.Name, b => b.Id, cancellationToken);
  }

  private async Task<Dictionary<(string Name, int BrandId), int>> GetExistingProductLookupAsync(
    int superMarketId,
    CancellationToken cancellationToken
  ) => await context.Products
    .Where(p => p.SuperMarketId == superMarketId)
    .ToDictionaryAsync(p => (p.Name, p.BrandId), p => p.Id, cancellationToken);

  private async Task<(List<ProductSource> Sources, List<int> AddedProductIds)> ResolveProductSourcesAsync(
    IReadOnlyCollection<MarketProduct> products,
    int superMarketId,
    Dictionary<string, int> brandLookup,
    Dictionary<(string Name, int BrandId), int> existingProducts,
    CancellationToken cancellationToken
  ) {
    var toInsert = new List<ProductInsert>();
    var sources = new List<ProductSource>();

    foreach (MarketProduct product in products) {
      int brandId = brandLookup[product.Brand.Name];

      if (existingProducts.TryGetValue((product.Name, brandId), out int existingId)) {
        sources.Add(new ProductSource(existingId, product));
        continue;
      }

      toInsert.Add(new ProductInsert(
        new ProductDbEntity {
          Name = product.Name,
          SuperMarketId = superMarketId,
          BrandId = brandId,
        },
        product));
    }

    List<ProductSource> newSources = await AddProductsAsync(toInsert, cancellationToken);
    sources.AddRange(newSources);

    return (sources, newSources.Select(s => s.ProductId).ToList());
  }

  private async Task<List<ProductSource>> AddProductsAsync(
    List<ProductInsert> toInsert,
    CancellationToken cancellationToken
  ) {
    IEnumerable<List<ProductInsert>> batches = toInsert.Chunk(BatchSize)
      .Select(x => x.ToList());

    List<ProductSource> sources = [];
    foreach (List<ProductInsert> batch in batches) {
      await context.Products.AddRangeAsync(batch.Select(x => x.Entity), cancellationToken);
      await context.SaveChangesAsync(cancellationToken);
      context.ChangeTracker.Clear();
      sources.AddRange(batch.Select(x => new ProductSource(x.Entity.Id, x.Source)));
    }

    return sources;
  }

  private async Task<Dictionary<string, int>> GetUnitLookupAsync(
    CancellationToken cancellationToken
  ) => await context.UnitsOfMeasure
      .AsNoTracking()
      .ToDictionaryAsync(u => u.Code, u => u.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

  private async Task<Dictionary<ProductFormatKey, int>> ResolveProductFormatsAsync(
    IReadOnlyCollection<ProductSource> productSources,
    IReadOnlyDictionary<string, int> unitLookup,
    CancellationToken cancellationToken
  ) {
    Dictionary<ProductFormatKey, int> formatLookup =
      await GetExistingProductFormatLookupAsync(productSources, cancellationToken);

    List<ProductFormatDbEntity> missingFormats = BuildMissingProductFormats(
      productSources, unitLookup, formatLookup);

    IDictionary<ProductFormatKey, int> newFormats = await AddProductFormatsAsync(
      missingFormats, cancellationToken);

    return formatLookup.Concat(newFormats)
      .ToDictionary(kv => kv.Key, kv => kv.Value);
  }

  private async Task<Dictionary<ProductFormatKey, int>> GetExistingProductFormatLookupAsync(
    IReadOnlyCollection<ProductSource> productSources,
    CancellationToken cancellationToken
  ) {
    List<int> productIds = [.. productSources.Select(s => s.ProductId).Distinct()];

    return await context.ProductFormats
      .Where(f => productIds.Contains(f.ProductId))
      .Select(f => new { f.Id, f.ProductId, f.Quantity, f.UnitOfMeasureId })
      .ToDictionaryAsync(
        f => new ProductFormatKey(f.ProductId, f.Quantity, f.UnitOfMeasureId),
        f => f.Id,
        cancellationToken);
  }

  private static List<ProductFormatDbEntity> BuildMissingProductFormats(
    IEnumerable<ProductSource> productSources,
    IReadOnlyDictionary<string, int> unitLookup,
    IReadOnlyDictionary<ProductFormatKey, int> existingFormatLookup
  ) {
    var missingFormats = new Dictionary<ProductFormatKey, ProductFormatDbEntity>();

    foreach (ProductSource productSource in productSources) {
      foreach (ProductFormat format in productSource.Source.Formats) {
        ProductFormatKey key = ToProductFormatKey(productSource.ProductId, format, unitLookup);
        if (existingFormatLookup.ContainsKey(key) || missingFormats.ContainsKey(key)) {
          continue;
        }

        missingFormats[key] = new ProductFormatDbEntity {
          ProductId = key.ProductId,
          Quantity = key.Quantity,
          UnitOfMeasureId = key.UnitOfMeasureId,
          ImageUrl = format.ImageUrl?.ToString() ?? string.Empty
        };
      }
    }

    return [.. missingFormats.Values];
  }

  private async Task<IDictionary<ProductFormatKey, int>> AddProductFormatsAsync(
    List<ProductFormatDbEntity> formats,
    CancellationToken cancellationToken
  ) {
    var newFormatLookup = new Dictionary<ProductFormatKey, int>();
    IEnumerable<List<ProductFormatDbEntity>> batches = formats
      .Chunk(BatchSize)
      .Select(x => x.ToList());
    foreach (List<ProductFormatDbEntity> batch in batches) {
      await context.ProductFormats.AddRangeAsync(batch, cancellationToken);
      await context.SaveChangesAsync(cancellationToken);
      context.ChangeTracker.Clear();

      foreach (ProductFormatDbEntity format in batch) {
        var key = new ProductFormatKey(
          format.ProductId,
          format.Quantity,
          format.UnitOfMeasureId);
        newFormatLookup[key] = format.Id;
      }
    }

    return newFormatLookup;
  }

  private static List<ProductsHistoryDbEntity> BuildProductHistory(
    IEnumerable<ProductSource> productSources,
    IReadOnlyDictionary<string, int> unitLookup,
    IReadOnlyDictionary<ProductFormatKey, int> formatLookup,
    DateTime createdAt
  ) => [.. productSources.SelectMany(productSource =>
    productSource.Source.Formats.Select(format => {
      ProductFormatKey key = ToProductFormatKey(
        productSource.ProductId, format, unitLookup);

      return new ProductsHistoryDbEntity {
        ProductId = productSource.ProductId,
        ProductFormatId = formatLookup[key],
        Price = format.Price.Value,
        CreatedAt = createdAt
      };
    }))];

  private static ProductFormatKey ToProductFormatKey(
    int productId,
    ProductFormat format,
    IReadOnlyDictionary<string, int> unitLookup
  ) => new(
    productId,
    (decimal)format.Quantity.Value,
    unitLookup[format.Quantity.UnitOfMeasure.Trim()]);

  private async Task AddProductHistoryAsync(
    List<ProductsHistoryDbEntity> history,
    CancellationToken cancellationToken
  ) {
    foreach (ProductsHistoryDbEntity[] batch in history.Chunk(BatchSize)) {
      await context.ProductsHistory.AddRangeAsync(batch, cancellationToken);
      await context.SaveChangesAsync(cancellationToken);
      context.ChangeTracker.Clear();
    }
  }

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

  public Task<bool> CheckUnitOfMeasureIsSupportedAsync(
    string unitOfMeasure, CancellationToken cancellationToken
  ) => PostgreSqlExceptionMapper.MapAsync(async () =>
    await context.UnitsOfMeasure.AnyAsync(
      u => EF.Functions.ILike(u.Code, unitOfMeasure),
      cancellationToken),
    "Couldn't check unit of measure.");
}