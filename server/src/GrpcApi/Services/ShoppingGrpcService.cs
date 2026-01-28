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
    ShoppingList shoppingList = new() {
      Name = "Weekly Groceries",
    };
    shoppingList.Products.Add(new Product() {
      Name = "Milk",
      Quantity = "2 Liters",
      Price = 3.50f,
    });
    return Task.FromResult(new CurrentShoppingList {
      ShoppingList = shoppingList,
    });
  }

  public override Task<Empty> RecordShoppingList(
    RecordShoppingListRequest request, ServerCallContext context
  ) {
    return Task.FromResult(new Empty());
  }
}
