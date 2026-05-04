using Metaspesa.Application.Abstractions.Core;

namespace Metaspesa.Application.Abstractions.Markets;

public record GetMarketProductsFilter(
  string? MarketName,
  string? BrandName,
  string? NameSegment,
  Pagination? Pagination
);
