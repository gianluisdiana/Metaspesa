namespace Metaspesa.RestApi.Markets.GetProducts;

/// <summary>Product catalog query filters.</summary>
internal sealed record GetProductsRequest {
  /// <summary>Case-insensitive product-name fragment. Omit to include all names.</summary>
  public string? Query { get; init; }

  /// <summary>Include products from several markets. IDs must be UUIDs.</summary>
  public IReadOnlyCollection<Guid>? MarketId { get; init; }

  /// <summary>Case-insensitive brand-name fragment. Omit to include all brands.</summary>
  public string? Brand { get; init; }

  /// <summary>One-based page number; defaults to 1.</summary>
  public int? Page { get; init; }

  /// <summary>Maximum products per page, from 1 to 100; defaults to 24.</summary>
  public int? PageSize { get; init; }

  /// <summary>Sort by name or lowest latest format price. Defaults to name.</summary>
  public string? Sort { get; init; }
}