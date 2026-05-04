using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Markets;
using Metaspesa.Domain.Markets;
using NSubstitute;

namespace Metaspesa.Application.UnitTests.Markets;

public class GetMarketsHandlerTest {
  private readonly IMarketRepository _marketRepository;
  private readonly GetMarkets.Handler _handler;

  public GetMarketsHandlerTest() {
    _marketRepository = Substitute.For<IMarketRepository>();
    _handler = new GetMarkets.Handler(_marketRepository);
  }

  [Fact(DisplayName = "Returns markets from repository")]
  public async Task Handler_ReturnsMarkets_FromRepository() {
    // Arrange
    List<MarketSummary> summaries = [
      new MarketSummary("Mercadona", new Uri("https://example.com/mercadona.png")),
      new MarketSummary("Alcampo", null),
    ];
    _marketRepository
      .GetMarketSummariesAsync(Arg.Any<CancellationToken>())
      .Returns(summaries);

    // Act
    Result<IReadOnlyCollection<MarketSummary>> result =
      await _handler.Handle(new GetMarkets.Query(), TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(2, result.Value.Count);
  }

  [Fact(DisplayName = "Returns empty when repository returns nothing")]
  public async Task Handler_ReturnsEmpty_WhenRepositoryReturnsNothing() {
    // Arrange
    _marketRepository
      .GetMarketSummariesAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    // Act
    Result<IReadOnlyCollection<MarketSummary>> result =
      await _handler.Handle(new GetMarkets.Query(), TestContext.Current.CancellationToken);

    // Assert
    Assert.Empty(result.Value);
  }

  [Fact(DisplayName = "Calls repository once")]
  public async Task Handler_CallsRepository_Once() {
    // Arrange
    _marketRepository
      .GetMarketSummariesAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    // Act
    await _handler.Handle(new GetMarkets.Query(), TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.Received(1).GetMarketSummariesAsync(TestContext.Current.CancellationToken);
  }
}
