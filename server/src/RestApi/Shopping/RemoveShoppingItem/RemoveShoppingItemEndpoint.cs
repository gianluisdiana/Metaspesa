using Metaspesa.Application.Shopping;
using Metaspesa.RestApi.Security;

namespace Metaspesa.RestApi.Shopping.RemoveShoppingItem;

internal static class RemoveShoppingItemEndpoint {
  public static IEndpointRouteBuilder MapRemoveShoppingItemEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapDelete("/{listId:guid}/items/{productFormatId:guid}", RemoveItemAsync)
      .WithName("RemoveShoppingItem")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status500InternalServerError)
      .AddEndpointFilter<AllowedOriginFilter>();
    return endpoints;
  }

  /// <summary>Remove a shopping item.</summary>
  /// <remarks>Remove a product format from an owned shopping list. Requires the Shopper role.</remarks>
  /// <param name="listId">ID of the shopping list containing the item.</param>
  /// <param name="productFormatId">ID of the item's product format.</param>
  /// <param name="context">Authenticated request context.</param>
  /// <param name="handler">Remove item use case.</param>
  /// <param name="cancellationToken">Request cancellation token.</param>
  /// <response code="204">Item removed; no response body.</response>
  /// <response code="400">The list or product format ID is invalid.</response>
  /// <response code="404">The list or item was not found.</response>
  /// <response code="401">No valid authentication token was provided.</response>
  /// <response code="403">The authenticated user lacks the Shopper role.</response>
  /// <response code="500">An unexpected server or database failure occurred.</response>
  internal static async Task<IResult> RemoveItemAsync(
    Guid listId, Guid productFormatId, HttpContext context,
    RemoveItem.Handler handler, CancellationToken cancellationToken
  ) {
    await handler.Handle(new RemoveItem.Command(
      ShoppingUser.GetUid(context), listId, productFormatId),
      cancellationToken);
    return Results.NoContent();
  }
}