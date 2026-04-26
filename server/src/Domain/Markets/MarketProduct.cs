using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.Markets;

public record MarketProduct(
  string Name, string Quantity, Price Price, ProductBrand Brand
) : Product(Name, Quantity, Price);
