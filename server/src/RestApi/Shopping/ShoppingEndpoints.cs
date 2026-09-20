using Metaspesa.Domain.Identity;
using Metaspesa.RestApi.Shopping.AddShoppingItems;
using Metaspesa.RestApi.Shopping.CheckoutShoppingList;
using Metaspesa.RestApi.Shopping.CreateShoppingList;
using Metaspesa.RestApi.Shopping.GetShoppingList;
using Metaspesa.RestApi.Shopping.GetShoppingLists;
using Metaspesa.RestApi.Shopping.RemoveShoppingItem;
using Metaspesa.RestApi.Shopping.RenameShoppingList;
using Metaspesa.RestApi.Shopping.UpdateShoppingItem;

namespace Metaspesa.RestApi.Shopping;

internal static class ShoppingEndpoints {
  public static IEndpointRouteBuilder MapShoppingEndpoints(
    this IEndpointRouteBuilder endpoints
  ) {
    RouteGroupBuilder lists = endpoints.MapGroup("/api/v1/shopping-lists")
      .RequireAuthorization(policy => policy
        .RequireAuthenticatedUser().RequireRole(nameof(Role.Shopper)));
    lists.MapGetShoppingListsEndpoint();
    lists.MapGetShoppingListEndpoint();
    lists.MapCreateShoppingListEndpoint();
    lists.MapRenameShoppingListEndpoint();
    lists.MapAddShoppingItemsEndpoint();
    lists.MapUpdateShoppingItemEndpoint();
    lists.MapRemoveShoppingItemEndpoint();
    lists.MapCheckoutShoppingListEndpoint();
    return endpoints;
  }
}