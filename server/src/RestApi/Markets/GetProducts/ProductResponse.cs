using Metaspesa.RestApi.Markets;

namespace Metaspesa.RestApi.Markets.GetProducts;

/// <summary>One market-owned product. IDs do not imply cross-market equivalence.</summary>
/// <param name="Id">Stable ID of this product.</param>
/// <param name="Name">Product display name.</param>
/// <param name="Brand">Product brand display name.</param>
/// <param name="Market">Market that owns this product.</param>
/// <param name="Formats">Available quantities and their latest observed prices.</param>
internal sealed record ProductResponse(
  int Id, string Name, string Brand, MarketResponse Market,
  IReadOnlyCollection<FormatResponse> Formats);