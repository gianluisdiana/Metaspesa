namespace Metaspesa.Domain.Markets;

public record MarketProduct(
  string Name, ProductBrand Brand, IReadOnlyCollection<ProductFormat> Formats
);