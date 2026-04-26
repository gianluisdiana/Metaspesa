using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Markets;
using Metaspesa.GrpcApi.Protos.Markets;
using Metaspesa.GrpcApi.Services;
using NSubstitute;

namespace Metaspesa.GrpcApi.UnitTests.Markets;

public static class MarketGrpcServiceTests {
  public class AddProductsRpc {
    private readonly ICommandHandler<AddMarketProducts.Command> _useCaseHandler;
    private readonly MarketGrpcService _service;

    public AddProductsRpc() {
      _useCaseHandler = Substitute.For<ICommandHandler<AddMarketProducts.Command>>();
      _service = new MarketGrpcService(
        _useCaseHandler);
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
        }
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
        }
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
        }
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
        Arg.Is<AddMarketProducts.Command>(cmd => cmd.RegisteredAt == DateOnly.FromDateTime(expectedTime)),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes null registered_at when not provided")]
    public async Task Api_PassesNullRegisteredAt_WhenNotProvided() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddMarketProducts.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddProductsRequest {
        Products = {
          new Product { Name = "Milk", Price = 1.99f, MarketName = "Walmart", BrandName = "Nike" }
        }
      };

      // Act
      await _service.AddProducts(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<AddMarketProducts.Command>(cmd => cmd.RegisteredAt == null),
        TestContext.Current.CancellationToken);
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
}
