using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Markets;
using NSubstitute;

namespace Metaspesa.Application.UnitTests.Markets;

public class GetMarketProductsHandlerTest {
  private readonly IProductRepository _productRepository;
  private readonly GetMarketProducts.Handler _handler;

  public GetMarketProductsHandlerTest() {
    _productRepository = Substitute.For<IProductRepository>();
    _productRepository
      .GetProductsAsync(
        Arg.Any<GetMarketProductsFilter>(),
        Arg.Any<CancellationToken>())
      .Returns(new PagedResult<MarketCatalog>([], 0));
    _handler = new GetMarketProducts.Handler(_productRepository);
  }

  [Fact(DisplayName = "Passes filters and pagination to product repository")]
  public async Task Handler_PassesFilter_ToProductRepository() {
    var filter = new GetMarketProductsFilter(
      "Mercadona",
      "Hacendado",
      "leche",
      new Pagination(3, 10));

    await _handler.Handle(
      new GetMarketProducts.Query(filter),
      TestContext.Current.CancellationToken);

    await _productRepository.Received(1).GetProductsAsync(
      Arg.Is<GetMarketProductsFilter>(value =>
        value.MarketName == "Mercadona" &&
        value.BrandNameSegment == "Hacendado" &&
        value.NameSegment == "leche" &&
        value.Pagination!.Index == 3 &&
        value.Pagination.Size == 10),
      TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Assigns infinite pagination when pagination is missing")]
  public async Task Handler_AssignsInfinitePagination_WhenPaginationIsMissing() {
    var filter = new GetMarketProductsFilter(null, null, null, null);

    await _handler.Handle(
      new GetMarketProducts.Query(filter),
      TestContext.Current.CancellationToken);

    await _productRepository.Received(1).GetProductsAsync(
      Arg.Is<GetMarketProductsFilter>(value =>
        value.Pagination != null && value.Pagination.IsInfinite),
      TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Returns catalog and total count from product repository")]
  public async Task Handler_ReturnsCatalog_FromProductRepository() {
    var catalogs = new List<MarketCatalog> {
      new("Mercadona", [new MarketProduct("Milk", "Brand", [])]),
    };
    _productRepository
      .GetProductsAsync(
        Arg.Any<GetMarketProductsFilter>(),
        Arg.Any<CancellationToken>())
      .Returns(new PagedResult<MarketCatalog>(catalogs, 42));

    PagedResult<MarketCatalog> result = await _handler.Handle(
      new GetMarketProducts.Query(
        new GetMarketProductsFilter(null, null, null, null)),
      TestContext.Current.CancellationToken);

    Assert.Single(result.Values);
    Assert.Equal(42, result.TotalCount);
  }
}