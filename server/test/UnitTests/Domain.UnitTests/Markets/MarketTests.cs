using Metaspesa.Domain.Markets;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class MarketTests {

  [Fact(DisplayName = "Market owns identity, name, and optional logo only")]
  public static void Market_Created_WithBoundedState() {
    ImageUrl Image = new(new Uri("https://example.com/product.png"));
    var market = new Market(new MarketId(1), new MarketName("Market"), Image);

    Assert.Equal(new MarketId(1), market.Id);
    Assert.Equal(new MarketName("Market"), market.Name);
    Assert.Equal(Image, market.LogoUrl);
  }
}