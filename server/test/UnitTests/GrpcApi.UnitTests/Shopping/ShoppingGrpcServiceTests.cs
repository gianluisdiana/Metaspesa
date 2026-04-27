using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.GrpcApi.Protos.Shopping;
using Metaspesa.GrpcApi.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Product = Metaspesa.Domain.Shopping.Product;

namespace Metaspesa.GrpcApi.UnitTests.Shopping;

public static class ShoppingGrpcServiceTests {
  public class GetRegisteredItemsRpc {
    private readonly IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<Product>> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public GetRegisteredItemsRpc() {
      _useCaseHandler = Substitute.For<
      IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<Product>>>();
      service = new ShoppingGrpcService(
        _useCaseHandler,
        Substitute.For<IQueryHandler<GetCurrentShoppingList.Query, Domain.Shopping.ShoppingList>>(),
        Substitute.For<ICommandHandler<RecordShoppingList.Command>>(),
        Substitute.For<ICommandHandler<CreateShoppingList.Command>>(),
        Substitute.For<ICommandHandler<AddItemsToList.Command>>(),
        Substitute.For<ICommandHandler<UpdateItem.Command>>(),
        Substitute.For<ICommandHandler<RemoveItem.Command>>()
      );
    }

    [Fact(DisplayName = "Throws RpcException if the query handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfQueryHandlerFails() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      // Act
      async Task action() => await service.GetRegisteredItems(
        new Empty(), CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns the registered products if the query handler returns a success result")]
    public async Task Api_ReturnsRegisteredProducts_IfQueryHandlerSucceeds() {
      // Arrange
      var registeredItems = new List<Product> {
        new("Product 1", null, new Price(10)),
        new("Product 2", "1 litre", new Price(5)),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredItemsResponse response = await service.GetRegisteredItems(
        new Empty(), CreateServerCallContext());

      // Assert
      Assert.Equal(registeredItems.Count, response.Items.Count);
    }

    [Fact(DisplayName = "Maps product name from registered items")]
    public async Task Api_MapsProductName_FromRegisteredItems() {
      // Arrange
      var registeredItems = new List<Product> {
        new("Product 1", null, new Price(10)),
        new("Product 2", "1 litre", new Price(5)),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredItemsResponse response = await service.GetRegisteredItems(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < registeredItems.Count; i++) {
        Assert.Equal(registeredItems[i].Name, response.Items[i].Name);
      }
    }

    [Fact(DisplayName = "Maps product quantity from registered items")]
    public async Task Api_MapsProductQuantity_FromRegisteredItems() {
      // Arrange
      var registeredItems = new List<Product> {
        new("Product 1", "5 pieces", new Price(10)),
        new("Product 2", "1 litre", new Price(5)),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredItemsResponse response = await service.GetRegisteredItems(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < registeredItems.Count; i++) {
        Assert.Equal(registeredItems[i].Quantity, response.Items[i].Quantity);
      }
    }

    [Fact(DisplayName = "Maps product empty quantity from registered items if it doesn't have it")]
    public async Task Api_MapsProductEmptyQuantity_FromRegisteredItems_IfItDoesNotHaveIt() {
      // Arrange
      var registeredItems = new List<Product> {
        new("Product 1", null, new Price(10)),
        new("Product 2", null, new Price(5)),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredItemsResponse response = await service.GetRegisteredItems(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < registeredItems.Count; i++) {
        Assert.Empty(response.Items[i].Quantity);
      }
    }

    [Fact(DisplayName = "Maps product price from registered items")]
    public async Task Api_MapsProductPrice_FromRegisteredItems() {
      // Arrange
      var registeredItems = new List<Product> {
        new("Product 1", null, new Price(3)),
        new("Product 2", "1 litre", new Price(5)),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredItemsResponse response = await service.GetRegisteredItems(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < registeredItems.Count; i++) {
        Assert.Equal(registeredItems[i].Price.Value, response.Items[i].Price);
      }
    }

    [Fact(DisplayName = "Maps product price to 0 if the last price is null in registered items")]
    public async Task Api_MapsProductPriceToZero_IfLastPriceIsNullInRegisteredItems() {
      // Arrange
      var registeredItems = new List<Product> {
        new("Product 1", null, Price.Empty),
        new("Product 2", "1 litre", Price.Empty),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredItemsResponse response = await service.GetRegisteredItems(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < registeredItems.Count; i++) {
        Assert.Equal(0, response.Items[i].Price);
      }
    }

    [Fact(DisplayName = "Maps product not checked in the response")]
    public async Task Api_MapsProductCheckedToFalse_InResponse() {
      // Arrange
      var registeredItems = new List<Product> {
        new("Product 1", null, new Price(3)),
        new("Product 2", "1 litre", new Price(5)),
      };
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(registeredItems);

      // Act
      RegisteredItemsResponse response = await service.GetRegisteredItems(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < registeredItems.Count; i++) {
        Assert.False(response.Items[i].Checked);
      }
    }

    [Fact(DisplayName = "Passes user UID from JWT claim to query")]
    public async Task Api_PassesUserUidFromClaim_ToQuery() {
      // Arrange
      var expectedUid = Guid.CreateVersion7();
      _useCaseHandler
        .Handle(Arg.Any<GetRegisteredItems.Query>(), TestContext.Current.CancellationToken)
        .Returns(new List<Product>());

      // Act
      await service.GetRegisteredItems(new Empty(), CreateServerCallContext(expectedUid));

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetRegisteredItems.Query>(q => q.UserUid == expectedUid),
        TestContext.Current.CancellationToken);
    }
  }

  public class GetCurrentShoppingListRpc {
    private readonly IQueryHandler<GetCurrentShoppingList.Query, Domain.Shopping.ShoppingList> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public GetCurrentShoppingListRpc() {
      _useCaseHandler = Substitute.For<
      IQueryHandler<GetCurrentShoppingList.Query, Domain.Shopping.ShoppingList>>();
      service = new ShoppingGrpcService(
        Substitute.For<IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<Product>>>(),
        _useCaseHandler,
        Substitute.For<ICommandHandler<RecordShoppingList.Command>>(),
        Substitute.For<ICommandHandler<CreateShoppingList.Command>>(),
        Substitute.For<ICommandHandler<AddItemsToList.Command>>(),
        Substitute.For<ICommandHandler<UpdateItem.Command>>(),
        Substitute.For<ICommandHandler<RemoveItem.Command>>()
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
          new("Product 1", null, new Price(3), true),
          new("Product 2", null, Price.Empty, false),
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
          new("Product 1", null, new Price(3), true),
          new("Product 2", null, Price.Empty, false),
        ]);
      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      CurrentShoppingList response = await service.GetCurrentShoppingList(
        new Empty(), CreateServerCallContext());

      // Assert
      Assert.Equal(shoppingList.Items.Count, response.ShoppingList.Items.Count);
    }

    [Fact(DisplayName = "Maps shopping list name to product name in the response")]
    public async Task Api_MapsShoppingListName_ToProductNameInResponse() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", null, new Price(3), true),
          new("Product 2", null, Price.Empty, false),
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
          response.ShoppingList.Items[i].Name);
      }
    }

    [Fact(DisplayName = "Maps shopping list item quantity to product quantity in the response")]
    public async Task Api_MapsShoppingListItemQuantity_ToProductQuantityInResponse() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", "1 litre", new Price(3), true),
          new("Product 2", "2 kg", Price.Empty, false),
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
          response.ShoppingList.Items[i].Quantity);
      }
    }

    [Fact(DisplayName = "Maps empty quantity to product quantity if it doesn't have it")]
    public async Task Api_MapsEmptyQuantity_ToProductQuantityIfItDoesNotHaveIt() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", null, new Price(3), true),
          new("Product 2", null, Price.Empty, false),
        ]);

      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      CurrentShoppingList response = await service.GetCurrentShoppingList(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < shoppingList.Items.Count; i++) {
        Assert.Empty(response.ShoppingList.Items[i].Quantity);
      }
    }

    [Fact(DisplayName = "Maps shopping list item price to product price in the response")]
    public async Task Api_MapsShoppingListItemPrice_ToProductPriceInResponse() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", null, new Price(3), true),
          new("Product 2", null, new Price(10.3f), false),
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
          shoppingList.Items.ElementAt(i).Price.Value,
          response.ShoppingList.Items[i].Price);
      }
    }

    [Fact(DisplayName = "Maps product price to 0 if the last price is null in shopping list items")]
    public async Task Api_MapsProductPriceToZero_IfLastPriceIsNullInShoppingListItems() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", null, Price.Empty, true),
          new("Product 2", null, Price.Empty, false),
        ]);

      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      CurrentShoppingList response = await service.GetCurrentShoppingList(
        new Empty(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < shoppingList.Items.Count; i++) {
        Assert.Equal(0f, response.ShoppingList.Items[i].Price);
      }
    }

    [Fact(DisplayName = "Maps shopping list item checked state to product checked state in the response")]
    public async Task Api_MapsShoppingListItemCheckedState_ToProductCheckedStateInResponse() {
      // Arrange
      var shoppingList = new Domain.Shopping.ShoppingList(
        "Weekly Groceries", [
          new("Product 1", null, new Price(3), true),
          new("Product 2", null, Price.Empty, false),
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
          response.ShoppingList.Items[i].Checked);
      }
    }

    [Fact(DisplayName = "Passes user UID from JWT claim to query")]
    public async Task Api_PassesUserUidFromClaim_ToQuery() {
      // Arrange
      var expectedUid = Guid.CreateVersion7();
      _useCaseHandler
        .Handle(Arg.Any<GetCurrentShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(new Domain.Shopping.ShoppingList("test", []));

      // Act
      await service.GetCurrentShoppingList(new Empty(), CreateServerCallContext(expectedUid));

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetCurrentShoppingList.Query>(q => q.UserUid == expectedUid),
        TestContext.Current.CancellationToken);
    }
  }

  public class RecordShoppingListRpc {
    private readonly ICommandHandler<RecordShoppingList.Command> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public RecordShoppingListRpc() {
      _useCaseHandler = Substitute.For<ICommandHandler<RecordShoppingList.Command>>();
      service = new ShoppingGrpcService(
        Substitute.For<IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<Product>>>(),
        Substitute.For<IQueryHandler<GetCurrentShoppingList.Query, Domain.Shopping.ShoppingList>>(),
        _useCaseHandler,
        Substitute.For<ICommandHandler<CreateShoppingList.Command>>(),
        Substitute.For<ICommandHandler<AddItemsToList.Command>>(),
        Substitute.For<ICommandHandler<UpdateItem.Command>>(),
        Substitute.For<ICommandHandler<RemoveItem.Command>>()
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
          Items = { }
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
          Items = { }
        }
      };

      // Act
      Empty response = await service.RecordShoppingList(
        request, CreateServerCallContext());

      // Assert
      Assert.NotNull(response);
    }

    [Fact(DisplayName = "Maps shopping list name from request to the command")]
    public async Task Api_MapsShoppingListName_FromRequestToCommand() {
      // Arrange
      const string ListName = "Weekly Groceries";
      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList {
          Name = ListName,
          Items = { }
        }
      };

      _useCaseHandler
        .Handle(Arg.Any<RecordShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      // Act
      await service.RecordShoppingList(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<RecordShoppingList.Command>(cmd =>
          cmd.ShoppingListName == ListName),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps product name from request to shopping list item in the command")]
    public async Task Api_MapsProductName_FromRequestToShoppingListItemInCommand() {
      // Arrange
      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList {
          Name = "Weekly Groceries",
          Items = {
            new Protos.Shopping.ShoppingItem {
              Name = "Product 1",
              Quantity = "1 litre",
              Price = 3,
              Checked = true,
            },
            new Protos.Shopping.ShoppingItem {
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
      for (int i = 0; i < request.ShoppingList.Items.Count; i++) {
        await _useCaseHandler.Received(1).Handle(
          Arg.Is<RecordShoppingList.Command>(cmd =>
            cmd.ShoppingListItems.ElementAt(i).Name ==
            request.ShoppingList.Items[i].Name),
          TestContext.Current.CancellationToken);
      }
    }

    [Fact(DisplayName = "Maps product quantity from request to shopping list item in the command")]
    public async Task Api_MapsProductQuantity_FromRequestToShoppingListItemInCommand() {
      // Arrange
      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList {
          Name = "Weekly Groceries",
          Items = {
            new Protos.Shopping.ShoppingItem {
              Name = "Product 1",
              Quantity = "1 litre",
              Price = 3,
              Checked = true,
            },
            new Protos.Shopping.ShoppingItem {
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
      for (int i = 0; i < request.ShoppingList.Items.Count; i++) {
        await _useCaseHandler.Received(1).Handle(
          Arg.Is<RecordShoppingList.Command>(cmd =>
            cmd.ShoppingListItems.ElementAt(i).Quantity ==
            request.ShoppingList.Items[i].Quantity),
          TestContext.Current.CancellationToken);
      }
    }

    [Fact(DisplayName = "Maps product price from request to shopping list item in the command")]
    public async Task Api_MapsProductPrice_FromRequestToShoppingListItemInCommand() {
      // Arrange
      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList {
          Name = "Weekly Groceries",
          Items = {
            new Protos.Shopping.ShoppingItem {
              Name = "Product 1",
              Quantity = "1 litre",
              Price = 3,
              Checked = true,
            },
            new Protos.Shopping.ShoppingItem {
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
      for (int i = 0; i < request.ShoppingList.Items.Count; i++) {
        float price = request.ShoppingList.Items[i].Price;
        await _useCaseHandler.Received(1).Handle(
          Arg.Is<RecordShoppingList.Command>(cmd => Math.Abs(
            cmd.ShoppingListItems.ElementAt(i).Price - price
          ) < Epsilon),
          TestContext.Current.CancellationToken);
      }
    }

    [Fact(DisplayName = "Maps product checked state from request to shopping list item in the command")]
    public async Task Api_MapsProductCheckedState_FromRequestToShoppingListItemInCommand() {
      // Arrange
      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList {
          Name = "Weekly Groceries",
          Items = {
            new Protos.Shopping.ShoppingItem {
              Name = "Product 1",
              Quantity = "1 litre",
              Price = 3,
              Checked = true,
            },
            new Protos.Shopping.ShoppingItem {
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
      for (int i = 0; i < request.ShoppingList.Items.Count; i++) {
        await _useCaseHandler.Received(1).Handle(
          Arg.Is<RecordShoppingList.Command>(cmd =>
            cmd.ShoppingListItems.ElementAt(i).IsChecked ==
            request.ShoppingList.Items[i].Checked),
          TestContext.Current.CancellationToken);
      }
    }

    [Fact(DisplayName = "Passes user UID from JWT claim to command")]
    public async Task Api_PassesUserUidFromClaim_ToCommand() {
      // Arrange
      var expectedUid = Guid.CreateVersion7();
      _useCaseHandler
        .Handle(Arg.Any<RecordShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new RecordShoppingListRequest {
        ShoppingList = new Protos.Shopping.ShoppingList { Name = "Weekly", Items = { } }
      };

      // Act
      await service.RecordShoppingList(request, CreateServerCallContext(expectedUid));

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<RecordShoppingList.Command>(cmd => cmd.UserUid == expectedUid),
        TestContext.Current.CancellationToken);
    }
  }

  public class CreateShoppingListRpc {
    private readonly ICommandHandler<CreateShoppingList.Command> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public CreateShoppingListRpc() {
      _useCaseHandler = Substitute.For<ICommandHandler<CreateShoppingList.Command>>();
      service = new ShoppingGrpcService(
        Substitute.For<IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<Product>>>(),
        Substitute.For<IQueryHandler<GetCurrentShoppingList.Query, Domain.Shopping.ShoppingList>>(),
        Substitute.For<ICommandHandler<RecordShoppingList.Command>>(),
        _useCaseHandler,
        Substitute.For<ICommandHandler<AddItemsToList.Command>>(),
        Substitute.For<ICommandHandler<UpdateItem.Command>>(),
        Substitute.For<ICommandHandler<RemoveItem.Command>>()
      );
    }

    [Fact(DisplayName = "Throws RpcException if the command handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfCommandHandlerFails() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<CreateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      // Act
      async Task action() => await service.CreateShoppingList(
        new CreateShoppingListRequest { Name = "Groceries" }, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns response with name when handler succeeds with a named list")]
    public async Task Api_ReturnsResponseWithName_WhenHandlerSucceedsWithNamedList() {
      // Arrange
      const string ListName = "Groceries";
      _useCaseHandler
        .Handle(Arg.Any<CreateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      // Act
      CreateShoppingListResponse response = await service.CreateShoppingList(
        new CreateShoppingListRequest { Name = ListName }, CreateServerCallContext());

      // Assert
      Assert.Equal(ListName, response.Name);
    }

    [Fact(DisplayName = "Returns response without name when handler succeeds with a temporary list")]
    public async Task Api_ReturnsResponseWithoutName_WhenHandlerSucceedsWithTemporaryList() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<CreateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      // Act
      CreateShoppingListResponse response = await service.CreateShoppingList(
        new CreateShoppingListRequest(), CreateServerCallContext());

      // Assert
      Assert.False(response.HasName);
    }

    [Fact(DisplayName = "Maps name from request to command")]
    public async Task Api_MapsName_FromRequestToCommand() {
      // Arrange
      const string ListName = "Weekly";
      _useCaseHandler
        .Handle(Arg.Any<CreateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      // Act
      await service.CreateShoppingList(
        new CreateShoppingListRequest { Name = ListName }, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<CreateShoppingList.Command>(cmd => cmd.ShoppingListName == ListName),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps null name when request has no name")]
    public async Task Api_MapsNullName_WhenRequestHasNoName() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<CreateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      // Act
      await service.CreateShoppingList(
        new CreateShoppingListRequest(), CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<CreateShoppingList.Command>(cmd => cmd.ShoppingListName == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes user UID from JWT claim to command")]
    public async Task Api_PassesUserUidFromClaim_ToCommand() {
      // Arrange
      var expectedUid = Guid.CreateVersion7();
      _useCaseHandler
        .Handle(Arg.Any<CreateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      // Act
      await service.CreateShoppingList(new CreateShoppingListRequest(), CreateServerCallContext(expectedUid));

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<CreateShoppingList.Command>(cmd => cmd.UserUid == expectedUid),
        TestContext.Current.CancellationToken);
    }
  }

  public class AddItemsToListRpc {
    private readonly ICommandHandler<AddItemsToList.Command> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public AddItemsToListRpc() {
      _useCaseHandler = Substitute.For<ICommandHandler<AddItemsToList.Command>>();
      service = new ShoppingGrpcService(
        Substitute.For<IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<Product>>>(),
        Substitute.For<IQueryHandler<GetCurrentShoppingList.Query, Domain.Shopping.ShoppingList>>(),
        Substitute.For<ICommandHandler<RecordShoppingList.Command>>(),
        Substitute.For<ICommandHandler<CreateShoppingList.Command>>(),
        _useCaseHandler,
        Substitute.For<ICommandHandler<UpdateItem.Command>>(),
        Substitute.For<ICommandHandler<RemoveItem.Command>>()
      );
    }

    [Fact(DisplayName = "Throws RpcException if the command handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfCommandHandlerFails() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddItemsToList.Command>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      var request = new AddItemsToListRequest {
        ShoppingListName = "Weekly",
        Items = { new Protos.Shopping.ShoppingItem { Name = "Milk" } }
      };

      // Act
      async Task action() => await service.AddItemsToList(request, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns empty when handler succeeds")]
    public async Task Api_ReturnsEmpty_WhenHandlerSucceeds() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddItemsToList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddItemsToListRequest {
        ShoppingListName = "Weekly",
        Items = { new Protos.Shopping.ShoppingItem { Name = "Milk" } }
      };

      // Act
      Empty response = await service.AddItemsToList(request, CreateServerCallContext());

      // Assert
      Assert.NotNull(response);
    }

    [Fact(DisplayName = "Maps shopping list name from request to command")]
    public async Task Api_MapsShoppingListName_FromRequestToCommand() {
      // Arrange
      const string ListName = "Weekly";
      _useCaseHandler
        .Handle(Arg.Any<AddItemsToList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddItemsToListRequest {
        ShoppingListName = ListName,
        Items = { new Protos.Shopping.ShoppingItem { Name = "Milk" } }
      };

      // Act
      await service.AddItemsToList(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<AddItemsToList.Command>(cmd => cmd.ShoppingListName == ListName),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps null list name when request has no shopping list name")]
    public async Task Api_MapsNullListName_WhenRequestHasNoShoppingListName() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddItemsToList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddItemsToListRequest {
        Items = { new Protos.Shopping.ShoppingItem { Name = "Milk" } }
      };

      // Act
      await service.AddItemsToList(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<AddItemsToList.Command>(cmd => cmd.ShoppingListName == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps item names from request to command")]
    public async Task Api_MapsItemNames_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddItemsToList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddItemsToListRequest {
        ShoppingListName = "Weekly",
        Items = {
          new Protos.Shopping.ShoppingItem { Name = "Milk", Price = 2f },
          new Protos.Shopping.ShoppingItem { Name = "Bread", Price = 1.5f },
        }
      };

      // Act
      await service.AddItemsToList(request, CreateServerCallContext());

      // Assert
      for (int i = 0; i < request.Items.Count; i++) {
        await _useCaseHandler.Received(1).Handle(
          Arg.Is<AddItemsToList.Command>(cmd =>
            cmd.Items.ElementAt(i).Name == request.Items[i].Name),
          TestContext.Current.CancellationToken);
      }
    }

    [Fact(DisplayName = "Passes user UID from JWT claim to command")]
    public async Task Api_PassesUserUidFromClaim_ToCommand() {
      // Arrange
      var expectedUid = Guid.CreateVersion7();
      _useCaseHandler
        .Handle(Arg.Any<AddItemsToList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddItemsToListRequest {
        Items = { new Protos.Shopping.ShoppingItem { Name = "Milk" } }
      };

      // Act
      await service.AddItemsToList(request, CreateServerCallContext(expectedUid));

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<AddItemsToList.Command>(cmd => cmd.UserUid == expectedUid),
        TestContext.Current.CancellationToken);
    }
  }

  public class UpdateItemRpc {
    private readonly ICommandHandler<UpdateItem.Command> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public UpdateItemRpc() {
      _useCaseHandler = Substitute.For<ICommandHandler<UpdateItem.Command>>();
      service = new ShoppingGrpcService(
        Substitute.For<IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<Product>>>(),
        Substitute.For<IQueryHandler<GetCurrentShoppingList.Query, Domain.Shopping.ShoppingList>>(),
        Substitute.For<ICommandHandler<RecordShoppingList.Command>>(),
        Substitute.For<ICommandHandler<CreateShoppingList.Command>>(),
        Substitute.For<ICommandHandler<AddItemsToList.Command>>(),
        _useCaseHandler,
        Substitute.For<ICommandHandler<RemoveItem.Command>>()
      );
    }

    [Fact(DisplayName = "Throws RpcException if the command handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfCommandHandlerFails() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", OriginalItemName = "Milk", ItemPrice = 3f
      };

      // Act
      async Task action() => await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns empty when handler succeeds")]
    public async Task Api_ReturnsEmpty_WhenHandlerSucceeds() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", OriginalItemName = "Milk", ItemPrice = 3f
      };

      // Act
      Empty response = await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      Assert.NotNull(response);
    }

    [Fact(DisplayName = "Maps shopping list name and item name from request to command")]
    public async Task Api_MapsNames_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", OriginalItemName = "Milk"
      };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd =>
          cmd.ShoppingListName == "Weekly" && cmd.OriginalItemName == "Milk"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps price from request to command when present")]
    public async Task Api_MapsPrice_FromRequestToCommand_WhenPresent() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", OriginalItemName = "Milk", ItemPrice = 3.5f
      };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      const float Epsilon = 0.01f;
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd =>
          cmd.Price.HasValue && Math.Abs(cmd.Price.Value - 3.5f) < Epsilon),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps null price when request has no price")]
    public async Task Api_MapsNullPrice_WhenRequestHasNoPrice() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest { ShoppingListName = "Weekly", OriginalItemName = "Milk" };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.Price == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps quantity from request to command when present")]
    public async Task Api_MapsQuantity_FromRequestToCommand_WhenPresent() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", OriginalItemName = "Milk", ItemQuantity = "2 litres"
      };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.Quantity == "2 litres"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps null quantity when request has no quantity")]
    public async Task Api_MapsNullQuantity_WhenRequestHasNoQuantity() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest { ShoppingListName = "Weekly", OriginalItemName = "Milk" };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.Quantity == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps new item name from request when present")]
    public async Task Api_MapsNewItemName_FromRequest_WhenPresent() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", OriginalItemName = "Milk", ItemName = "Whole Milk"
      };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.NewName == "Whole Milk"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps null new name when request has no item name")]
    public async Task Api_MapsNullNewName_WhenRequestHasNoItemName() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", OriginalItemName = "Milk"
      };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.NewName == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps checked from request when present")]
    public async Task Api_MapsChecked_FromRequestToCommand_WhenPresent() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", OriginalItemName = "Milk", Checked = true
      };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.IsChecked == true),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps null checked when request has no checked field")]
    public async Task Api_MapsNullChecked_WhenRequestHasNoCheckedField() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", OriginalItemName = "Milk"
      };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.IsChecked == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes user UID from JWT claim to command")]
    public async Task Api_PassesUserUidFromClaim_ToCommand() {
      // Arrange
      var expectedUid = Guid.CreateVersion7();
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest { ShoppingListName = "Weekly", OriginalItemName = "Milk" };

      // Act
      await service.UpdateItem(request, CreateServerCallContext(expectedUid));

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.UserUid == expectedUid),
        TestContext.Current.CancellationToken);
    }
  }

  public class RemoveItemRpc {
    private readonly ICommandHandler<RemoveItem.Command> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public RemoveItemRpc() {
      _useCaseHandler = Substitute.For<ICommandHandler<RemoveItem.Command>>();
      service = new ShoppingGrpcService(
        Substitute.For<IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<Product>>>(),
        Substitute.For<IQueryHandler<GetCurrentShoppingList.Query, Domain.Shopping.ShoppingList>>(),
        Substitute.For<ICommandHandler<RecordShoppingList.Command>>(),
        Substitute.For<ICommandHandler<CreateShoppingList.Command>>(),
        Substitute.For<ICommandHandler<AddItemsToList.Command>>(),
        Substitute.For<ICommandHandler<UpdateItem.Command>>(),
        _useCaseHandler
      );
    }

    [Fact(DisplayName = "Throws RpcException if the command handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfCommandHandlerFails() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<RemoveItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      var request = new RemoveItemRequest { ShoppingListName = "Weekly", ItemName = "Milk" };

      // Act
      async Task action() => await service.RemoveItem(request, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns empty when handler succeeds")]
    public async Task Api_ReturnsEmpty_WhenHandlerSucceeds() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<RemoveItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new RemoveItemRequest { ShoppingListName = "Weekly", ItemName = "Milk" };

      // Act
      Empty response = await service.RemoveItem(request, CreateServerCallContext());

      // Assert
      Assert.NotNull(response);
    }

    [Fact(DisplayName = "Maps shopping list name and item name from request to command")]
    public async Task Api_MapsNames_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<RemoveItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new RemoveItemRequest { ShoppingListName = "Weekly", ItemName = "Milk" };

      // Act
      await service.RemoveItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<RemoveItem.Command>(cmd =>
          cmd.ShoppingListName == "Weekly" && cmd.ItemName == "Milk"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes user UID from JWT claim to command")]
    public async Task Api_PassesUserUidFromClaim_ToCommand() {
      // Arrange
      var expectedUid = Guid.CreateVersion7();
      _useCaseHandler
        .Handle(Arg.Any<RemoveItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new RemoveItemRequest { ShoppingListName = "Weekly", ItemName = "Milk" };

      // Act
      await service.RemoveItem(request, CreateServerCallContext(expectedUid));

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<RemoveItem.Command>(cmd => cmd.UserUid == expectedUid),
        TestContext.Current.CancellationToken);
    }
  }

  private static ServerCallContext CreateServerCallContext(Guid? userId = null) {
    ServerCallContext context = TestServerCallContext.Create(
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

    var httpContext = new DefaultHttpContext {
      User = new(new ClaimsIdentity([
        new Claim(ClaimTypes.NameIdentifier, (userId ?? Guid.CreateVersion7()).ToString()),
      ]))
    };
    context.UserState["__HttpContext"] = httpContext;

    return context;
  }
}
