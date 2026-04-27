using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Users;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Protos.Shopping;
using Microsoft.AspNetCore.Authorization;
using ShoppingList = Metaspesa.Domain.Shopping.ShoppingList;

namespace Metaspesa.GrpcApi.Services;

[Authorize(Roles = nameof(Role.Shopper))]
internal class ShoppingGrpcService(
  IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<Product>> getRegisteredItemsHandler,
  IQueryHandler<GetCurrentShoppingList.Query, ShoppingList> getCurrentShoppingListHandler,
  ICommandHandler<RecordShoppingList.Command> recordShoppingListHandler,
  ICommandHandler<CreateShoppingList.Command> createShoppingListHandler,
  ICommandHandler<AddItemsToList.Command> addItemsToListHandler,
  ICommandHandler<UpdateItem.Command> updateItemHandler,
  ICommandHandler<RemoveItem.Command> removeItemHandler
) : ShoppingService.ShoppingServiceBase {

  public override async Task<RegisteredItemsResponse> GetRegisteredItems(
    Empty request, ServerCallContext context
  ) {
    var query = new GetRegisteredItems.Query(
      UserUid: context.GetHttpContext().GetUserUid());

    Result<IReadOnlyCollection<Product>> result = await getRegisteredItemsHandler.Handle(
      query, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    var response = new RegisteredItemsResponse();
    response.Items.AddRange(
      result.Value.Select(item => item.ToProto())
    );

    return response;
  }

  public override async Task<CurrentShoppingList> GetCurrentShoppingList(
    Empty request, ServerCallContext context
  ) {
    var query = new GetCurrentShoppingList.Query(
      UserUid: context.GetHttpContext().GetUserUid());

    Result<ShoppingList> result = await getCurrentShoppingListHandler
      .Handle(query, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    ShoppingList shoppingList = result.Value;

    var response = new CurrentShoppingList {
      ShoppingList = shoppingList.ToProto(),
    };

    return response;
  }

  public override async Task<CreateShoppingListResponse> CreateShoppingList(
    CreateShoppingListRequest request, ServerCallContext context
  ) {
    var command = new CreateShoppingList.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: request.HasName ? request.Name : null);

    Result result = await createShoppingListHandler.Handle(
      command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    var response = new CreateShoppingListResponse();
    if (!string.IsNullOrWhiteSpace(command.ShoppingListName)) {
      response.Name = command.ShoppingListName;
    }
    return response;
  }

  public override async Task<Empty> AddItemsToList(
    AddItemsToListRequest request, ServerCallContext context
  ) {
    var command = new AddItemsToList.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: request.HasShoppingListName ? request.ShoppingListName : null,
      Items: [.. request.Items.Select(i => i.ToAddItemsCommand())]);

    Result result = await addItemsToListHandler.Handle(
      command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }

  public override async Task<Empty> UpdateItem(
    UpdateItemRequest request, ServerCallContext context
  ) {
    var command = new UpdateItem.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: request.ShoppingListName,
      OriginalItemName: request.OriginalItemName,
      NewName: request.HasItemName ? request.ItemName : null,
      Quantity: request.HasItemQuantity ? request.ItemQuantity : null,
      Price: request.HasItemPrice ? request.ItemPrice : null,
      IsChecked: request.HasChecked ? request.Checked : null);

    Result result = await updateItemHandler.Handle(command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }

  public override async Task<Empty> RemoveItem(
    RemoveItemRequest request, ServerCallContext context
  ) {
    var command = new RemoveItem.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: request.ShoppingListName,
      ItemName: request.ItemName);

    Result result = await removeItemHandler.Handle(command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }

  public override async Task<Empty> RecordShoppingList(
    RecordShoppingListRequest request, ServerCallContext context
  ) {
    var command = new RecordShoppingList.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: request.ShoppingList.Name,
      ShoppingListItems: [.. request.ShoppingList.Items.Select(p => p.ToCommand())]);

    Result result = await recordShoppingListHandler.Handle(
      command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }
}