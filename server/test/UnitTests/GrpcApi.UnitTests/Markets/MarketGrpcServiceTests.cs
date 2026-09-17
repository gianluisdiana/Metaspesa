using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Markets;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.GrpcApi.Protos.Markets;
using Metaspesa.GrpcApi.Services;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using DomainMarket = Metaspesa.Application.Abstractions.Markets.MarketCatalog;
using DomainMarketProduct = Metaspesa.Application.Abstractions.Markets.MarketProduct;
using DomainMarketSummary = Metaspesa.Application.Abstractions.Markets.MarketSummary;
using DomainPrice = Metaspesa.Domain.SharedKernel.Money;

namespace Metaspesa.GrpcApi.UnitTests.Markets;

public static class MarketGrpcServiceTests {
  public class AuthorizationAttributes {
    [Fact(DisplayName = "Allows anonymous users to get market products")]
    public void GetMarketProducts_AllowsAnonymousUsers() {
      // Act
      object[] attributes = typeof(MarketGrpcService)
        .GetMethod(nameof(MarketGrpcService.GetMarketProducts))!
        .GetCustomAttributes(inherit: true);

      // Assert
      Assert.Contains(attributes, attribute => attribute is AllowAnonymousAttribute);
    }

    [Fact(DisplayName = "Allows anonymous users to get markets")]
    public void GetMarkets_AllowsAnonymousUsers() {
      // Act
      object[] attributes = typeof(MarketGrpcService)
        .GetMethod(nameof(MarketGrpcService.GetMarkets))!
        .GetCustomAttributes(inherit: true);

      // Assert
      Assert.Contains(attributes, attribute => attribute is AllowAnonymousAttribute);
    }

    [Fact(DisplayName = "Keeps shopping service protected for shoppers")]
    public void ShoppingService_RequiresShopperRole() {
      // Act
      AuthorizeAttribute attribute = typeof(ShoppingGrpcService)
        .GetCustomAttributes(inherit: true)
        .OfType<AuthorizeAttribute>()
        .Single();

      // Assert
      Assert.Equal(nameof(Role.Shopper), attribute.Roles);
    }
  }

  public class GetMarketProductsRpc {
    private readonly IProductRepository _productRepository;
    private readonly MarketGrpcService _service;

    public GetMarketProductsRpc() {
      _productRepository = Substitute.For<IProductRepository>();
      _productRepository
        .GetProductsAsync(
          Arg.Any<GetMarketProductsFilter>(),
          Arg.Any<CancellationToken>())
        .Returns(EmptyPagedResult());
      _service = new MarketGrpcService(
        new GetMarketProducts.Handler(_productRepository),
        new GetMarkets.Handler(Substitute.For<IMarketRepository>()));
    }

    private static PagedResult<DomainMarket> EmptyPagedResult() => new([], 0);

    [Fact(DisplayName = "Throws when request contains invalid finite pagination")]
    public async Task Api_Throws_WhenPaginationIsInvalid() {
      var request = new GetMarketProductsRequest { Page = 0, PageSize = 10 };

      // Act
      async Task action() =>
        await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<ArgumentOutOfRangeException>(action);
    }

    [Fact(DisplayName = "Returns market count from handler result")]
    public async Task Api_ReturnsMarketCount_FromHandlerResult() {
      // Arrange
      _productRepository
        .GetProductsAsync(
          Arg.Any<GetMarketProductsFilter>(),
          TestContext.Current.CancellationToken)
        .Returns(new PagedResult<DomainMarket>([
          new DomainMarket("Mercadona", []),
          new DomainMarket("Alcampo", []),
        ], 2));

      // Act
      GetMarketProductsResponse response =
        await _service.GetMarketProducts(new GetMarketProductsRequest(), CreateServerCallContext());

      // Assert
      Assert.Equal(2, response.Markets.Count);
    }

