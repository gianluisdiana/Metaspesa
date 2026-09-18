using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Database.Entities;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.Repositories;

internal partial class PostgreSqlMarketProductRepository(
  MainContext context
) : IProductRepository {
  private const int BatchSize = 1_000;

  private readonly record struct ProductSource(
    int ProductId, ProductImport Source);
  private readonly record struct ProductInsert(
    ProductDbEntity Entity, ProductImport Source);
  private readonly record struct ProductFormatKey(
    int ProductId,
    decimal Quantity,
    int UnitOfMeasureId
  );

  public async Task<Product?> GetByIdAsync(
    ProductId productId, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    ProductDbEntity? entity = await context.Products
      .AsNoTracking()
      .Include(product => product.Brand)
      .Include(product => product.Formats)
      .ThenInclude(format => format.UnitOfMeasure)
      .SingleOrDefaultAsync(
        product => product.Id == productId.Value,
        cancellationToken);

    return entity?.MapToDomain();
  }, "Couldn't get product.");

  public async Task<PagedResult<CatalogProduct>> GetProductsAsync(
    GetMarketProductsFilter filter, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    IQueryable<ProductDbEntity> query = context.Products
      .AsNoTracking()
      .Where(product => product.Formats.Any(format => format.PriceSnapshots.Count > 0));

    if (!string.IsNullOrWhiteSpace(filter.NameSegment)) {
      query = query.Where(product => EF.Functions.ILike(
        product.Name, $"%{EscapeLike(filter.NameSegment)}%", "\\"));
    }
    if (filter.MarketIds.Count > 0) {
      int[] marketIds = [.. filter.MarketIds.Select(id => id.Value)];
      query = query.Where(product => marketIds.Contains(product.SuperMarketId));
    }
    if (!string.IsNullOrWhiteSpace(filter.BrandNameSegment)) {
      query = query.Where(product => EF.Functions.ILike(
        product.Brand.Name, $"%{EscapeLike(filter.BrandNameSegment)}%", "\\"));
    }

    int totalCount = await query.CountAsync(cancellationToken);
    IOrderedQueryable<ProductDbEntity> ordered = filter.Sort switch {
      CatalogSort.PriceAsc => query.OrderBy(product => product.Formats
        .Where(format => format.PriceSnapshots.Count > 0)
        .Min(format => format.PriceSnapshots
          .OrderByDescending(snapshot => snapshot.ObservedAt)
          .ThenByDescending(snapshot => snapshot.Id)
          .Select(snapshot => snapshot.PriceAmount).First())),
      CatalogSort.PriceDesc => query.OrderByDescending(product => product.Formats
        .Where(format => format.PriceSnapshots.Count > 0)
        .Min(format => format.PriceSnapshots
          .OrderByDescending(snapshot => snapshot.ObservedAt)
          .ThenByDescending(snapshot => snapshot.Id)
          .Select(snapshot => snapshot.PriceAmount).First())),
      _ => query.OrderBy(product => product.Name),
    };

    List<ProductDbEntity> products = await ordered
      .ThenBy(product => product.Name)
      .ThenBy(product => product.Id)
      .Skip(filter.Pagination.Skip)
      .Take(filter.Pagination.Size)
      .Include(product => product.Brand)
      .Include(product => product.SuperMarket)
      .Include(product => product.Formats)
      .ThenInclude(format => format.UnitOfMeasure)
      .Include(product => product.Formats)
      .ThenInclude(format => format.PriceSnapshots)
      .AsSplitQuery()
      .ToListAsync(cancellationToken);

    return new PagedResult<CatalogProduct>([
      .. products.Select(product => new CatalogProduct(
        product.Id,
        product.Name,
        product.Brand.Name,
        new MarketSummary(product.SuperMarketId, product.SuperMarket.Name,
          ToUri(product.SuperMarket.LogoUrl)),
        [.. product.Formats
          .Where(format => format.PriceSnapshots.Count > 0)
          .OrderBy(format => format.Id)
          .Select(ToCatalogFormat)]))
    ], totalCount);
  }, "Couldn't get market products.");

  public async Task<IReadOnlyDictionary<int, MarketProduct>> GetProductsAsync(
    IReadOnlyCollection<int> productFormatIds,
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    if (productFormatIds.Count == 0) {
      return [];
    }

    List<ProductFormatDbEntity> formats = await context.ProductFormats
      .AsNoTracking()
      .Include(format => format.Product)
      .ThenInclude(product => product.Brand)
      .Include(format => format.UnitOfMeasure)
      .Include(format => format.PriceSnapshots)
      .Where(format =>
        productFormatIds.Contains(format.Id) &&
        format.PriceSnapshots.Count > 0)
      .ToListAsync(cancellationToken);

    return formats.ToDictionary(
      format => format.Id,
      format => new MarketProduct(
        format.Product.Name,
        format.Product.Brand.Name,
        [ToReadModel(format)]));
  }, "Couldn't get market products by references.");

  public async Task<IReadOnlyCollection<BrandName>> GetBrandsAsync(
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    List<string> names = await context.ProductBrands
      .AsNoTracking()
      .Select(brand => brand.Name)
      .ToListAsync(cancellationToken);
    return (IReadOnlyCollection<BrandName>)[
      ..names.Select(name => new BrandName(name))
    ];
  }, "Couldn't get brands.");

  public async Task AddBrandsAsync(
    IReadOnlyCollection<BrandName> brands,
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    context.ProductBrands.AddRange(
      brands.Select(brand => new ProductBrandDbEntity { Name = brand.Value }));
    await context.SaveChangesAsync(cancellationToken);
  }, "Couldn't add brands.");

  public async Task<ProductImportResult> ResolveProductsAsync(
    MarketImport market,
    DateTime observedAt,
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    bool autoDetectChangesEnabled =
      context.ChangeTracker.AutoDetectChangesEnabled;
    context.ChangeTracker.AutoDetectChangesEnabled = false;
    try {
      context.ChangeTracker.Clear();

      int marketId = await GetMarketIdAsync(market.Name, cancellationToken);
      Dictionary<string, int> brandLookup = await GetBrandLookupAsync(
        market.Products, cancellationToken);
      Dictionary<(string Name, int BrandId), int> existingProducts =
        await GetExistingProductLookupAsync(marketId, cancellationToken);

      (List<ProductSource> productSources, List<ProductId> addedProductIds) =
        await ResolveProductSourcesAsync(
          market.Products,
          marketId,
          brandLookup,
          existingProducts,
          cancellationToken);

      Dictionary<string, int> unitLookup = await GetUnitLookupAsync(
        cancellationToken);
      (
        Dictionary<ProductFormatKey, int> formatLookup,
        IReadOnlyCollection<ProductFormatId> addedProductFormatIds
      ) = await ResolveProductFormatsAsync(
        productSources, unitLookup, cancellationToken);
      IReadOnlyCollection<PriceObservation> observations = BuildPriceObservations(
        productSources, unitLookup, formatLookup, observedAt);

      return new ProductImportResult(
        addedProductIds, addedProductFormatIds, observations);
    } finally {
      context.ChangeTracker.AutoDetectChangesEnabled =
        autoDetectChangesEnabled;
    }
  }, "Couldn't resolve market products.");

  public async Task DeleteBrandsAsync(
    IReadOnlyCollection<BrandName> brandNames,
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    string[] names = [.. brandNames.Select(name => name.Value)];
    List<int> productIds = await context.Products
      .Where(product => names.Contains(product.Brand.Name))
      .Select(product => product.Id)
      .ToListAsync(cancellationToken);
    await DeleteProductsByIdsAsync(productIds, cancellationToken);
    await context.ProductBrands
      .Where(brand => names.Contains(brand.Name))
      .ExecuteDeleteAsync(cancellationToken);
  }, "Couldn't delete brands.");

  public async Task DeleteProductsAsync(
    IReadOnlyCollection<ProductId> productIds,
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => await DeleteProductsByIdsAsync(
      [.. productIds.Select(id => id.Value)], cancellationToken),
    "Couldn't delete products.");

  public async Task DeleteProductFormatsAsync(
    IReadOnlyCollection<ProductFormatId> productFormatIds,
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    int[] ids = [.. productFormatIds.Select(id => id.Value)];
    await context.PriceSnapshots
      .Where(snapshot => ids.Contains(snapshot.ProductFormatId))
      .ExecuteDeleteAsync(cancellationToken);
    await context.ProductFormats
      .Where(format => ids.Contains(format.Id))
      .ExecuteDeleteAsync(cancellationToken);
  }, "Couldn't delete product formats.");

  private static CatalogFormat ToCatalogFormat(ProductFormatDbEntity format) {
    PriceSnapshotDbEntity snapshot = format.PriceSnapshots
      .OrderByDescending(value => value.ObservedAt)
      .ThenByDescending(value => value.Id)
      .First();
    return new CatalogFormat(
      format.Id, format.Quantity, format.UnitOfMeasure.Code,
      snapshot.PriceAmount, snapshot.CurrencyCode, ToUri(format.ImageUrl),
      DateTime.SpecifyKind(snapshot.ObservedAt, DateTimeKind.Utc));
  }

  private static MarketProductFormat ToReadModel(ProductFormatDbEntity format) {
    PriceSnapshotDbEntity snapshot = format.PriceSnapshots
      .OrderByDescending(value => value.ObservedAt)
      .ThenByDescending(value => value.Id)
      .First();

    return new MarketProductFormat(
      new Quantity(
        format.Quantity,
        new UnitOfMeasure(format.UnitOfMeasure.Code)),
      new Money(snapshot.PriceAmount),
      ToUri(format.ImageUrl),
      format.Id);
  }

  private async Task<int> GetMarketIdAsync(
    MarketName marketName,
    CancellationToken cancellationToken
  ) => await context.SuperMarkets
    .Where(market => EF.Functions.ILike(market.Name, marketName.Value))
    .Select(market => market.Id)
    .SingleAsync(cancellationToken);

  private async Task<Dictionary<string, int>> GetBrandLookupAsync(
    IReadOnlyCollection<ProductImport> products,
    CancellationToken cancellationToken
  ) {
    string[] brandNames = [
      ..products.Select(product => product.Brand.Value).Distinct()
    ];

    return await context.ProductBrands
      .Where(brand => brandNames.Contains(brand.Name))
      .ToDictionaryAsync(
        brand => brand.Name,
        brand => brand.Id,
        cancellationToken);
  }

  private async Task<Dictionary<(string Name, int BrandId), int>>
    GetExistingProductLookupAsync(
      int marketId,
      CancellationToken cancellationToken
    ) => await context.Products
      .Where(product => product.SuperMarketId == marketId)
      .ToDictionaryAsync(
        product => new ValueTuple<string, int>(product.Name, product.BrandId),
        product => product.Id,
        cancellationToken);

  private async Task<(List<ProductSource> Sources, List<ProductId> AddedProductIds)>
    ResolveProductSourcesAsync(
      IReadOnlyCollection<ProductImport> products,
      int marketId,
      Dictionary<string, int> brandLookup,
      Dictionary<(string Name, int BrandId), int> existingProducts,
      CancellationToken cancellationToken
    ) {
    var toInsert = new List<ProductInsert>();
    var sources = new List<ProductSource>();

    foreach (ProductImport product in products) {
      int brandId = brandLookup[product.Brand.Value];
      if (existingProducts.TryGetValue(
        (product.Name.Value, brandId), out int existingId)) {
        sources.Add(new ProductSource(existingId, product));
        continue;
      }

      toInsert.Add(new ProductInsert(
        new ProductDbEntity {
          Name = product.Name.Value,
          SuperMarketId = marketId,
          BrandId = brandId,
        },
        product));
    }

    List<ProductSource> newSources = await AddProductsAsync(
      toInsert, cancellationToken);
    sources.AddRange(newSources);

    return (
      sources,
      [.. newSources.Select(source => new ProductId(source.ProductId))]);
  }

  private async Task<List<ProductSource>> AddProductsAsync(
    IReadOnlyCollection<ProductInsert> toInsert,
    CancellationToken cancellationToken
  ) {
    IEnumerable<List<ProductInsert>> batches = toInsert
      .Chunk(BatchSize)
      .Select(batch => batch.ToList());

    List<ProductSource> sources = [];
    foreach (List<ProductInsert> batch in batches) {
      await context.Products.AddRangeAsync(
        batch.Select(value => value.Entity), cancellationToken);
      await context.SaveChangesAsync(cancellationToken);
      context.ChangeTracker.Clear();
      sources.AddRange(batch.Select(value =>
        new ProductSource(value.Entity.Id, value.Source)));
    }

    return sources;
  }

  private async Task<Dictionary<string, int>> GetUnitLookupAsync(
    CancellationToken cancellationToken
  ) => await context.UnitsOfMeasure
    .AsNoTracking()
    .ToDictionaryAsync(
      unit => unit.Code,
      unit => unit.Id,
      StringComparer.OrdinalIgnoreCase,
      cancellationToken);

  private async Task<(
    Dictionary<ProductFormatKey, int> FormatLookup,
    IReadOnlyCollection<ProductFormatId> AddedProductFormatIds
  )> ResolveProductFormatsAsync(
    IReadOnlyCollection<ProductSource> productSources,
    IReadOnlyDictionary<string, int> unitLookup,
    CancellationToken cancellationToken
  ) {
    Dictionary<ProductFormatKey, int> formatLookup =
      await GetExistingProductFormatLookupAsync(
        productSources, cancellationToken);
    List<ProductFormatDbEntity> missingFormats = BuildMissingProductFormats(
      productSources, unitLookup, formatLookup);
    IDictionary<ProductFormatKey, int> newFormats = await AddProductFormatsAsync(
      missingFormats, cancellationToken);

    return (
      formatLookup.Concat(newFormats)
        .ToDictionary(pair => pair.Key, pair => pair.Value),
      [.. newFormats.Values.Select(id => new ProductFormatId(id))]);
  }

  private async Task<Dictionary<ProductFormatKey, int>>
    GetExistingProductFormatLookupAsync(
      IReadOnlyCollection<ProductSource> productSources,
      CancellationToken cancellationToken
    ) {
    int[] productIds = [
      ..productSources.Select(source => source.ProductId).Distinct()
    ];

    return await context.ProductFormats
      .Where(format => productIds.Contains(format.ProductId))
      .Select(format => new {
        format.Id,
        format.ProductId,
        format.Quantity,
        format.UnitOfMeasureId,
      })
      .ToDictionaryAsync(
        format => new ProductFormatKey(
          format.ProductId,
          format.Quantity,
          format.UnitOfMeasureId),
        format => format.Id,
        cancellationToken);
  }

  private static List<ProductFormatDbEntity> BuildMissingProductFormats(
    IEnumerable<ProductSource> productSources,
    IReadOnlyDictionary<string, int> unitLookup,
    IReadOnlyDictionary<ProductFormatKey, int> existingFormatLookup
  ) {
    var missingFormats = new Dictionary<ProductFormatKey, ProductFormatDbEntity>();

    foreach (ProductSource productSource in productSources) {
      foreach (ProductFormatImport format in productSource.Source.Formats) {
        ProductFormatKey key = ToProductFormatKey(
          productSource.ProductId, format, unitLookup);
        if (existingFormatLookup.ContainsKey(key) ||
          missingFormats.ContainsKey(key)) {
          continue;
        }

        missingFormats[key] = new ProductFormatDbEntity {
          ProductId = key.ProductId,
          Quantity = key.Quantity,
          UnitOfMeasureId = key.UnitOfMeasureId,
          ImageUrl = format.ImageUrl?.Value.ToString() ?? string.Empty,
        };
      }
    }

    return [.. missingFormats.Values];
  }

  private async Task<IDictionary<ProductFormatKey, int>> AddProductFormatsAsync(
    IReadOnlyCollection<ProductFormatDbEntity> formats,
    CancellationToken cancellationToken
  ) {
    var newFormatLookup = new Dictionary<ProductFormatKey, int>();
    IEnumerable<List<ProductFormatDbEntity>> batches = formats
      .Chunk(BatchSize)
      .Select(batch => batch.ToList());

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

  private static IReadOnlyCollection<PriceObservation> BuildPriceObservations(
    IEnumerable<ProductSource> productSources,
    IReadOnlyDictionary<string, int> unitLookup,
    IReadOnlyDictionary<ProductFormatKey, int> formatLookup,
    DateTime observedAt
  ) => [
    ..productSources.SelectMany(productSource =>
      productSource.Source.Formats.Select(format => {
        ProductFormatKey key = ToProductFormatKey(
          productSource.ProductId, format, unitLookup);
        return new PriceObservation(
          new ProductFormatId(formatLookup[key]),
          format.Price,
          observedAt);
      }))
  ];

  private static ProductFormatKey ToProductFormatKey(
    int productId,
    ProductFormatImport format,
    IReadOnlyDictionary<string, int> unitLookup
  ) => new(
    productId,
    format.Quantity.Amount,
    unitLookup[format.Quantity.UnitOfMeasure.Value]);

  private async Task DeleteProductsByIdsAsync(
    IReadOnlyCollection<int> productIds,
    CancellationToken cancellationToken
  ) {
    await context.PriceSnapshots
      .Where(snapshot => productIds.Contains(snapshot.ProductFormat.ProductId))
      .ExecuteDeleteAsync(cancellationToken);
    await context.ProductFormats
      .Where(format => productIds.Contains(format.ProductId))
      .ExecuteDeleteAsync(cancellationToken);
    await context.Products
      .Where(product => productIds.Contains(product.Id))
      .ExecuteDeleteAsync(cancellationToken);
  }

  private static Uri? ToUri(string? value) =>
    string.IsNullOrWhiteSpace(value)
      ? null
      : new Uri(value, UriKind.Absolute);

  private static string EscapeLike(string value) => value
    .Replace("\\", "\\\\", StringComparison.Ordinal)
    .Replace("%", "\\%", StringComparison.Ordinal)
    .Replace("_", "\\_", StringComparison.Ordinal);
}