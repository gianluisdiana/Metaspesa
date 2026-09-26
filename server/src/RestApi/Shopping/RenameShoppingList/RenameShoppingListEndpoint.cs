using Metaspesa.Application.Shopping;
using Metaspesa.Infrastructure;
using Metaspesa.RestApi.Security;

namespace Metaspesa.RestApi.Shopping.RenameShoppingList;

internal static class RenameShoppingListEndpoint {
  public static IEndpointRouteBuilder MapRenameShoppingListEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapPatch("/{listId:guid}", RenameAsync)
      .WithName("RenameShoppingList")
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

  /// <summary>Rename a shopping list.</summary>
  /// <remarks>Assign a nonblank name to an owned list, including a temporary list. Requires the Shopper role.</remarks>
  /// <param name="listId">ID of the shopping list to rename.</param>
  /// <param name="request">New list name.</param>
  /// <param name="context">Authenticated request context.</param>
  /// <param name="handler">Update shopping list use case.</param>
  /// <param name="cancellationToken">Request cancellation token.</param>
  /// <response code="204">List renamed; no response body.</response>
  /// <response code="400">The list ID or new name is invalid.</response>
  /// <response code="404">The list was not found.</response>
  /// <response code="409">Another list already has this name.</response>
  /// <response code="401">No valid authentication token was provided.</response>
  /// <response code="403">The authenticated user lacks the Shopper role.</response>
  /// <response code="500">An unexpected server or database failure occurred.</response>
  internal static async Task<IResult> RenameAsync(
    Guid listId, RenameShoppingListRequest request, HttpContext context,
    UpdateShoppingList.Handler handler, CancellationToken cancellationToken
  ) {
    await handler.Handle(new UpdateShoppingList.Command(
      ShoppingUser.GetUid(context), listId,
      request.Name is null ? null : TextSanitizer.Sanitize(request.Name)
      ), cancellationToken);
    return Results.NoContent();
  }
}