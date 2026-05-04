using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Markets;
using Metaspesa.Domain.Markets;
using Metaspesa.GrpcApi.Protos.Markets;
using Metaspesa.GrpcApi.Services;
using NSubstitute;
using DomainMarket = Metaspesa.Domain.Markets.Market;
using DomainMarketProduct = Metaspesa.Domain.Markets.MarketProduct;
using DomainMarketSummary = Metaspesa.Domain.Markets.MarketSummary;
using DomainPrice = Metaspesa.Domain.Shopping.Price;

namespace Metaspesa.GrpcApi.UnitTests.Markets;

public static class MarketGrpcServiceTests {
  public class AddProductsRpc {
    private readonly ICommandHandler<AddMarketProducts.Command> _useCaseHandler;
    private readonly MarketGrpcService _service;

    public AddProductsRpc() {
      _useCaseHandler = Substitute.For<ICommandHandler<AddMarketProducts.Command>>();
      _service = new MarketGrpcService(
        _useCaseHandler,
        Substitute.For<IQueryHandler<GetMarketProducts.Query, PagedResult<DomainMarket>>>(),
        Substitute.For<IQueryHandler<GetMarkets.Query, IReadOnlyCollection<DomainMarketSummary>>>());
    }

    [Fact(DisplayName = "Throws RpcException if the command handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfCommandHandlerFails() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddMarketProducts.Command>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      var request = new AddProductsRequest {
        Products = {
          new Product {
            Name = "Milk", Price = 1.99f, Quantity = "1L",
            MarketName = "Walmart", BrandName = "Nike"
          }
        },
        RegisteredAt = Timestamp.FromDateTime(DateTime.UtcNow),
      };

      // Act
      async Task action() => await _service.AddProducts(request, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns empty when handler succeeds")]
    public async Task Api_ReturnsEmpty_WhenHandlerSucceeds() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddMarketProducts.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddProductsRequest {
        Products = {
          new Product {
            Name = "Milk", Price = 1.99f, Quantity = "1L",
            MarketName = "Walmart", BrandName = "Nike"
          }
        },
        RegisteredAt = Timestamp.FromDateTime(DateTime.UtcNow),
      };

      // Act
      Empty response = await _service.AddProducts(request, CreateServerCallContext());

      // Assert
      Assert.NotNull(response);
    }

    [Fact(DisplayName = "Maps products count from request to command")]
    public async Task Api_MapsProductsCount_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddMarketProducts.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddProductsRequest {
        Products = {
          new Product { Name = "Milk", Price = 1.99f, MarketName = "Walmart", BrandName = "Nike" },
          new Product { Name = "Bread", Price = 0.99f, MarketName = "Carrefour", BrandName = "Adidas" },
        },
        RegisteredAt = Timestamp.FromDateTime(DateTime.UtcNow),
      };

      // Act
      await _service.AddProducts(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<AddMarketProducts.Command>(cmd => cmd.Products.Count == 2),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps registered_at from request when provided")]
    public async Task Api_MapsRegisteredAt_WhenProvided() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddMarketProducts.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var expectedTime = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
      var request = new AddProductsRequest {
        Products = {
          new Product { Name = "Milk", Price = 1.99f, MarketName = "Walmart", BrandName = "Nike" }
        },
        RegisteredAt = Timestamp.FromDateTime(expectedTime),
      };

      // Act
      await _service.AddProducts(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<AddMarketProducts.Command>(cmd =>
          cmd.RegisteredAt == DateOnly.FromDateTime(expectedTime)),
        TestContext.Current.CancellationToken);
    }
  }

  public class GetMarketProductsRpc {
    private readonly IQueryHandler<GetMarketProducts.Query, PagedResult<DomainMarket>> _useCaseHandler;
    private readonly MarketGrpcService _service;

    public GetMarketProductsRpc() {
      _useCaseHandler = Substitute.For<IQueryHandler<GetMarketProducts.Query, PagedResult<DomainMarket>>>();
      _service = new MarketGrpcService(
        Substitute.For<ICommandHandler<AddMarketProducts.Command>>(),
        _useCaseHandler,
        Substitute.For<IQueryHandler<GetMarkets.Query, IReadOnlyCollection<DomainMarketSummary>>>());
    }

    private static PagedResult<DomainMarket> EmptyPagedResult() => new([], 0);

    [Fact(DisplayName = "Throws RpcException if the query handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfQueryHandlerFails() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarketProducts.Query>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      // Act
      async Task action() =>
        await _service.GetMarketProducts(new GetMarketProductsRequest(), CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns market count from handler result")]
    public async Task Api_ReturnsMarketCount_FromHandlerResult() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarketProducts.Query>(), TestContext.Current.CancellationToken)
        .Returns(new PagedResult<DomainMarket>([
          new DomainMarket("Mercadona", [new DomainMarketProduct("Leche", new ProductBrand("H"), [new ProductFormat("1L", new DomainPrice(0.89f))])]),
          new DomainMarket("Alcampo", [new DomainMarketProduct("Pan", new ProductBrand("B"), [new ProductFormat("500g", new DomainPrice(1.20f))])]),
        ], 2));

      // Act
      GetMarketProductsResponse response =
        await _service.GetMarketProducts(new GetMarketProductsRequest(), CreateServerCallContext());

      // Assert
      Assert.Equal(2, response.Markets.Count);
    }

    [Fact(DisplayName = "Returns total_products from handler result")]
    public async Task Api_ReturnsTotalProducts_FromHandlerResult() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarketProducts.Query>(), TestContext.Current.CancellationToken)
        .Returns(new PagedResult<DomainMarket>([], 57));

