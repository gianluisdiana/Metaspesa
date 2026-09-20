using Metaspesa.Application.Abstractions.Markets;
using ShoppingListUseCase = Metaspesa.Application.Shopping.GetShoppingList;

namespace Metaspesa.RestApi.Shopping.GetShoppingList;

internal static class GetShoppingListEndpoint {
  public static IEndpointRouteBuilder MapGetShoppingListEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapGet("/{listId:int}", GetAsync)
      .WithName("GetShoppingList")
      .Produces<ShoppingListResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status500InternalServerError);
    return endpoints;
  }

  /// <summary>Get a shopping list.</summary>
  /// <remarks>Return the list and its items with current product details for its owner. Requires the Shopper role.</remarks>
  /// <param name="listId">ID of the shopping list to retrieve.</param>
  /// <param name="context">Authenticated request context.</param>
  /// <param name="handler">Get shopping list use case.</param>
  /// <param name="cancellationToken">Request cancellation token.</param>
  /// <response code="200">The shopping list and its items.</response>
  /// <response code="400">The list ID is invalid.</response>
  /// <response code="404">The list or a referenced product format was not found.</response>
  /// <response code="401">No valid authentication token was provided.</response>
  /// <response code="403">The authenticated user lacks the Shopper role.</response>
  /// <response code="500">An unexpected server or database failure occurred.</response>
  internal static async Task<IResult> GetAsync(
    int listId, HttpContext context,
    ShoppingListUseCase.Handler handler,
    CancellationToken cancellationToken
  ) {
    ShoppingListUseCase.Response detail = await handler.Handle(
      new ShoppingListUseCase.Query(
        ShoppingUser.GetUid(context), listId),
      cancellationToken);
    return Results.Ok(new ShoppingListResponse(
      detail.Id, detail.ShoppingListName, detail.IsTemporary,
      [.. detail.Items.Select(item => {
        MarketProductFormat format = item.Format;
        MarketSummary market = item.Market ??
          throw new InvalidOperationException("Shopping product market is missing.");
        return new ShoppingItemResponse(
          format.ProductFormatUid, item.ProductName,
          item.BrandName,
          new ShoppingMarketResponse(market.Id, market.Name),
          new ShoppingQuantityResponse(format.Quantity.Amount,
            format.Quantity.UnitOfMeasure.Value),
          new ShoppingMoneyResponse(format.Price.Amount, "EUR"),
          item.Amount, item.IsChecked,
          format.ImageUrl?.ToString());
      })]));
  }
}