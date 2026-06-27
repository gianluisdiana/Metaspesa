using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Users;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Protos.Shopping;
using Microsoft.AspNetCore.Authorization;

namespace Metaspesa.GrpcApi.Services;

[Authorize(Roles = nameof(Role.Shopper))]
internal class ShoppingGrpcService(
  IQueryHandler<GetShoppingListSummaries.Query, List<AShoppingList>> getShoppingListSummariesHandler,
  IQueryHandler<GetShoppingList.Query, GetShoppingList.Response> getShoppingListHandler,
  ICommandHandler<RecordShoppingList.Command> recordShoppingListHandler,
  ICommandHandler<CreateShoppingList.Command> createShoppingListHandler,
  ICommandHandler<AddItemsToList.Command> addItemsToListHandler,
  ICommandHandler<UpdateItem.Command> updateItemHandler,
  ICommandHandler<RemoveItem.Command> removeItemHandler,
  ICommandHandler<UpdateShoppingList.Command>? updateShoppingListHandler = null
) : ShoppingService.ShoppingServiceBase {
  public override async Task<ShoppingListSummariesResponse> GetShoppingListSummaries(
    Empty request, ServerCallContext context
  ) {
    var query = new GetShoppingListSummaries.Query(
      UserUid: context.GetHttpContext().GetUserUid());

    Result<List<AShoppingList>> result =
      await getShoppingListSummariesHandler.Handle(query, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    var response = new ShoppingListSummariesResponse();
    response.ShoppingLists.AddRange(result.Value.Select(summary => summary.ToSummaryProto()));

    return response;
  }

  public override async Task<ShoppingListResponse> GetShoppingList(
    GetShoppingListRequest request, ServerCallContext context
  ) {
    var query = new GetShoppingList.Query(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: request.HasShoppingListName
        ? GrpcTextSanitizer.SanitizeAscii(request.ShoppingListName)
        : null);

    Result<GetShoppingList.Response> result = await getShoppingListHandler
      .Handle(query, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    var response = new ShoppingListResponse {
      ShoppingList = result.Value.ToProto(),
    };

    return response;
  }

  public override async Task<CreateShoppingListResponse> CreateShoppingList(
    CreateShoppingListRequest request, ServerCallContext context
  ) {
    var command = new CreateShoppingList.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: request.HasName ? GrpcTextSanitizer.SanitizeAscii(request.Name) : null);

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
      ShoppingListName: request.HasShoppingListName
        ? GrpcTextSanitizer.SanitizeAscii(request.ShoppingListName)
        : null,
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
      ShoppingListName: GrpcTextSanitizer.SanitizeAscii(request.ShoppingListName),
      ProductReferenceUid: request.ProductReferenceUid,
      Amount: request.HasAmount ? request.Amount : null,
      IsChecked: request.HasIsChecked ? request.IsChecked : null);

    Result result = await updateItemHandler.Handle(command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }

  public override async Task<Empty> UpdateShoppingList(
    UpdateShoppingListRequest request, ServerCallContext context
  ) {
    if (updateShoppingListHandler is null) {
      throw new RpcException(new Status(StatusCode.Internal, "Update shopping list handler is not configured."));
    }

    var command = new UpdateShoppingList.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: string.IsNullOrWhiteSpace(request.ShoppingListName)
        ? null
        : GrpcTextSanitizer.SanitizeAscii(request.ShoppingListName),
      NewName: request.HasListName
        ? GrpcTextSanitizer.SanitizeAscii(request.ListName)
        : null);

    Result result = await updateShoppingListHandler.Handle(command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }

  public override async Task<Empty> RemoveItem(
    RemoveItemRequest request, ServerCallContext context
  ) {
    var command = new RemoveItem.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: GrpcTextSanitizer.SanitizeAscii(request.ShoppingListName),
      ProductReferenceUid: request.ProductReferenceUid);

    Result result = await removeItemHandler.Handle(command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }

  public override async Task<Empty> RecordShoppingList(
    RecordShoppingListRequest request, ServerCallContext context
  ) {
    var command = new RecordShoppingList.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: GrpcTextSanitizer.SanitizeAscii(request.ShoppingList.Name),
      ShoppingListItems: [.. request.ShoppingList.Items.Select(p => p.ToCommand())]);

    Result result = await recordShoppingListHandler.Handle(
      command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }
}
