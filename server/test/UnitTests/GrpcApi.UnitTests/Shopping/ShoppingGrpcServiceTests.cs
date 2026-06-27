using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;
using Metaspesa.GrpcApi.Protos.Shopping;
using Metaspesa.GrpcApi.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using DomainShoppingList = Metaspesa.Domain.Shopping.AShoppingList;

namespace Metaspesa.GrpcApi.UnitTests.Shopping;

public static class ShoppingGrpcServiceTests {
  public class GetShoppingListRpc {
    private readonly IQueryHandler<GetShoppingList.Query, GetShoppingList.Response> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public GetShoppingListRpc() {
      _useCaseHandler = Substitute.For<
      IQueryHandler<GetShoppingList.Query, GetShoppingList.Response>>();
      service = new ShoppingGrpcService(
        Substitute.For<IQueryHandler<GetShoppingListSummaries.Query, List<DomainShoppingList>>>(),
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
        .Handle(Arg.Any<GetShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      // Act
      async Task action() => await service.GetShoppingList(
        new GetShoppingListRequest(), CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns the shopping list if the query handler returns a success result")]
    public async Task Api_ReturnsShoppingList_IfQueryHandlerSucceeds() {
      // Arrange
      GetShoppingList.Response shoppingList = MakeShoppingList("Weekly Groceries");

      _useCaseHandler
        .Handle(Arg.Any<GetShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      ShoppingListResponse response = await service.GetShoppingList(
        new GetShoppingListRequest(), CreateServerCallContext());

      // Assert
      Assert.Equal(shoppingList.ShoppingListName, response.ShoppingList.Name);
    }

    [Fact(DisplayName = "Maps shopping list items to products in the response")]
    public async Task Api_MapsShoppingListItems_ToProductsInResponse() {
      // Arrange
      GetShoppingList.Response shoppingList = MakeShoppingList("Weekly Groceries");
      _useCaseHandler
        .Handle(Arg.Any<GetShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      ShoppingListResponse response = await service.GetShoppingList(
        new GetShoppingListRequest(), CreateServerCallContext());

      // Assert
      Assert.Equal(shoppingList.Items.Count, response.ShoppingList.Items.Count);
    }

    [Fact(DisplayName = "Maps shopping list name to product name in the response")]
    public async Task Api_MapsShoppingListName_ToProductNameInResponse() {
      // Arrange
      GetShoppingList.Response shoppingList = MakeShoppingList("Weekly Groceries");

      _useCaseHandler
        .Handle(Arg.Any<GetShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      ShoppingListResponse response = await service.GetShoppingList(
        new GetShoppingListRequest(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < shoppingList.Items.Count; i++) {
        Assert.Equal(
          shoppingList.Items.ElementAt(i).ProductName,
          response.ShoppingList.Items[i].Name);
      }
    }

    [Fact(DisplayName = "Maps shopping list item quantity to product quantity in the response")]
    public async Task Api_MapsShoppingListItemQuantity_ToProductQuantityInResponse() {
      // Arrange
      var shoppingList = new GetShoppingList.Response(
        "Weekly Groceries",
        [
          new GetShoppingList.ResponseItem(
            "Product 1", 1, MakeFormat(1, "litre", 3), true),
          new GetShoppingList.ResponseItem(
            "Product 2", 1, MakeFormat(2, "kg", 0), false),
        ]);

      _useCaseHandler
        .Handle(Arg.Any<GetShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      ShoppingListResponse response = await service.GetShoppingList(
        new GetShoppingListRequest(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < shoppingList.Items.Count; i++) {
        Assert.Equal(
          $"{shoppingList.Items.ElementAt(i).Format.Quantity.Value:G} " +
          shoppingList.Items.ElementAt(i).Format.Quantity.UnitOfMeasure,
          response.ShoppingList.Items[i].Quantity);
      }
    }

    [Fact(DisplayName = "Maps shopping list item price to product price in the response")]
    public async Task Api_MapsShoppingListItemPrice_ToProductPriceInResponse() {
      // Arrange
      var shoppingList = new GetShoppingList.Response(
        "Weekly Groceries",
        [
          new GetShoppingList.ResponseItem(
            "Product 1", 1, MakeFormat(1, "unit", 3), true),
          new GetShoppingList.ResponseItem(
            "Product 2", 1, MakeFormat(1, "unit", 10.3m), false),
        ]);

      _useCaseHandler
        .Handle(Arg.Any<GetShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      ShoppingListResponse response = await service.GetShoppingList(
        new GetShoppingListRequest(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < shoppingList.Items.Count; i++) {
        Assert.Equal(
          shoppingList.Items.ElementAt(i).Format.Price.Value.ToString(CultureInfo.InvariantCulture),
          response.ShoppingList.Items[i].Price);
      }
    }

    [Fact(DisplayName = "Maps product price to 0 if the last price is null in shopping list items")]
    public async Task Api_MapsProductPriceToZero_IfLastPriceIsNullInShoppingListItems() {
      // Arrange
      var shoppingList = new GetShoppingList.Response(
        "Weekly Groceries",
        [
          new GetShoppingList.ResponseItem(
            "Product 1", 1, MakeFormat(1, "unit", 0), true),
          new GetShoppingList.ResponseItem(
            "Product 2", 1, MakeFormat(1, "unit", 0), false),
        ]);

      _useCaseHandler
        .Handle(Arg.Any<GetShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      ShoppingListResponse response = await service.GetShoppingList(
        new GetShoppingListRequest(), CreateServerCallContext());

      // Assert
      for (int i = 0; i < shoppingList.Items.Count; i++) {
        Assert.Equal("0", response.ShoppingList.Items[i].Price);
      }
    }

    [Fact(DisplayName = "Maps shopping list item checked state to product checked state in the response")]
    public async Task Api_MapsShoppingListItemCheckedState_ToProductCheckedStateInResponse() {
      // Arrange
      GetShoppingList.Response shoppingList = MakeShoppingList("Weekly Groceries");

      _useCaseHandler
        .Handle(Arg.Any<GetShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(shoppingList);

      // Act
      ShoppingListResponse response = await service.GetShoppingList(
        new GetShoppingListRequest(), CreateServerCallContext());

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
        .Handle(Arg.Any<GetShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(new GetShoppingList.Response("test", []));

      // Act
      await service.GetShoppingList(new GetShoppingListRequest(), CreateServerCallContext(expectedUid));

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetShoppingList.Query>(q => q.UserUid == expectedUid),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps shopping list name from request to query")]
    public async Task Api_MapsShoppingListName_FromRequestToQuery() {
      // Arrange
      const string ListName = "Weekly";
      _useCaseHandler
        .Handle(Arg.Any<GetShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(new GetShoppingList.Response(ListName, []));

      // Act
      await service.GetShoppingList(
        new GetShoppingListRequest { ShoppingListName = ListName },
        CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetShoppingList.Query>(q => q.ShoppingListName == ListName),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps missing shopping list name to null in query")]
    public async Task Api_MapsMissingShoppingListName_ToNullInQuery() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<GetShoppingList.Query>(), TestContext.Current.CancellationToken)
        .Returns(new GetShoppingList.Response(null, []));

      // Act
      await service.GetShoppingList(new GetShoppingListRequest(), CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetShoppingList.Query>(q => q.ShoppingListName == null),
        TestContext.Current.CancellationToken);
    }

    private static GetShoppingList.Response MakeShoppingList(string? name) =>
      new(
        name,
        [
          new GetShoppingList.ResponseItem(
            "Product 1", 1, MakeFormat(1, "unit", 3), true),
          new GetShoppingList.ResponseItem(
            "Product 2", 1, MakeFormat(1, "unit", 0), false),
        ]);

    private static ProductFormat MakeFormat(
      float quantity,
      string unitOfMeasure,
      decimal price
    ) => new(new AQuantity(quantity, unitOfMeasure), new Price(price), null);
  }

  public class GetShoppingListSummariesRpc {
    private readonly IQueryHandler<
      GetShoppingListSummaries.Query,
      List<DomainShoppingList>> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public GetShoppingListSummariesRpc() {
      _useCaseHandler = Substitute.For<
        IQueryHandler<GetShoppingListSummaries.Query, List<DomainShoppingList>>>();
      service = new ShoppingGrpcService(
        _useCaseHandler,
        Substitute.For<IQueryHandler<GetShoppingList.Query, GetShoppingList.Response>>(),
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
        .Handle(Arg.Any<GetShoppingListSummaries.Query>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      // Act
      async Task action() => await service.GetShoppingListSummaries(
        new Empty(), CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns shopping list summary count if the query handler succeeds")]
    public async Task Api_ReturnsShoppingListSummaryCount_IfQueryHandlerSucceeds() {
      // Arrange
      List<DomainShoppingList> summaries = [
        new DomainShoppingList("Groceries", []),
        new DomainShoppingList(null, []),
      ];
      _useCaseHandler
        .Handle(Arg.Any<GetShoppingListSummaries.Query>(), TestContext.Current.CancellationToken)
        .Returns(summaries);

      // Act
      ShoppingListSummariesResponse response = await service.GetShoppingListSummaries(
        new Empty(), CreateServerCallContext());

      // Assert
      Assert.Equal(summaries.Count, response.ShoppingLists.Count);
    }

    [Fact(DisplayName = "Maps named shopping list summary name")]
    public async Task Api_MapsNamedShoppingListSummaryName() {
      // Arrange
      List<DomainShoppingList> summaries = [
        new DomainShoppingList("Groceries", []),
        new DomainShoppingList(null, []),
      ];
      _useCaseHandler
        .Handle(Arg.Any<GetShoppingListSummaries.Query>(), TestContext.Current.CancellationToken)
        .Returns(summaries);

      // Act
      ShoppingListSummariesResponse response = await service.GetShoppingListSummaries(
        new Empty(), CreateServerCallContext());

      // Assert
      Assert.Equal("Groceries", response.ShoppingLists[0].Name);
    }

    [Fact(DisplayName = "Maps temporary shopping list summary with missing name")]
    public async Task Api_MapsTemporaryShoppingListSummaryWithMissingName() {
      // Arrange
      List<DomainShoppingList> summaries = [
        new DomainShoppingList("Groceries", []),
        new DomainShoppingList(null, []),
      ];
      _useCaseHandler
        .Handle(Arg.Any<GetShoppingListSummaries.Query>(), TestContext.Current.CancellationToken)
        .Returns(summaries);

      // Act
      ShoppingListSummariesResponse response = await service.GetShoppingListSummaries(
        new Empty(), CreateServerCallContext());

      // Assert
      Assert.False(response.ShoppingLists[1].HasName);
    }

    [Fact(DisplayName = "Passes user UID from JWT claim to query")]
    public async Task Api_PassesUserUidFromClaim_ToQuery() {
      // Arrange
      var expectedUid = Guid.CreateVersion7();
      _useCaseHandler
        .Handle(Arg.Any<GetShoppingListSummaries.Query>(), TestContext.Current.CancellationToken)
        .Returns(new List<DomainShoppingList>());

      // Act
      await service.GetShoppingListSummaries(new Empty(), CreateServerCallContext(expectedUid));

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<GetShoppingListSummaries.Query>(q => q.UserUid == expectedUid),
        TestContext.Current.CancellationToken);
    }
  }

  public class RecordShoppingListRpc {
    private readonly ICommandHandler<RecordShoppingList.Command> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public RecordShoppingListRpc() {
      _useCaseHandler = Substitute.For<ICommandHandler<RecordShoppingList.Command>>();
      service = new ShoppingGrpcService(
        Substitute.For<IQueryHandler<GetShoppingListSummaries.Query, List<DomainShoppingList>>>(),
        Substitute.For<IQueryHandler<GetShoppingList.Query, GetShoppingList.Response>>(),
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
              Price = "3",
              Checked = true,
            },
            new Protos.Shopping.ShoppingItem {
              Name = "Product 2",
              Quantity = "2 kg",
              Price = "10.3",
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
              Price = "3",
              Checked = true,
            },
            new Protos.Shopping.ShoppingItem {
              Name = "Product 2",
              Quantity = "2 kg",
              Price = "10.3",
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
              Price = "3",
              Checked = true,
            },
            new Protos.Shopping.ShoppingItem {
              Name = "Product 2",
              Quantity = "2 kg",
              Price = "10.3",
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
      const decimal Epsilon = 0.01m;
      for (int i = 0; i < request.ShoppingList.Items.Count; i++) {
        decimal price = decimal.Parse(
          request.ShoppingList.Items[i].Price,
          CultureInfo.InvariantCulture);
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
              Price = "3",
              Checked = true,
            },
            new Protos.Shopping.ShoppingItem {
              Name = "Product 2",
              Quantity = "2 kg",
              Price = "10.3",
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
        Substitute.For<IQueryHandler<GetShoppingListSummaries.Query, List<DomainShoppingList>>>(),
        Substitute.For<IQueryHandler<GetShoppingList.Query, GetShoppingList.Response>>(),
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

    [Fact(DisplayName = "Passes user UID from mapped name identifier claim to command")]
    public async Task Api_PassesUserUidFromMappedClaim_ToCommand() {
      // Arrange
      var expectedUid = Guid.CreateVersion7();
      _useCaseHandler
        .Handle(Arg.Any<CreateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      // Act
      await service.CreateShoppingList(
        new CreateShoppingListRequest(),
        CreateServerCallContext(expectedUid, ClaimTypes.NameIdentifier));

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
        Substitute.For<IQueryHandler<GetShoppingListSummaries.Query, List<DomainShoppingList>>>(),
        Substitute.For<IQueryHandler<GetShoppingList.Query, GetShoppingList.Response>>(),
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
        Items = { new Protos.Shopping.AShoppingItem { ReferenceUid = 10, Amount = 1 } }
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
        Items = { new Protos.Shopping.AShoppingItem { ReferenceUid = 10, Amount = 1 } }
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
        Items = { new Protos.Shopping.AShoppingItem { ReferenceUid = 10, Amount = 1 } }
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
        Items = { new Protos.Shopping.AShoppingItem { ReferenceUid = 10, Amount = 1 } }
      };

      // Act
      await service.AddItemsToList(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<AddItemsToList.Command>(cmd => cmd.ShoppingListName == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps item reference UIDs from request to command")]
    public async Task Api_MapsItemReferenceUids_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddItemsToList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddItemsToListRequest {
        ShoppingListName = "Weekly",
        Items = {
          new Protos.Shopping.AShoppingItem { ReferenceUid = 10, Amount = 2 },
          new Protos.Shopping.AShoppingItem { ReferenceUid = 11, Amount = 1 },
        }
      };

      // Act
      await service.AddItemsToList(request, CreateServerCallContext());

      // Assert
      for (int i = 0; i < request.Items.Count; i++) {
        await _useCaseHandler.Received(1).Handle(
          Arg.Is<AddItemsToList.Command>(cmd =>
            cmd.Items.ElementAt(i).ReferenceUid == request.Items[i].ReferenceUid),
          TestContext.Current.CancellationToken);
      }
    }

    [Fact(DisplayName = "Maps item amounts from request to command")]
    public async Task Api_MapsItemAmounts_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddItemsToList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddItemsToListRequest {
        ShoppingListName = "Weekly",
        Items = {
          new Protos.Shopping.AShoppingItem { ReferenceUid = 10, Amount = 2 },
        }
      };

      // Act
      await service.AddItemsToList(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<AddItemsToList.Command>(cmd =>
          cmd.Items.Single().Amount == 2),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps item checked state from request to command")]
    public async Task Api_MapsItemCheckedState_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddItemsToList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddItemsToListRequest {
        ShoppingListName = "Weekly",
        Items = {
          new Protos.Shopping.AShoppingItem { ReferenceUid = 10, Amount = 1, IsChecked = true },
        }
      };

      // Act
      await service.AddItemsToList(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<AddItemsToList.Command>(cmd =>
          cmd.Items.Single().IsChecked),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Sanitizes non-ASCII list name before creating command")]
    public async Task Api_SanitizesNonAsciiListName_BeforeCreatingCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<AddItemsToList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddItemsToListRequest {
        ShoppingListName = "Semanal \u2713",
        Items = {
          new Protos.Shopping.AShoppingItem { ReferenceUid = 10, Amount = 1 },
        }
      };

      // Act
      await service.AddItemsToList(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<AddItemsToList.Command>(cmd =>
          cmd.ShoppingListName == "Semanal "),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes user UID from JWT claim to command")]
    public async Task Api_PassesUserUidFromClaim_ToCommand() {
      // Arrange
      var expectedUid = Guid.CreateVersion7();
      _useCaseHandler
        .Handle(Arg.Any<AddItemsToList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new AddItemsToListRequest {
        Items = { new Protos.Shopping.AShoppingItem { ReferenceUid = 10, Amount = 1 } }
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
        Substitute.For<IQueryHandler<GetShoppingListSummaries.Query, List<DomainShoppingList>>>(),
        Substitute.For<IQueryHandler<GetShoppingList.Query, GetShoppingList.Response>>(),
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
        ShoppingListName = "Weekly", ProductReferenceUid = 10, Amount = 3
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
        ShoppingListName = "Weekly", ProductReferenceUid = 10, Amount = 3
      };

      // Act
      Empty response = await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      Assert.NotNull(response);
    }

    [Fact(DisplayName = "Maps shopping list name from request to command")]
    public async Task Api_MapsShoppingListName_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", ProductReferenceUid = 10
      };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.ShoppingListName == "Weekly"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps product reference UID from request to command")]
    public async Task Api_MapsProductReferenceUid_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", ProductReferenceUid = 10
      };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.ProductReferenceUid == 10),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps amount from request to command when present")]
    public async Task Api_MapsAmount_FromRequestToCommand_WhenPresent() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", ProductReferenceUid = 10, Amount = 3
      };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.Amount == 3),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps null amount when request has no amount")]
    public async Task Api_MapsNullAmount_WhenRequestHasNoAmount() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest { ShoppingListName = "Weekly", ProductReferenceUid = 10 };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.Amount == null),
        TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Maps checked from request when present")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Api_MapsChecked_FromRequestToCommand_WhenPresent(bool isChecked) {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest {
        ShoppingListName = "Weekly", ProductReferenceUid = 10, IsChecked = isChecked
      };

      // Act
      await service.UpdateItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.IsChecked == isChecked),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps null checked when request has no checked field")]
    public async Task Api_MapsNullChecked_WhenRequestHasNoCheckedField() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateItemRequest { ShoppingListName = "Weekly", ProductReferenceUid = 10 };

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

      var request = new UpdateItemRequest { ShoppingListName = "Weekly", ProductReferenceUid = 10 };

      // Act
      await service.UpdateItem(request, CreateServerCallContext(expectedUid));

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateItem.Command>(cmd => cmd.UserUid == expectedUid),
        TestContext.Current.CancellationToken);
    }

  }

  public class UpdateShoppingListRpc {
    private readonly ICommandHandler<UpdateShoppingList.Command> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public UpdateShoppingListRpc() {
      _useCaseHandler = Substitute.For<ICommandHandler<UpdateShoppingList.Command>>();
      service = new ShoppingGrpcService(
        Substitute.For<IQueryHandler<GetShoppingListSummaries.Query, List<DomainShoppingList>>>(),
        Substitute.For<IQueryHandler<GetShoppingList.Query, GetShoppingList.Response>>(),
        Substitute.For<ICommandHandler<RecordShoppingList.Command>>(),
        Substitute.For<ICommandHandler<CreateShoppingList.Command>>(),
        Substitute.For<ICommandHandler<AddItemsToList.Command>>(),
        Substitute.For<ICommandHandler<UpdateItem.Command>>(),
        Substitute.For<ICommandHandler<RemoveItem.Command>>(),
        _useCaseHandler
      );
    }

    [Fact(DisplayName = "Throws RpcException if the command handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfCommandHandlerFails() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unexpected));

      var request = new UpdateShoppingListRequest { ShoppingListName = "", ListName = "Groceries" };

      // Act
      async Task action() => await service.UpdateShoppingList(request, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns empty when handler succeeds")]
    public async Task Api_ReturnsEmpty_WhenHandlerSucceeds() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateShoppingListRequest { ShoppingListName = "", ListName = "Groceries" };

      // Act
      Empty response = await service.UpdateShoppingList(request, CreateServerCallContext());

      // Assert
      Assert.NotNull(response);
    }

    [Fact(DisplayName = "Maps empty shopping list name to temporary list")]
    public async Task Api_MapsEmptyShoppingListName_ToTemporaryList() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateShoppingListRequest { ShoppingListName = "", ListName = "Groceries" };

      // Act
      await service.UpdateShoppingList(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateShoppingList.Command>(cmd => cmd.ShoppingListName == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps named shopping list from request to command")]
    public async Task Api_MapsNamedShoppingList_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateShoppingListRequest { ShoppingListName = "Weekly", ListName = "Groceries" };

      // Act
      await service.UpdateShoppingList(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateShoppingList.Command>(cmd => cmd.ShoppingListName == "Weekly"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps list name from request to command")]
    public async Task Api_MapsListName_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateShoppingListRequest { ShoppingListName = "", ListName = "Groceries" };

      // Act
      await service.UpdateShoppingList(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateShoppingList.Command>(cmd => cmd.NewName == "Groceries"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps null list name when request has no list name")]
    public async Task Api_MapsNullListName_WhenRequestHasNoListName() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<UpdateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateShoppingListRequest { ShoppingListName = "" };

      // Act
      await service.UpdateShoppingList(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateShoppingList.Command>(cmd => cmd.NewName == null),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes user UID from JWT claim to command")]
    public async Task Api_PassesUserUidFromClaim_ToCommand() {
      // Arrange
      var expectedUid = Guid.CreateVersion7();
      _useCaseHandler
        .Handle(Arg.Any<UpdateShoppingList.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new UpdateShoppingListRequest { ShoppingListName = "", ListName = "Groceries" };

      // Act
      await service.UpdateShoppingList(request, CreateServerCallContext(expectedUid));

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<UpdateShoppingList.Command>(cmd => cmd.UserUid == expectedUid),
        TestContext.Current.CancellationToken);
    }
  }

  public class RemoveItemRpc {
    private readonly ICommandHandler<RemoveItem.Command> _useCaseHandler;
    private readonly ShoppingGrpcService service;

    public RemoveItemRpc() {
      _useCaseHandler = Substitute.For<ICommandHandler<RemoveItem.Command>>();
      service = new ShoppingGrpcService(
        Substitute.For<IQueryHandler<GetShoppingListSummaries.Query, List<DomainShoppingList>>>(),
        Substitute.For<IQueryHandler<GetShoppingList.Query, GetShoppingList.Response>>(),
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

      var request = new RemoveItemRequest { ShoppingListName = "Weekly", ProductReferenceUid = 10 };

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

      var request = new RemoveItemRequest { ShoppingListName = "Weekly", ProductReferenceUid = 10 };

      // Act
      Empty response = await service.RemoveItem(request, CreateServerCallContext());

      // Assert
      Assert.NotNull(response);
    }

    [Fact(DisplayName = "Maps shopping list name from request to command")]
    public async Task Api_MapsShoppingListName_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<RemoveItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new RemoveItemRequest { ShoppingListName = "Weekly", ProductReferenceUid = 10 };

      // Act
      await service.RemoveItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<RemoveItem.Command>(cmd => cmd.ShoppingListName == "Weekly"),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps product reference UID from request to command")]
    public async Task Api_MapsProductReferenceUid_FromRequestToCommand() {
      // Arrange
      _useCaseHandler
        .Handle(Arg.Any<RemoveItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new RemoveItemRequest { ShoppingListName = "Weekly", ProductReferenceUid = 10 };

      // Act
      await service.RemoveItem(request, CreateServerCallContext());

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<RemoveItem.Command>(cmd => cmd.ProductReferenceUid == 10),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Passes user UID from JWT claim to command")]
    public async Task Api_PassesUserUidFromClaim_ToCommand() {
      // Arrange
      var expectedUid = Guid.CreateVersion7();
      _useCaseHandler
        .Handle(Arg.Any<RemoveItem.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new RemoveItemRequest { ShoppingListName = "Weekly", ProductReferenceUid = 10 };

      // Act
      await service.RemoveItem(request, CreateServerCallContext(expectedUid));

      // Assert
      await _useCaseHandler.Received(1).Handle(
        Arg.Is<RemoveItem.Command>(cmd => cmd.UserUid == expectedUid),
        TestContext.Current.CancellationToken);
    }
  }

  private static ServerCallContext CreateServerCallContext(
    Guid? userId = null,
    string claimType = JwtRegisteredClaimNames.Sub
  ) {
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
        new Claim(claimType, (userId ?? Guid.CreateVersion7()).ToString()),
      ]))
    };
    context.UserState["__HttpContext"] = httpContext;

    return context;
  }
}