      // Act
      GetMarketProductsResponse response =
        await _service.GetMarketProducts(new GetMarketProductsRequest(), CreateServerCallContext());

      // Assert
      Assert.Equal(57, response.TotalProducts);
    }

    [Fact(DisplayName = "Passes null market_name to filter when not set in request")]
    public async Task Api_PassesNullMarketName_WhenNotSetInRequest() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarketProducts.Query>(), TestContext.Current.CancellationToken)
        .Returns(EmptyPagedResult());

      // Act
      await _service.GetMarketProducts(new GetMarketProductsRequest(), CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetMarketProducts.Query>(q => q.Filter.MarketName == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes market_name to filter when set in request")]
    public async Task Api_PassesMarketName_WhenSetInRequest() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarketProducts.Query>(), TestContext.Current.CancellationToken)
        .Returns(EmptyPagedResult());
      var request = new GetMarketProductsRequest { MarketName = "Mercadona" };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetMarketProducts.Query>(q => q.Filter.MarketName == "Mercadona"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes brand_name to filter when set in request")]
    public async Task Api_PassesBrandName_WhenSetInRequest() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarketProducts.Query>(), TestContext.Current.CancellationToken)
        .Returns(EmptyPagedResult());
      var request = new GetMarketProductsRequest { BrandName = "Hacendado" };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetMarketProducts.Query>(q => q.Filter.BrandName == "Hacendado"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes name_segment to filter when set in request")]
    public async Task Api_PassesNameSegment_WhenSetInRequest() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarketProducts.Query>(), TestContext.Current.CancellationToken)
        .Returns(EmptyPagedResult());
      var request = new GetMarketProductsRequest { NameSegment = "leche" };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetMarketProducts.Query>(q => q.Filter.NameSegment == "leche"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes null pagination when neither page nor page_size is set")]
    public async Task Api_PassesNullPagination_WhenNeitherPageNorSizeSet() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarketProducts.Query>(), TestContext.Current.CancellationToken)
        .Returns(EmptyPagedResult());

      // Act
      await _service.GetMarketProducts(new GetMarketProductsRequest(), CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetMarketProducts.Query>(q => q.Filter.Pagination == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes null pagination when only page is set")]
    public async Task Api_PassesNullPagination_WhenOnlyPageSet() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarketProducts.Query>(), TestContext.Current.CancellationToken)
        .Returns(EmptyPagedResult());
      var request = new GetMarketProductsRequest { Page = 2 };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetMarketProducts.Query>(q => q.Filter.Pagination == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes null pagination when only page_size is set")]
    public async Task Api_PassesNullPagination_WhenOnlyPageSizeSet() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarketProducts.Query>(), TestContext.Current.CancellationToken)
        .Returns(EmptyPagedResult());
      var request = new GetMarketProductsRequest { PageSize = 15 };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetMarketProducts.Query>(q => q.Filter.Pagination == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes explicit page index to filter when both page and page_size are set")]
    public async Task Api_PassesExplicitPageIndex_WhenBothSet() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarketProducts.Query>(), TestContext.Current.CancellationToken)
        .Returns(EmptyPagedResult());
      var request = new GetMarketProductsRequest { Page = 3, PageSize = 10 };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetMarketProducts.Query>(q => q.Filter.Pagination!.Index == 3),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes explicit page size to filter when both page and page_size are set")]
    public async Task Api_PassesExplicitPageSize_WhenBothSet() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarketProducts.Query>(), TestContext.Current.CancellationToken)
        .Returns(EmptyPagedResult());
      var request = new GetMarketProductsRequest { Page = 3, PageSize = 10 };

      // Act
      await _service.GetMarketProducts(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetMarketProducts.Query>(q => q.Filter.Pagination!.Size == 10),
        TestContext.Current.CancellationToken);
    }
  }

  public class GetMarketsRpc {
    private readonly IQueryHandler<GetMarkets.Query, IReadOnlyCollection<DomainMarketSummary>> _useCaseHandler;
    private readonly MarketGrpcService _service;

    public GetMarketsRpc() {
      _useCaseHandler = Substitute.For<IQueryHandler<GetMarkets.Query, IReadOnlyCollection<DomainMarketSummary>>>();
      _service = new MarketGrpcService(
        Substitute.For<ICommandHandler<AddMarketProducts.Command>>(),
        Substitute.For<IQueryHandler<GetMarketProducts.Query, PagedResult<DomainMarket>>>(),
        _useCaseHandler);
    }

    [Fact(DisplayName = "Throws RpcException if the query handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfQueryHandlerFails() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarkets.Query>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      // Act
      async Task action() => await _service.GetMarkets(new Empty(), CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns market count from handler result")]
    public async Task Api_ReturnsMarketCount_FromHandlerResult() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetMarkets.Query>(), TestContext.Current.CancellationToken)
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
      _useCaseHandler
        .Handle(Arg.Any<GetMarkets.Query>(), TestContext.Current.CancellationToken)
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