    [Fact(DisplayName = "Maps format quantity and unit of measure to quantity text")]
    public async Task Api_MapsFormatQuantityAndUnitOfMeasure_ToQuantityText() {
      // Arrange
      _productRepository
        .GetProductsAsync(
          Arg.Any<GetMarketProductsFilter>(),
          TestContext.Current.CancellationToken)
        .Returns(new PagedResult<DomainMarket>([
          new DomainMarket("Mercadona", [
            new DomainMarketProduct("Leche", "H", [
              new Metaspesa.Application.Abstractions.Markets.MarketProductFormat(
                new Quantity(1.5m, new UnitOfMeasure("L")),
                new DomainPrice(0.89m),
                null,
                17)
            ])
          ]),
        ], 1));

      // Act
      GetMarketProductsResponse response =
        await _service.GetMarketProducts(new GetMarketProductsRequest(), CreateServerCallContext());

      // Assert
      Assert.Equal("1.5 l", response.Markets.Single().Products.Single().Formats.Single().Quantity);
    }

    [Fact(DisplayName = "Maps product format identifier to catalog response")]
    public async Task Api_MapsProductFormatIdentifier_ToCatalogResponse() {
      // Arrange
      _productRepository
        .GetProductsAsync(
          Arg.Any<GetMarketProductsFilter>(),
          TestContext.Current.CancellationToken)
        .Returns(new PagedResult<DomainMarket>([
          new DomainMarket("Mercadona", [
            new DomainMarketProduct("Leche", "H", [
              new Metaspesa.Application.Abstractions.Markets.MarketProductFormat(
                new Quantity(1.5m, new UnitOfMeasure("L")),
                new DomainPrice(0.89m),
                null,
                17)
            ])
          ]),
        ], 1));

      // Act
      GetMarketProductsResponse response =
        await _service.GetMarketProducts(new GetMarketProductsRequest(), CreateServerCallContext());

      // Assert
      Assert.Equal(
        17,
        response.Markets.Single().Products.Single().Formats.Single().ProductFormatUid);
    }

    [Fact(DisplayName = "Returns total_products from handler result")]
    public async Task Api_ReturnsTotalProducts_FromHandlerResult() {
      // Arrange
      _productRepository
        .GetProductsAsync(
          Arg.Any<GetMarketProductsFilter>(),
          TestContext.Current.CancellationToken)
        .Returns(new PagedResult<DomainMarket>([], 57));

      // Act
      GetMarketProductsResponse response =
        await _service.GetMarketProducts(new GetMarketProductsRequest(), CreateServerCallContext());

      // Assert
      Assert.Equal(57, response.TotalProducts);
    }

