using Metaspesa.Application.Shopping;
using Metaspesa.RestApi.Security;

namespace Metaspesa.RestApi.Shopping.UpdateShoppingItem;

internal static class UpdateShoppingItemEndpoint {
  public static IEndpointRouteBuilder MapUpdateShoppingItemEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapPatch("/{listId:guid}/items/{productFormatId:guid}", UpdateItemAsync)
      .WithName("UpdateShoppingItem")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status500InternalServerError)
      .AddEndpointFilter<AllowedOriginFilter>();
    return endpoints;
  }

  /// <summary>Update a shopping item.</summary>
  /// <remarks>Change the amount, checked state, or both for an item in an owned list. Requires the Shopper role.</remarks>
  /// <param name="listId">ID of the shopping list containing the item.</param>
  /// <param name="productFormatId">ID of the item's product format.</param>
  /// <param name="request">New positive amount or checked state; at least one is required.</param>
  /// <param name="context">Authenticated request context.</param>
  /// <param name="handler">Update item use case.</param>
  /// <param name="cancellationToken">Request cancellation token.</param>
  /// <response code="204">Item updated; no response body.</response>
  /// <response code="400">An ID or amount is invalid, or neither field was supplied.</response>
  /// <response code="404">The list or item was not found.</response>
  /// <response code="401">No valid authentication token was provided.</response>
  /// <response code="403">The authenticated user lacks the Shopper role.</response>
  /// <response code="500">An unexpected server or database failure occurred.</response>
  internal static async Task<IResult> UpdateItemAsync(
    Guid listId, Guid productFormatId, UpdateShoppingItemRequest request,
    HttpContext context, UpdateItem.Handler handler,
    CancellationToken cancellationToken
  ) {
    await handler.Handle(new UpdateItem.Command(
      ShoppingUser.GetUid(context), listId, productFormatId,
      request.Amount, request.Checked), cancellationToken);
    return Results.NoContent();
  }
}