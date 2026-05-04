using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Database.Entities;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metaspesa.Database.Repositories;

internal partial class PostgreSqlMarketRepository(
  MainContext context,
  ILogger<PostgreSqlMarketRepository> logger
) : IMarketRepository {
  public async Task<List<MarketSummary>> GetMarketSummariesAsync(
    CancellationToken cancellationToken
  ) {
    try {
      List<MarketSummary> marketSummaries = await context.SuperMarkets
        .Select(m => new MarketSummary(
          m.Name,
          m.LogoUrl == null ? null : new Uri(m.LogoUrl)
        ))
        .ToListAsync(cancellationToken);

      return marketSummaries;
    } catch (Exception ex) when (
      ex is NpgsqlException or OperationCanceledException ||
      ex.InnerException is NpgsqlException
    ) {
      LogErrorGettingMarketSummaries(ex);
      return [];
    }
  }

  [LoggerMessage(LogLevel.Error, "Couldn't get market summaries")]
  partial void LogErrorGettingMarketSummaries(Exception ex);

  public async Task<PagedResult<Market>> GetProductsAsync(
    GetMarketProductsFilter filter, CancellationToken cancellationToken
  ) {
    try {
      IQueryable<ProductDbEntity> baseQuery = context.Products
        .Include(p => p.Brand)
        .Include(p => p.SuperMarket)
        .Where(p => p.History.Any());

      if (filter.MarketName is not null) {
        baseQuery = baseQuery.Where(p => EF.Functions.ILike(p.SuperMarket.Name, filter.MarketName));
      }
      if (filter.BrandName is not null) {
        baseQuery = baseQuery.Where(p => EF.Functions.ILike(p.Brand.Name, filter.BrandName));
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
              p.Name,
              new ProductBrand(p.Brand.Name),
              [..p.History
                .Where(h => h.CreatedAt == latest)
                .Select(h => new ProductFormat(h.Quantity, new Price(h.Price)))]);
          })]
        ))];

      return new PagedResult<Market>(markets, totalCount);
    } catch (Exception ex) when (
      ex is NpgsqlException or OperationCanceledException ||
      ex.InnerException is NpgsqlException
    ) {
      LogErrorGettingProducts(ex);
      return new PagedResult<Market>([], 0);
    }
  }

  [LoggerMessage(LogLevel.Error, "Couldn't get market products")]
  partial void LogErrorGettingProducts(Exception ex);

  public async Task<List<Market>> GetMarketsAsync(
    CancellationToken cancellationToken
  ) {
    try {
      List<Market> existing = await context.SuperMarkets
        .Select(m => new Market(m.Name, new List<MarketProduct>()))
        .ToListAsync(cancellationToken);
      return existing;
    } catch (Exception ex) when (
      ex is NpgsqlException or OperationCanceledException ||
      ex.InnerException is NpgsqlException
    ) {
      LogErrorGettingMarkets(ex);
      return [];
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't get markets")]
  partial void LogErrorGettingMarkets(Exception ex);

  public async Task AddMarketsAsync(
    IReadOnlyCollection<Market> markets, CancellationToken cancellationToken
  ) {
    try {
      context.SuperMarkets.AddRange(
        markets.Select(m => new SuperMarketDbEntity { Name = m.Name }));
      await context.SaveChangesAsync(cancellationToken);
    } catch (Exception ex) when (
      ex is NpgsqlException or OperationCanceledException ||
      ex.InnerException is NpgsqlException
    ) {
      LogErrorAddingMarkets(string.Join(", ", markets.Select(m => m.Name)), ex);
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't add markets {MarketNames}")]
  partial void LogErrorAddingMarkets(string marketNames, Exception ex);

  public async Task<List<ProductBrand>> GetBrandsAsync(
    CancellationToken cancellationToken
  ) {
    try {
      return await context.ProductBrands
        .Select(b => new ProductBrand(b.Name))
        .ToListAsync(cancellationToken);
    } catch (Exception ex) when (
      ex is NpgsqlException or OperationCanceledException ||
      ex.InnerException is NpgsqlException
    ) {
      LogErrorGettingBrands(ex);
      return [];
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't get brands")]
  partial void LogErrorGettingBrands(Exception ex);

  public async Task AddBrandsAsync(
    IReadOnlyCollection<ProductBrand> brands, CancellationToken cancellationToken
  ) {
    try {
      context.ProductBrands.AddRange(
        brands.Select(b => new ProductBrandDbEntity { Name = b.Name }));
      await context.SaveChangesAsync(cancellationToken);
    } catch (Exception ex) when (
      ex is NpgsqlException or OperationCanceledException ||
      ex.InnerException is NpgsqlException
    ) {
      LogErrorAddingBrands(string.Join(", ", brands.Select(b => b.Name)), ex);
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't add brands {BrandNames}")]
  partial void LogErrorAddingBrands(string brandNames, Exception ex);

  public async Task<IReadOnlyCollection<int>> AddMarketProductsAsync(
    Market market,
    DateOnly registeredAt,
    CancellationToken cancellationToken
  ) {
    context.ChangeTracker.AutoDetectChangesEnabled = false;
    context.ChangeTracker.Clear();

    int superMarketId = await context.SuperMarkets
      .Where(s => s.Name == market.Name)
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
  }

  public async Task DeleteProductsAsync(
    IReadOnlyCollection<int> productIds, CancellationToken cancellationToken
  ) {
    await context.Products
      .Where(p => productIds.Contains(p.Id))
      .ExecuteDeleteAsync(cancellationToken);
  }

  public async Task DeleteProductsHistoryForMarketsAsync(
    IReadOnlyCollection<string> marketNames,
    DateOnly registeredAt,
    CancellationToken cancellationToken
  ) {
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
  }

  public async Task DeleteMarketsAsync(
    IReadOnlyCollection<string> marketNames, CancellationToken cancellationToken
  ) {
    await context.SuperMarkets
      .Where(m => marketNames.Contains(m.Name))
      .ExecuteDeleteAsync(cancellationToken);
  }

  public async Task DeleteBrandsAsync(
    IReadOnlyCollection<string> brandNames, CancellationToken cancellationToken
  ) {
    await context.ProductBrands
      .Where(b => brandNames.Contains(b.Name))
      .ExecuteDeleteAsync(cancellationToken);
  }

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't add products {ProductNames}")]
  partial void LogErrorAddingProducts(string productNames, Exception ex);
}
