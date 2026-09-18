namespace Metaspesa.RestApi.Markets.GetMarkets;

/// <summary>Markets available in the catalog.</summary>
/// <param name="Items">Market summaries; empty when no markets are registered.</param>
internal sealed record MarketListResponse(IReadOnlyCollection<MarketResponse> Items);