    [Fact(DisplayName = "Passes null market_name to filter when not set in request")]
    public async Task Api_PassesNullMarketName_WhenNotSetInRequest() {
      // Act
      await _service.GetMarketProducts(new GetMarketProductsRequest(), CreateServerCallContext());

      // Assert
      await _productRepository.Received(1).GetProductsAsync(
        Arg.Is<GetMarketProductsFilter>(filter => filter.MarketName == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes market_name to filter when set in request")]
    public async Task Api_PassesMarketName_WhenSetInRequest() {
      // Arrange
      var request = new GetMarketProductsRequest { MarketName = "Mercadona" };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _productRepository.Received(1).GetProductsAsync(
        Arg.Is<GetMarketProductsFilter>(filter =>
          filter.MarketName == "Mercadona"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes brand_name_segment to filter when set in request")]
    public async Task Api_PassesBrandNameSegment_WhenSetInRequest() {
      // Arrange
      var request = new GetMarketProductsRequest { BrandNameSegment = "Hacendado" };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _productRepository.Received(1).GetProductsAsync(
        Arg.Is<GetMarketProductsFilter>(filter =>
          filter.BrandNameSegment == "Hacendado"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes name_segment to filter when set in request")]
    public async Task Api_PassesNameSegment_WhenSetInRequest() {
      // Arrange
      var request = new GetMarketProductsRequest { NameSegment = "leche" };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _productRepository.Received(1).GetProductsAsync(
        Arg.Is<GetMarketProductsFilter>(filter =>
          filter.NameSegment == "leche"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes infinite pagination when neither page nor page_size is set")]
    public async Task Api_PassesInfinitePagination_WhenNeitherPageNorSizeSet() {
      // Act
      await _service.GetMarketProducts(new GetMarketProductsRequest(), CreateServerCallContext());

      // Assert
      await _productRepository.Received(1).GetProductsAsync(
        Arg.Is<GetMarketProductsFilter>(filter =>
          filter.Pagination != null && filter.Pagination.IsInfinite),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes infinite pagination when only page is set")]
    public async Task Api_PassesInfinitePagination_WhenOnlyPageSet() {
      var request = new GetMarketProductsRequest { Page = 2 };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _productRepository.Received(1).GetProductsAsync(
        Arg.Is<GetMarketProductsFilter>(filter =>
          filter.Pagination != null && filter.Pagination.IsInfinite),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes infinite pagination when only page_size is set")]
    public async Task Api_PassesInfinitePagination_WhenOnlyPageSizeSet() {
      var request = new GetMarketProductsRequest { PageSize = 15 };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _productRepository.Received(1).GetProductsAsync(
        Arg.Is<GetMarketProductsFilter>(filter =>
          filter.Pagination != null && filter.Pagination.IsInfinite),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes explicit page index to filter when both page and page_size are set")]
    public async Task Api_PassesExplicitPageIndex_WhenBothSet() {
      // Arrange
      var request = new GetMarketProductsRequest { Page = 3, PageSize = 10 };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _productRepository.Received(1).GetProductsAsync(
        Arg.Is<GetMarketProductsFilter>(filter =>
          filter.Pagination!.Index == 3),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes explicit page size to filter when both page and page_size are set")]
    public async Task Api_PassesExplicitPageSize_WhenBothSet() {
      // Arrange
      var request = new GetMarketProductsRequest { Page = 3, PageSize = 10 };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _productRepository.Received(1).GetProductsAsync(
        Arg.Is<GetMarketProductsFilter>(filter =>
          filter.Pagination!.Size == 10),
        TestContext.Current.CancellationToken);
    }
  }

  public class GetMarketsRpc {
    private readonly IMarketRepository _marketRepository;
    private readonly MarketGrpcService _service;

    public GetMarketsRpc() {
      _marketRepository = Substitute.For<IMarketRepository>();
      _marketRepository
        .GetMarketSummariesAsync(Arg.Any<CancellationToken>())
        .Returns([]);
      _service = new MarketGrpcService(
        new GetMarketProducts.Handler(Substitute.For<IProductRepository>()),
        new GetMarkets.Handler(_marketRepository));
    }

    [Fact(DisplayName = "Propagates repository exceptions")]
    public async Task Api_Throws_WhenRepositoryFails() {
      // Arrange
      _marketRepository
        .GetMarketSummariesAsync(TestContext.Current.CancellationToken)
        .Returns<Task<IReadOnlyCollection<DomainMarketSummary>>>(
          _ => throw new InvalidOperationException());

      // Act
      async Task action() => await _service.GetMarkets(new Empty(), CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact(DisplayName = "Returns market count from handler result")]
    public async Task Api_ReturnsMarketCount_FromHandlerResult() {
      // Arrange
      _marketRepository
        .GetMarketSummariesAsync(TestContext.Current.CancellationToken)
        .Returns(new List<DomainMarketSummary> {
          new("Mercadona", new Uri("https://example.com/mercadona.png")),
          new("Alcampo", null),
        });

      // Act
      GetMarketsResponse response = await _service.GetMarkets(new Empty(), CreateServerCallContext());

      // Assert
      Assert.Equal(2, response.Markets.Count);
    }

    [Fact(DisplayName = "Returns market names from handler result")]
    public async Task Api_ReturnsMarketNames_FromHandlerResult() {
      // Arrange
      _marketRepository
        .GetMarketSummariesAsync(TestContext.Current.CancellationToken)
        .Returns(new List<DomainMarketSummary> { new("Mercadona", null) });

      // Act
      GetMarketsResponse response = await _service.GetMarkets(new Empty(), CreateServerCallContext());

      // Assert
      Assert.Equal("Mercadona", response.Markets.Single().Name);
    }
  }

  private static ServerCallContext CreateServerCallContext() => TestServerCallContext.Create(
    method: string.Empty,
    host: string.Empty,
    deadline: DateTime.UtcNow.AddMinutes(1),
    requestHeaders: [],
    cancellationToken: TestContext.Current.CancellationToken,
    peer: string.Empty,
    authContext: null!,
    contextPropagationToken: null!,
    writeHeadersFunc: _ => Task.CompletedTask,
    writeOptionsGetter: () => new WriteOptions(),
    writeOptionsSetter: _ => { });
}