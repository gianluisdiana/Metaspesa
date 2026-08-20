namespace Metaspesa.Domain.Markets;

public sealed class Market(
  MarketId id, MarketName name, ImageUrl? logoUrl = null
) {
  public MarketId Id { get; } = id;
  public MarketName Name { get; } = name;
  public ImageUrl? LogoUrl { get; } = logoUrl;
}