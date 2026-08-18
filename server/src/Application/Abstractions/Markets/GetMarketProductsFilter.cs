using Metaspesa.Application.Abstractions.Core;

namespace Metaspesa.Application.Abstractions.Markets;

public record GetMarketProductsFilter {
  public GetMarketProductsFilter(
    string? marketName,
    string? brandNameSegment,
    string? nameSegment,
    Pagination? pagination
  ) {
    if (pagination is { IsInfinite: false, Index: <= 0 }) {
      throw new ArgumentOutOfRangeException(
        nameof(pagination),
        pagination.Index,
        "Page index must be greater than 0.");
    }
    if (pagination is { IsInfinite: false, Size: <= 0 }) {
      throw new ArgumentOutOfRangeException(
        nameof(pagination),
        pagination.Size,
        "Page size must be greater than 0.");
    }

    MarketName = marketName;
    BrandNameSegment = brandNameSegment;
    NameSegment = nameSegment;
    Pagination = pagination;
  }

  public string? MarketName { get; init; }
  public string? BrandNameSegment { get; init; }
  public string? NameSegment { get; init; }
  public Pagination? Pagination { get; init; }
}
