using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Protos.Shopping;
using ShoppingList = Metaspesa.Domain.Shopping.ShoppingList;

namespace Metaspesa.GrpcApi.Services;

internal class ShoppingGrpcService(
  IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<Product>> getRegisteredItemsHandler,
  IQueryHandler<GetCurrentShoppingList.Query, ShoppingList> getCurrentShoppingListHandler,
  ICommandHandler<RecordShoppingList.Command> recordShoppingListHandler,
  ICommandHandler<CreateShoppingList.Command> createShoppingListHandler,
  ICommandHandler<AddItemsToList.Command> addItemsToListHandler
) : ShoppingService.ShoppingServiceBase {
  public override async Task<RegisteredItemsResponse> GetRegisteredItems(
    Empty request, ServerCallContext context
  ) {
    Result<IReadOnlyCollection<Product>> result = await getRegisteredItemsHandler.Handle(
      new GetRegisteredItems.Query(Guid.Empty), context.CancellationToken);

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
    var query = new GetCurrentShoppingList.Query(Guid.Empty);

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
      Guid.Empty,
      request.HasName ? request.Name : null
    );

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
      Guid.Empty,
      request.HasShoppingListName ? request.ShoppingListName : null,
      [..request.Items.Select(i => i.ToAddItemsCommand())]
    );

    Result result = await addItemsToListHandler.Handle(
      command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }

  public override async Task<Empty> RecordShoppingList(
    RecordShoppingListRequest request, ServerCallContext context
  ) {
    var command = new RecordShoppingList.Command(
      Guid.Empty,
      request.ShoppingList.Name,
      [..request.ShoppingList.Items.Select(p => p.ToCommand())]
    );

    Result result = await recordShoppingListHandler.Handle(
      command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }
}