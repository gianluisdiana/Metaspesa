namespace Metaspesa.Application.Abstractions.Markets;

public sealed record CatalogProduct(
  int Id,
  string Name,
  string Brand,
  MarketSummary Market,
  IReadOnlyCollection<CatalogFormat> Formats);

public sealed record CatalogFormat(
  int Id,
  decimal Quantity,
  string Unit,
  decimal Price,
  string Currency,
  Uri? ImageUrl,
  DateTime ObservedAt);