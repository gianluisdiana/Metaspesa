using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.GrpcApi.Protos.Shopping;
using Metaspesa.GrpcApi.Services;
using NSubstitute;

namespace Metaspesa.GrpcApi.UnitTests.Shopping;

public static class ShoppingGrpcServiceTests {
  public class GetRegisteredProductsRpc {
    private readonly IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<RegisteredItem>> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public GetRegisteredProductsRpc() {
      _useCaseHandler = Substitute.For<
      IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<RegisteredItem>>>();
      service = new ShoppingGrpcService(
        _useCaseHandler,
        Substitute.For<IQueryHandler<GetCurrentShoppingList.Query, Domain.Shopping.ShoppingList>>(),
        Substitute.For<ICommandHandler<RecordShoppingList.Command>>()
      );
    }

    [Fact(DisplayName = "Throws RpcException if the query handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfQueryHandlerFails() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      // Act
      async Task action() => await service.GetRegisteredProducts(
        new Empty(), CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns the registered products if the query handler returns a success result")]
    public async Task Api_ReturnsRegisteredProducts_IfQueryHandlerSucceeds() {
      // Arrange
      var registeredItems = new List<RegisteredItem> {
        new("Product 1", null, 10),
        new("Product 2", "1 litre", 5),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredProductsResponse response = await service.GetRegisteredProducts(
        new Empty(), CreateServerCallContext());

      // Assert
      Assert.Equal(registeredItems.Count, response.Products.Count);
    }

    [Fact(DisplayName = "Maps product name from registered items")]
    public async Task Api_MapsProductName_FromRegisteredItems() {
      // Arrange
      var registeredItems = new List<RegisteredItem> {
        new("Product 1", null, 10),
        new("Product 2", "1 litre", 5),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredProductsResponse response = await service.GetRegisteredProducts(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < registeredItems.Count; i++) {
        Assert.Equal(registeredItems[i].Name, response.Products[i].Name);
      }
    }

    [Fact(DisplayName = "Maps product quantity from registered items")]
    public async Task Api_MapsProductQuantity_FromRegisteredItems() {
      // Arrange
      var registeredItems = new List<RegisteredItem> {
        new("Product 1", "5 pieces", 10),
        new("Product 2", "1 litre", 5),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredProductsResponse response = await service.GetRegisteredProducts(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < registeredItems.Count; i++) {
        Assert.Equal(registeredItems[i].Quantity, response.Products[i].Quantity);
      }
    }

    [Fact(DisplayName = "Maps product empty quantity from registered items if it doesn't have it")]
    public async Task Api_MapsProductEmptyQuantity_FromRegisteredItems_IfItDoesNotHaveIt() {
      // Arrange
      var registeredItems = new List<RegisteredItem> {
        new("Product 1", null, 10),
        new("Product 2", null, 5),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredProductsResponse response = await service.GetRegisteredProducts(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < registeredItems.Count; i++) {
        Assert.Empty(response.Products[i].Quantity);
      }
    }

    [Fact(DisplayName = "Maps product price from registered items")]
    public async Task Api_MapsProductPrice_FromRegisteredItems() {
      // Arrange
      var registeredItems = new List<RegisteredItem> {
        new("Product 1", null, 3),
        new("Product 2", "1 litre", 5),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredProductsResponse response = await service.GetRegisteredProducts(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < registeredItems.Count; i++) {
        Assert.Equal(registeredItems[i].LastPrice, response.Products[i].Price);
      }
    }

    [Fact(DisplayName = "Maps product price to 0 if the last price is null in registered items")]
    public async Task Api_MapsProductPriceToZero_IfLastPriceIsNullInRegisteredItems() {
      // Arrange
      var registeredItems = new List<RegisteredItem> {
        new("Product 1", null, null),
        new("Product 2", "1 litre", null),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredProductsResponse response = await service.GetRegisteredProducts(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < registeredItems.Count; i++) {
        Assert.Equal(0, response.Products[i].Price);
      }
    }

    [Fact(DisplayName = "Maps product not checked in the response")]
    public async Task Api_MapsProductCheckedToFalse_InResponse() {
      // Arrange
      var registeredItems = new List<RegisteredItem> {
        new("Product 1", null, 3),
        new("Product 2", "1 litre", 5),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredProductsResponse response = await service.GetRegisteredProducts(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < registeredItems.Count; i++) {
        Assert.False(response.Products[i].Checked);
      }
    }
  }

  public class GetCurrentShoppingListRpc {
    private readonly IQueryHandler<GetCurrentShoppingList.Query, Domain.Shopping.ShoppingList> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public GetCurrentShoppingListRpc() {
      _useCaseHandler = Substitute.For<
      IQueryHandler<GetCurrentShoppingList.Query, Domain.Shopping.ShoppingList>>();
      service = new ShoppingGrpcService(
        Substitute.For<IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<RegisteredItem>>>(),
        _useCaseHandler,
        Substitute.For<ICommandHandler<RecordShoppingList.Command>>()
      );
    }

    [Fact(DisplayName = "Throws RpcException if the query handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfQueryHandlerFails() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      // Act
      async Task action() => await service.GetCurrentShoppingList(
        new Empty(), CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns the current shopping list if the query handler returns a success result")]
    public async Task Api_ReturnsCurrentShoppingList_IfQueryHandlerSucceeds() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", null, 3, true),
          new("Product 2", null, null, false),
        ]);

      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      CurrentShoppingList response = await service.GetCurrentShoppingList(
        new Empty(), CreateServerCallContext());

      // Assert
      Assert.Equal(shoppingList.Name, response.ShoppingList.Name);
    }

    [Fact(DisplayName = "Maps shopping list items to products in the response")]
    public async Task Api_MapsShoppingListItems_ToProductsInResponse() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", null, 3, true),
          new("Product 2", null, null, false),
        ]);
      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      CurrentShoppingList response = await service.GetCurrentShoppingList(
        new Empty(), CreateServerCallContext());

      // Assert
      Assert.Equal(shoppingList.Items.Count, response.ShoppingList.Products.Count);
    }

    [Fact(DisplayName = "Maps shopping list name to product name in the response")]
    public async Task Api_MapsShoppingListName_ToProductNameInResponse() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", null, 3, true),
          new("Product 2", null, null, false),
        ]);

      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      CurrentShoppingList response = await service.GetCurrentShoppingList(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < shoppingList.Items.Count; i++) {
        Assert.Equal(
          shoppingList.Items.ElementAt(i).Name,
          response.ShoppingList.Products[i].Name);
      }
    }

    [Fact(DisplayName = "Maps shopping list item quantity to product quantity in the response")]
    public async Task Api_MapsShoppingListItemQuantity_ToProductQuantityInResponse() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", "1 litre", 3, true),
          new("Product 2", "2 kg", null, false),
        ]);

      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      CurrentShoppingList response = await service.GetCurrentShoppingList(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < shoppingList.Items.Count; i++) {
        Assert.Equal(
          shoppingList.Items.ElementAt(i).Quantity,
          response.ShoppingList.Products[i].Quantity);
      }
    }

    [Fact(DisplayName = "Maps empty quantity to product quantity if it doesn't have it")]
    public async Task Api_MapsEmptyQuantity_ToProductQuantityIfItDoesNotHaveIt() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", null, 3, true),
          new("Product 2", null, null, false),
        ]);

      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      CurrentShoppingList response = await service.GetCurrentShoppingList(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < shoppingList.Items.Count; i++) {
        Assert.Empty(response.ShoppingList.Products[i].Quantity);
      }
    }

    [Fact(DisplayName = "Maps shopping list item price to product price in the response")]
    public async Task Api_MapsShoppingListItemPrice_ToProductPriceInResponse() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", null, 3, true),
          new("Product 2", null, 10.3f, false),
        ]);

      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      CurrentShoppingList response = await service.GetCurrentShoppingList(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < shoppingList.Items.Count; i++) {
        Assert.Equal(
          shoppingList.Items.ElementAt(i).Price,
          response.ShoppingList.Products[i].Price);
      }
    }

    [Fact(DisplayName = "Maps product price to 0 if the last price is null in shopping list items")]
    public async Task Api_MapsProductPriceToZero_IfLastPriceIsNullInShoppingListItems() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", null, null, true),
          new("Product 2", null, null, false),
        ]);

      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      CurrentShoppingList response = await service.GetCurrentShoppingList(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < shoppingList.Items.Count; i++) {
        Assert.Equal(0f, response.ShoppingList.Products[i].Price);
      }
    }

    [Fact(DisplayName = "Maps shopping list item checked state to product checked state in the response")]
    public async Task Api_MapsShoppingListItemCheckedState_ToProductCheckedStateInResponse() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", null, 3, true),
          new("Product 2", null, null, false),
        ]);

      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      CurrentShoppingList response = await service.GetCurrentShoppingList(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < shoppingList.Items.Count; i++) {
        Assert.Equal(
          shoppingList.Items.ElementAt(i).IsChecked,
          response.ShoppingList.Products[i].Checked);
      }
    }
  }

  public class RecordShoppingListRpc {
    private readonly ICommandHandler<RecordShoppingList.Command> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public RecordShoppingListRpc() {
      _useCaseHandler = Substitute.For<ICommandHandler<RecordShoppingList.Command>>();
      service = new ShoppingGrpcService(
        Substitute.For<IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<RegisteredItem>>>(),
        Substitute.For<IQueryHandler<GetCurrentShoppingList.Query, Domain.Shopping.ShoppingList>>(),
        _useCaseHandler
      );
    }

    [Fact(DisplayName = "Throws RpcException if the command handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfCommandHandlerFails() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<RecordShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList {
          Name = "Weekly Groceries",
          Products = { }
        }
      };

      // Act
      async Task action() => await service.RecordShoppingList(
        request, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns empty if the command handler returns a success result")]
    public async Task Api_ReturnsEmpty_IfCommandHandlerSucceeds() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<RecordShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList {
          Name = "Weekly Groceries",
          Products = { }
        }
      };

      // Act
      Empty response = await service.RecordShoppingList(
        request, CreateServerCallContext());

      // Assert
      Assert.NotNull(response);
    }

    [Fact(DisplayName = "Maps product name from request to shopping list item in the command")]
    public async Task Api_MapsProductName_FromRequestToShoppingListItemInCommand() {
      // Arrange
      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList {
          Name = "Weekly Groceries",
          Products = {
            new Protos.Shopping.Product {
              Name = "Product 1",
              Quantity = "1 litre",
              Price = 3,
              Checked = true,
            },
            new Protos.Shopping.Product {
              Name = "Product 2",
              Quantity = "2 kg",
              Price = 10.3f,
              Checked = false,
            }
          }
        }
      };

      _useCaseHandler
        .Handle(Arg.Any<RecordShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      // Act
      await service.RecordShoppingList(
        request, CreateServerCallContext());

      // Assert
      for (int i = 0; i < request.ShoppingList.Products.Count; i++) {
        await _useCaseHandler.Received(1).Handle(
          Arg.Is<RecordShoppingList.Command>(cmd =>
            cmd.ShoppingList.Items.ElementAt(i).Name ==
            request.ShoppingList.Products[i].Name),
          TestContext.Current.CancellationToken);
      }
    }

    [Fact(DisplayName = "Maps product quantity from request to shopping list item in the command")]
    public async Task Api_MapsProductQuantity_FromRequestToShoppingListItemInCommand() {
      // Arrange
      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList {
          Name = "Weekly Groceries",
          Products = {
            new Protos.Shopping.Product {
              Name = "Product 1",
              Quantity = "1 litre",
              Price = 3,
              Checked = true,
            },
            new Protos.Shopping.Product {
              Name = "Product 2",
              Quantity = "2 kg",
              Price = 10.3f,
              Checked = false,
            }
          }
        }
      };

      _useCaseHandler
        .Handle(Arg.Any<RecordShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      // Act
      await service.RecordShoppingList(request, CreateServerCallContext());

      // Assert
      for (int i = 0; i < request.ShoppingList.Products.Count; i++) {
        await _useCaseHandler.Received(1).Handle(
          Arg.Is<RecordShoppingList.Command>(cmd =>
            cmd.ShoppingList.Items.ElementAt(i).Quantity ==
            request.ShoppingList.Products[i].Quantity),
          TestContext.Current.CancellationToken);
      }
    }


    [Fact(DisplayName = "Maps product price from request to shopping list item in the command")]
    public async Task Api_MapsProductPrice_FromRequestToShoppingListItemInCommand() {
      // Arrange
      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList {
          Name = "Weekly Groceries",
          Products = {
            new Protos.Shopping.Product {
              Name = "Product 1",
              Quantity = "1 litre",
              Price = 3,
              Checked = true,
            },
            new Protos.Shopping.Product {
              Name = "Product 2",
              Quantity = "2 kg",
              Price = 10.3f,
              Checked = false,
            }
          }
        }
      };

      _useCaseHandler
        .Handle(Arg.Any<RecordShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      // Act
      await service.RecordShoppingList(request, CreateServerCallContext());

      // Assert
      const float Epsilon = 0.01f;
      for (int i = 0; i < request.ShoppingList.Products.Count; i++) {
        float price = request.ShoppingList.Products[i].Price;
        await _useCaseHandler.Received(1).Handle(
          Arg.Is<RecordShoppingList.Command>(cmd => Math.Abs(
            cmd.ShoppingList.Items.ElementAt(i).Price!.Value - price
          ) < Epsilon),
          TestContext.Current.CancellationToken);
      }
    }

    [Theory(DisplayName = "Maps null price if the product price is close to 0")]
    [InlineData(0f)]
    [InlineData(0.009f)]
    [InlineData(-0.009f)]
    public async Task Api_MapsNullPrice_IfProductPriceIsCloseToZero(float price) {
      // Arrange
      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList {
          Name = "Weekly Groceries",
          Products = {
            new Protos.Shopping.Product {
              Name = "Product 1",
              Quantity = "1 litre",
              Price = price,
              Checked = true,
            },
          }
        }
      };

      _useCaseHandler
        .Handle(Arg.Any<RecordShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      // Act
      await service.RecordShoppingList(request, CreateServerCallContext());

      // Assert
      for (int i = 0; i < request.ShoppingList.Products.Count; i++) {
        await _useCaseHandler.Received(1).Handle(
          Arg.Is<RecordShoppingList.Command>(cmd =>
            cmd.ShoppingList.Items.ElementAt(i).Price == null),
          TestContext.Current.CancellationToken);
      }
    }

    [Fact(DisplayName = "Maps product checked state from request to shopping list item in the command")]
    public async Task Api_MapsProductCheckedState_FromRequestToShoppingListItemInCommand() {
      // Arrange
      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList {
          Name = "Weekly Groceries",
          Products = {
            new Protos.Shopping.Product {
              Name = "Product 1",
              Quantity = "1 litre",
              Price = 3,
              Checked = true,
            },
            new Protos.Shopping.Product {
              Name = "Product 2",
              Quantity = "2 kg",
              Price = 10.3f,
              Checked = false,
            }
          }
        }
      };

      _useCaseHandler
        .Handle(Arg.Any<RecordShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      // Act
      await service.RecordShoppingList(request, CreateServerCallContext());

      // Assert
      for (int i = 0; i < request.ShoppingList.Products.Count; i++) {
        await _useCaseHandler.Received(1).Handle(
          Arg.Is<RecordShoppingList.Command>(cmd =>
            cmd.ShoppingList.Items.ElementAt(i).IsChecked ==
            request.ShoppingList.Products[i].Checked),
          TestContext.Current.CancellationToken);
      }
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
