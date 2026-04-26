namespace Metaspesa.Domain.Markets;

public record Market(string Name, IReadOnlyCollection<MarketProduct> Products);
