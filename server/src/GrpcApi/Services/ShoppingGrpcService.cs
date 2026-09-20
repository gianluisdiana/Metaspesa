using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.Application.Purchasing;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping.Errors;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Protos.Shopping;
using Metaspesa.Infrastructure;
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
    int shoppingListId = await ResolveListIdAsync(
      request.HasShoppingListName ? request.ShoppingListName : null, context);
    var query = new GetShoppingList.Query(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListId: shoppingListId);

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
      ShoppingListName: request.HasName ? TextSanitizer.Sanitize(request.Name) : null);

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
    int shoppingListId = await ResolveListIdAsync(
      request.HasShoppingListName ? request.ShoppingListName : null, context);
    var command = new AddItemsToList.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListId: shoppingListId,
      Items: [.. request.Items.Select(i => i.ToAddItemsCommand())]);

    await addItemsToListHandler.Handle(command, context.CancellationToken);

    return new Empty();
  }

  public override async Task<Empty> UpdateItem(
    UpdateItemRequest request, ServerCallContext context
  ) {
    int shoppingListId = await ResolveListIdAsync(request.ShoppingListName, context);
    var command = new UpdateItem.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListId: shoppingListId,
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

    int shoppingListId = await ResolveListIdAsync(request.ShoppingListName, context);
    var command = new UpdateShoppingList.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListId: shoppingListId,
      NewName: request.HasListName
        ? TextSanitizer.Sanitize(request.ListName)
        : null);

    await updateShoppingListHandler.Handle(command, context.CancellationToken);

    return new Empty();
  }

  public override async Task<Empty> RemoveItem(
    RemoveItemRequest request, ServerCallContext context
  ) {
    int shoppingListId = await ResolveListIdAsync(request.ShoppingListName, context);
    var command = new RemoveItem.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListId: shoppingListId,
      ProductFormatUid: request.ProductFormatUid);

    await removeItemHandler.Handle(command, context.CancellationToken);

    return new Empty();
  }

  public override async Task<Empty> RecordShoppingList(
    RecordShoppingListRequest request, ServerCallContext context
  ) {
    int shoppingListId = await ResolveListIdAsync(request.ShoppingListName, context);
    var command = new CheckoutShoppingList.Command(
      UserUid: context.GetHttpContext().GetUserUid(),
      ShoppingListId: shoppingListId);

    await checkoutShoppingListHandler.Handle(command, context.CancellationToken);

    return new Empty();
  }

  private async Task<int> ResolveListIdAsync(
    string? name, ServerCallContext context
  ) {
    string? sanitizedName = string.IsNullOrWhiteSpace(name)
      ? null : TextSanitizer.Sanitize(name).Trim();
    IReadOnlyCollection<GetShoppingListSummaries.Response> lists =
      await getShoppingListSummariesHandler.Handle(
        new GetShoppingListSummaries.Query(context.GetHttpContext().GetUserUid()),
        context.CancellationToken);
    return lists.FirstOrDefault(list =>
      string.Equals(list.Name, sanitizedName, StringComparison.OrdinalIgnoreCase))
      ?.Id ?? throw new ShoppingListNotFoundException();
  }
}