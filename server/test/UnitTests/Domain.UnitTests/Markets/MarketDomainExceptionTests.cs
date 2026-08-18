using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class MarketDomainExceptionTests {
  [Theory(DisplayName = "Market exceptions derive from shared domain exception")]
  [ClassData<MarketExceptions>]
  public static void MarketException_DerivesFromDomainException(
    Exception exception
  ) {
    Assert.IsType<MarketDomainException>(exception, exactMatch: false);
  }

  private sealed class MarketExceptions() : TheoryData<Exception>(
    (Exception)new DuplicateMarketProductException(),
    (Exception)new DuplicateProductFormatException(),
    (Exception)new EmptyMarketProductsException(),
    (Exception)new InvalidBrandNameException(),
    (Exception)new InvalidImageUrlException(),
    (Exception)new InvalidMarketIdException(),
    (Exception)new InvalidMarketNameException(),
    (Exception)new InvalidMarketProductsRegisteredAtException(),
    (Exception)new InvalidPriceSnapshotIdException(),
    (Exception)new InvalidPriceSnapshotObservedAtException(),
    (Exception)new InvalidProductFormatIdException(),
    (Exception)new InvalidProductIdException(),
    (Exception)new InvalidProductNameException(),
    (Exception)new ProductFormatNotFoundException(),
    (Exception)new UnsupportedUnitOfMeasureException()
  );
}
