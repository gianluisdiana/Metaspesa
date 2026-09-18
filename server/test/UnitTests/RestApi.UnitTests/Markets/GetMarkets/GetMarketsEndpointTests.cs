using System.Text.Json;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.RestApi.Markets.GetMarkets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using GetMarketsUseCase = Metaspesa.Application.Markets.GetMarkets;

namespace Metaspesa.RestApi.UnitTests.Markets.GetMarkets;

public static class GetMarketsEndpointTests {
  [Fact]
  public static async Task GetMarkets_ReturnsIdentifiers() {
    IMarketRepository repository = Substitute.For<IMarketRepository>();
    repository.GetMarketSummariesAsync(Arg.Any<CancellationToken>())
      .Returns([new MarketSummary(7, "Market", null)]);
    var context = new DefaultHttpContext {
      RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider(),
    };
    context.Response.Body = new MemoryStream();

    IResult response = await GetMarketsEndpoint.HandleAsync(
      new GetMarketsUseCase.Handler(repository), TestContext.Current.CancellationToken);
    await response.ExecuteAsync(context);
    context.Response.Body.Position = 0;
    using JsonDocument json = await JsonDocument.ParseAsync(context.Response.Body,
      cancellationToken: TestContext.Current.CancellationToken);

    Assert.Equal(7, json.RootElement.GetProperty("items")[0]
      .GetProperty("id").GetInt32());
  }
}