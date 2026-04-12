using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Shopping;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Protos.Shopping;
using Product = Metaspesa.Domain.Shopping.Product;
using ShoppingList = Metaspesa.Domain.Shopping.ShoppingList;

namespace Metaspesa.GrpcApi.Services;

internal class ShoppingGrpcService(
  IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<Product>> getRegisteredItemsHandler,
  IQueryHandler<GetCurrentShoppingList.Query, ShoppingList> getCurrentShoppingListHandler,
  ICommandHandler<RecordShoppingList.Command> recordShoppingListHandler
) : ShoppingService.ShoppingServiceBase {
  public override async Task<RegisteredProductsResponse> GetRegisteredProducts(
    Empty request, ServerCallContext context
  ) {
    Result<IReadOnlyCollection<Product>> result = await getRegisteredItemsHandler.Handle(
      new GetRegisteredItems.Query(Guid.Empty), context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    var response = new RegisteredProductsResponse();
    response.Products.AddRange(
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

  public override async Task<Empty> RecordShoppingList(
    RecordShoppingListRequest request, ServerCallContext context
  ) {
    var command = new RecordShoppingList.Command(
      Guid.Empty,
      request.ShoppingList.Name,
      [..request.ShoppingList.Products.Select(p => p.ToCommand())]
    );

    Result result = await recordShoppingListHandler.Handle(
      command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }
}