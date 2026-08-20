namespace Metaspesa.Application.Abstractions.Markets;

public sealed record MarketProduct(
  string Name,
  string BrandName,
  IReadOnlyCollection<MarketProductFormat> Formats
);