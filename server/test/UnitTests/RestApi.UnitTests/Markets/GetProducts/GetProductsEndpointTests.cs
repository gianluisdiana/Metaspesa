using System.Text.Json;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Markets;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;
using Metaspesa.RestApi.Markets.GetProducts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Metaspesa.RestApi.UnitTests.Markets.GetProducts;

public static class GetProductsEndpointTests {
  private static readonly string[] MarketIds = ["2", "4"];

  [Fact]
  public static void ParseFilter_UsesDocumentedDefaults() {
    GetMarketProductsFilter filter = GetProductsEndpoint.ParseFilter(
      new QueryCollection());

    Assert.Equal(1, filter.Pagination.Index);
    Assert.Equal(24, filter.Pagination.Size);
    Assert.Equal(CatalogSort.Name, filter.Sort);
    Assert.Empty(filter.MarketIds);
  }

  [Fact]
  public static void ParseFilter_AcceptsRepeatedMarketsAndSort() {
    var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
      ["marketId"] = new(MarketIds),
      ["sort"] = "priceDesc",
      ["page"] = "3",
      ["pageSize"] = "10",
      ["query"] = "milk",
      ["brand"] = "Brand",
    });

    GetMarketProductsFilter filter = GetProductsEndpoint.ParseFilter(query);

    Assert.Equal([new MarketId(2), new MarketId(4)], filter.MarketIds);
    Assert.Equal(CatalogSort.PriceDesc, filter.Sort);
    Assert.Equal(3, filter.Pagination.Index);
    Assert.Equal(10, filter.Pagination.Size);
    Assert.Equal("milk", filter.NameSegment);
    Assert.Equal("Brand", filter.BrandNameSegment);
  }

  [Theory]
  [InlineData("pageSize", "101")]
  [InlineData("page", "0")]
  [InlineData("marketId", "abc")]
  [InlineData("sort", "newest")]
  public static void ParseFilter_RejectsInvalidValues(string key, string value) {
    var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
      [key] = value,
    });

    Assert.Throws<BadHttpRequestException>(() => GetProductsEndpoint.ParseFilter(query));
  }

  [Fact]
  public static void ParseFilter_UsesDomainExceptionForInvalidMarketId() {
    var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
      ["marketId"] = "0",
    });

    Assert.Throws<InvalidMarketIdException>(() => GetProductsEndpoint.ParseFilter(query));
  }

  [Fact]
  public static async Task GetProducts_ReturnsFlatPageAndOmitsMissingImages() {
    IProductRepository repository = Substitute.For<IProductRepository>();
    repository.GetProductsAsync(Arg.Any<GetMarketProductsFilter>(), Arg.Any<CancellationToken>())
      .Returns(new PagedResult<CatalogProduct>([
        new CatalogProduct(41, "Whole milk", "Hacendado",
          new MarketSummary(1, "Mercadona", null), [
            new CatalogFormat(93, 1, "l", 1.04m, "EUR", null,
              new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc)),
          ]),
      ], 25));
    var context = new DefaultHttpContext {
      RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider(),
    };
    context.Response.Body = new MemoryStream();

    IResult response = await GetProductsEndpoint.GetProductsAsync(
      context.Request, new GetMarketProducts.Handler(repository),
      TestContext.Current.CancellationToken);
    await response.ExecuteAsync(context);
    context.Response.Body.Position = 0;
    using JsonDocument json = await JsonDocument.ParseAsync(context.Response.Body,
      cancellationToken: TestContext.Current.CancellationToken);

    JsonElement root = json.RootElement;
    Assert.Equal(200, context.Response.StatusCode);
    Assert.Equal(1, root.GetProperty("page").GetInt32());
    Assert.Equal(24, root.GetProperty("pageSize").GetInt32());
    Assert.Equal(25, root.GetProperty("totalItems").GetInt32());
    Assert.Equal(2, root.GetProperty("totalPages").GetInt32());
    JsonElement product = root.GetProperty("items")[0];
    Assert.Equal(41, product.GetProperty("id").GetInt32());
    Assert.Equal(1, product.GetProperty("market").GetProperty("id").GetInt32());
    Assert.False(product.GetProperty("market").TryGetProperty("logoUrl", out _));
    JsonElement format = product.GetProperty("formats")[0];
    Assert.Equal(93, format.GetProperty("id").GetInt32());
    Assert.Equal("EUR", format.GetProperty("currentPrice").GetProperty("currency").GetString());
    Assert.False(format.TryGetProperty("imageUrl", out _));
  }

}