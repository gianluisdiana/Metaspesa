using Metaspesa.Application.Shopping;
using Metaspesa.RestApi.Security;

namespace Metaspesa.RestApi.Shopping.AddShoppingItems;

internal static class AddShoppingItemsEndpoint {
  public static IEndpointRouteBuilder MapAddShoppingItemsEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapPost("/{listId:int}/items", AddItemsAsync)
      .WithName("AddShoppingItems")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status409Conflict)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status500InternalServerError)
      .AddEndpointFilter<AllowedOriginFilter>();
    return endpoints;
  }

  /// <summary>Add items to a shopping list.</summary>
  /// <remarks>Add one or more product formats to an owned list. Each format may appear only once in the list. Requires the Shopper role.</remarks>
  /// <param name="listId">ID of the shopping list to update.</param>
  /// <param name="request">Items to add with positive amounts and checked states.</param>
  /// <param name="context">Authenticated request context.</param>
  /// <param name="handler">Add items use case.</param>
  /// <param name="cancellationToken">Request cancellation token.</param>
  /// <response code="204">Items added; no response body.</response>
  /// <response code="400">The list ID or items are invalid, or the collection is empty.</response>
  /// <response code="404">The list or a product format was not found.</response>
  /// <response code="409">A product format is already in the list or repeated in the request.</response>
  /// <response code="401">No valid authentication token was provided.</response>
  /// <response code="403">The authenticated user lacks the Shopper role.</response>
  /// <response code="500">An unexpected server or database failure occurred.</response>
  internal static async Task<IResult> AddItemsAsync(
    int listId, AddShoppingItemsRequest request, HttpContext context,
    AddItemsToList.Handler handler, CancellationToken cancellationToken
  ) {
    if (request.Items is null || request.Items.Any(item => item is null)) {
      throw new BadHttpRequestException("Items must be an array.");
    }
    await handler.Handle(new AddItemsToList.Command(
      ShoppingUser.GetUid(context), listId,
      [.. request.Items.Select(item => new AddItemsToList.CommandItem(
        item.ProductFormatId, item.Amount, item.Checked))]), cancellationToken);
    return Results.NoContent();
  }
}