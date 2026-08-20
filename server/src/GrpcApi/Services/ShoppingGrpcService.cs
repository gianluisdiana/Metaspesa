using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.Application.Purchasing;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Protos.Shopping;
using Microsoft.AspNetCore.Authorization;

namespace Metaspesa.GrpcApi.Services;

[Authorize(Roles = nameof(Role.Shopper))]
internal class ShoppingGrpcService(
  GetShoppingListSummaries.Handler getShoppingListSummariesHandler,
  GetShoppingList.Handler getShoppingListHandler,
  CheckoutShoppingList.Handler checkoutShoppingListHandler,
  CreateShoppingList.Handler createShoppingListHandler,
  AddItemsToList.Handler addItemsToListHandler,
  UpdateItem.Handler updateItemHandler,
  RemoveItem.Handler removeItemHandler,
  UpdateShoppingList.Handler? updateShoppingListHandler = null
) : ShoppingService.ShoppingServiceBase {
  public override async Task<ShoppingListSummariesResponse> GetShoppingListSummaries(
    Empty request, ServerCallContext context
  ) {
    var query = new GetShoppingListSummaries.Query(
      UserUid: context.GetHttpContext().GetUserUid());

    IReadOnlyCollection<GetShoppingListSummaries.Response> result =
      await getShoppingListSummariesHandler.Handle(query, context.CancellationToken);

    var response = new ShoppingListSummariesResponse();
    response.ShoppingLists.AddRange(result.Select(summary => summary.ToSummaryProto()));

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

    GetShoppingList.Response result = await getShoppingListHandler
      .Handle(query, context.CancellationToken);

    var response = new ShoppingListResponse {
      ShoppingList = result.ToProto(),
    };

    return response;
  }

  public override async Task<CreateShoppingListResponse> CreateShoppingList(
    CreateShoppingListRequest request, ServerCallContext context
  ) {
    var command = new CreateShoppingList.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: request.HasName ? GrpcTextSanitizer.SanitizeAscii(request.Name) : null);

    await createShoppingListHandler.Handle(command, context.CancellationToken);

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

    await addItemsToListHandler.Handle(command, context.CancellationToken);

    return new Empty();
  }

  public override async Task<Empty> UpdateItem(
    UpdateItemRequest request, ServerCallContext context
  ) {
    var command = new UpdateItem.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: GrpcTextSanitizer.SanitizeAscii(request.ShoppingListName),
      ProductFormatUid: request.ProductFormatUid,
      Amount: request.HasAmount ? request.Amount : null,
      IsChecked: request.HasIsChecked ? request.IsChecked : null);

    await updateItemHandler.Handle(command, context.CancellationToken);

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

    await updateShoppingListHandler.Handle(command, context.CancellationToken);

    return new Empty();
  }

  public override async Task<Empty> RemoveItem(
    RemoveItemRequest request, ServerCallContext context
  ) {
    var command = new RemoveItem.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: GrpcTextSanitizer.SanitizeAscii(request.ShoppingListName),
      ProductFormatUid: request.ProductFormatUid);

    await removeItemHandler.Handle(command, context.CancellationToken);

    return new Empty();
  }

  public override async Task<Empty> RecordShoppingList(
    RecordShoppingListRequest request, ServerCallContext context
  ) {
    var command = new CheckoutShoppingList.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListName: GrpcTextSanitizer.SanitizeAscii(request.ShoppingListName));

    await checkoutShoppingListHandler.Handle(command, context.CancellationToken);

    return new Empty();
  }
}