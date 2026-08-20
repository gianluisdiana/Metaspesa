using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Markets;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets.Errors;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.GrpcApi.Protos.Markets;
using Metaspesa.GrpcApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    [Fact(DisplayName = "Requires product manager role to add products")]
    public void AddProducts_RequiresProductManagerRole() {
      // Act
      AuthorizeAttribute attribute = typeof(MarketGrpcService)
        .GetMethod(nameof(MarketGrpcService.AddProducts))!
        .GetCustomAttributes(inherit: true)
        .OfType<AuthorizeAttribute>()
        .Single();

      // Assert
      Assert.Equal(nameof(Role.ProductManager), attribute.Roles);
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

  public class AddProductsRpc {
    private readonly IMarketRepository _marketRepository;
    private readonly IProductRepository _productRepository;
    private readonly MarketGrpcService _service;

    public AddProductsRpc() {
      _marketRepository = Substitute.For<IMarketRepository>();
      _productRepository = Substitute.For<IProductRepository>();
      IPriceSnapshotRepository snapshotRepository =
        Substitute.For<IPriceSnapshotRepository>();
      _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
        .Returns([]);
      _marketRepository
        .CheckUnitOfMeasureIsSupportedAsync(
          Arg.Any<string>(),
          Arg.Any<CancellationToken>())
        .Returns(true);
      _productRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
        .Returns([]);
      _productRepository.ResolveProductsAsync(
          Arg.Any<MarketImport>(),
          Arg.Any<DateTime>(),
          Arg.Any<CancellationToken>())
        .Returns(new ProductImportResult([], [], []));
      IServiceScopeFactory scopeFactory = new ServiceCollection()
        .AddSingleton(_marketRepository)
        .AddSingleton(_productRepository)
        .AddSingleton(snapshotRepository)
        .BuildServiceProvider()
        .GetRequiredService<IServiceScopeFactory>();
      _service = new MarketGrpcService(
        new AddMarketProducts.Handler(
          _marketRepository,
          _productRepository,
          snapshotRepository,
          scopeFactory,
          Substitute.For<ILogger<AddMarketProducts.Handler>>()),
        new GetMarketProducts.Handler(Substitute.For<IProductRepository>()),
        new GetMarkets.Handler(Substitute.For<IMarketRepository>()));
    }

    [Fact(DisplayName = "Propagates domain exception from command handler")]
    public async Task Api_ThrowsDomainException_IfCommandIsInvalid() {
      var request = new AddProductsRequest {
        RegisteredAt = Timestamp.FromDateTime(DateTime.UtcNow),
      };

      // Act
      async Task action() => await _service.AddProducts(request, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<EmptyMarketProductsException>(action);
    }

    [Fact(DisplayName = "Returns empty when handler succeeds")]
    public async Task Api_ReturnsEmpty_WhenHandlerSucceeds() {
      // Arrange
      var request = new AddProductsRequest {
        Products = {
          new Product {
            Name = "Milk", Price = "1.99", Quantity = 1, UnitOfMeasure = "L",
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
      var request = new AddProductsRequest {
        Products = {
          new Product {
            Name = "Milk", Price = "1.99", Quantity = 1, UnitOfMeasure = "L",
            MarketName = "Walmart", BrandName = "Nike"
          },
          new Product {
            Name = "Bread", Price = "0.99", Quantity = 500, UnitOfMeasure = "g",
            MarketName = "Carrefour", BrandName = "Adidas"
          },
        },
        RegisteredAt = Timestamp.FromDateTime(DateTime.UtcNow),
      };

      // Act
      await _service.AddProducts(request, CreateServerCallContext());

      // Assert
      await _productRepository.Received(2).ResolveProductsAsync(
        Arg.Any<MarketImport>(),
        Arg.Any<DateTime>(),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Sanitizes non-ASCII product text before creating command")]
    public async Task Api_SanitizesNonAsciiProductText_BeforeCreatingCommand() {
      // Arrange
      var request = new AddProductsRequest {
        Products = {
          new Product {
            Name = "Caf\u00e9 \u2615",
            Price = "1.99",
            Quantity = 500,
            UnitOfMeasure = "g \u2713",
            MarketName = "Mercad\u00f3na",
            BrandName = "Ni\u00f1o",
          }
        },
        RegisteredAt = Timestamp.FromDateTime(DateTime.UtcNow),
      };

      // Act
      await _service.AddProducts(request, CreateServerCallContext());

      // Assert
      await _productRepository.Received(1).ResolveProductsAsync(
        Arg.Is<MarketImport>(market =>
          market.Name.Value == "Mercadona" &&
          market.Products.Single().Name.Value == "Cafe" &&
          market.Products.Single().Brand.Value == "Nino" &&
          market.Products.Single().Formats.Single()
            .Quantity.UnitOfMeasure.Value == "g"),
        Arg.Any<DateTime>(),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps quantity and unit of measure from request to command")]
    public async Task Api_MapsQuantityAndUnitOfMeasure_FromRequestToCommand() {
      // Arrange
      var request = new AddProductsRequest {
        Products = {
          new Product {
            Name = "Milk", Price = "1.99", Quantity = 1.5F, UnitOfMeasure = "L",
            MarketName = "Walmart", BrandName = "Nike"
          }
        },
        RegisteredAt = Timestamp.FromDateTime(DateTime.UtcNow),
      };

      // Act
      await _service.AddProducts(request, CreateServerCallContext());

      // Assert
      await _productRepository.Received(1).ResolveProductsAsync(
        Arg.Is<MarketImport>(market =>
          market.Products.Single().Formats.Single().Quantity.Amount == 1.5m &&
          market.Products.Single().Formats.Single()
            .Quantity.UnitOfMeasure.Value == "l"),
        Arg.Any<DateTime>(),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps registered_at from request when provided")]
    public async Task Api_MapsRegisteredAt_WhenProvided() {
      // Arrange
      var expectedTime = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
      var request = new AddProductsRequest {
        Products = {
          new Product {
            Name = "Milk", Price = "1.99", Quantity = 1, UnitOfMeasure = "L",
            MarketName = "Walmart", BrandName = "Nike"
          }
        },
        RegisteredAt = Timestamp.FromDateTime(expectedTime),
      };

      // Act
      await _service.AddProducts(request, CreateServerCallContext());

      // Assert
      await _productRepository.Received(1).ResolveProductsAsync(
        Arg.Any<MarketImport>(),
        Arg.Is<DateTime>(observedAt =>
          observedAt == DateOnly.FromDateTime(expectedTime).ToDateTime(
            TimeOnly.MinValue,
            DateTimeKind.Utc)),
        TestContext.Current.CancellationToken);
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
        CreateUnusedAddProductsHandler(),
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
        CreateUnusedAddProductsHandler(),
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

  private static AddMarketProducts.Handler CreateUnusedAddProductsHandler() {
    IMarketRepository marketRepository = Substitute.For<IMarketRepository>();
    IProductRepository productRepository = Substitute.For<IProductRepository>();
    IPriceSnapshotRepository snapshotRepository =
      Substitute.For<IPriceSnapshotRepository>();
    IServiceScopeFactory scopeFactory = new ServiceCollection()
      .AddSingleton(marketRepository)
      .AddSingleton(productRepository)
      .AddSingleton(snapshotRepository)
      .BuildServiceProvider()
      .GetRequiredService<IServiceScopeFactory>();

    return new AddMarketProducts.Handler(
      marketRepository,
      productRepository,
      snapshotRepository,
      scopeFactory,
      Substitute.For<ILogger<AddMarketProducts.Handler>>());
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