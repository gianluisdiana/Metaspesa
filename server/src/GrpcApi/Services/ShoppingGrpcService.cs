using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.GrpcApi.Protos.Shopping;

namespace Metaspesa.GrpcApi.Services;

internal class ShoppingGrpcService() : ShoppingService.ShoppingServiceBase {
  public override Task<RegisteredProductsResponse> GetRegisteredProducts(
    Empty request, ServerCallContext context
  ) {
    var result = new RegisteredProductsResponse();
    return Task.FromResult(result);
  }

  public override Task<CurrentShoppingList> GetCurrentShoppingList(
    Empty request, ServerCallContext context
  ) {
    var result = new CurrentShoppingList();
    return Task.FromResult(result);
  }

  public override Task<Empty> RecordShoppingList(
    RecordShoppingListRequest request, ServerCallContext context
  ) {
    return Task.FromResult(new Empty());
  }
}
