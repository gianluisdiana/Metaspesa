using System.Text.Json.Serialization;
using Metaspesa.Application.Abstractions.Markets;

namespace Metaspesa.RestApi.Markets;

/// <summary>Market identity and display information.</summary>
/// <param name="Id">Stable market ID used by the catalog marketId filter.</param>
/// <param name="Name">Display name of the market.</param>
internal sealed record MarketResponse(int Id, string Name) {
  /// <summary>Absolute logo URL when available; otherwise omitted.</summary>
  [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? LogoUrl { get; init; }

  public static MarketResponse FromSummary(MarketSummary market) =>
    new(market.Id, market.Name) { LogoUrl = market.LogoUrl?.ToString() };
}