namespace Metaspesa.Application.Abstractions.Markets;

public sealed record MarketCatalog(
  string Name, IReadOnlyCollection<MarketProduct> Products);
