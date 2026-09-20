using Metaspesa.Application.Shopping;

namespace Metaspesa.RestApi.Shopping.GetShoppingLists;

internal static class GetShoppingListsEndpoint {
  public static IEndpointRouteBuilder MapGetShoppingListsEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapGet("", ListAsync)
      .WithName("GetShoppingLists")
      .Produces<ShoppingListCollectionResponse>()
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status500InternalServerError);
    return endpoints;
  }

  /// <summary>List the shopper's shopping lists.</summary>
  /// <remarks>Return named and temporary lists owned by the authenticated shopper. Requires the Shopper role.</remarks>
  /// <param name="context">Authenticated request context.</param>
  /// <param name="handler">Get shopping list summaries use case.</param>
  /// <param name="cancellationToken">Request cancellation token.</param>
  /// <response code="200">Shopping list summaries; the collection can be empty.</response>
  /// <response code="401">No valid authentication token was provided.</response>
  /// <response code="403">The authenticated user lacks the Shopper role.</response>
  /// <response code="500">An unexpected server or database failure occurred.</response>
  internal static async Task<IResult> ListAsync(
    HttpContext context, GetShoppingListSummaries.Handler handler,
    CancellationToken cancellationToken
  ) {
    IReadOnlyCollection<GetShoppingListSummaries.Response> lists = await handler.Handle(
      new GetShoppingListSummaries.Query(ShoppingUser.GetUid(context)),
      cancellationToken);
    return Results.Ok(new ShoppingListCollectionResponse(
      [.. lists.Select(ToSummary)]));
  }

  private static ShoppingListSummaryResponse ToSummary(
    GetShoppingListSummaries.Response list) =>
    new(list.Id, list.Name, list.IsTemporary);
}