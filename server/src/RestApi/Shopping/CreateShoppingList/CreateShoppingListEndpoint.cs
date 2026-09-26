using Metaspesa.Infrastructure;
using Metaspesa.RestApi.Security;
using CreateListUseCase = Metaspesa.Application.Shopping.CreateShoppingList;

namespace Metaspesa.RestApi.Shopping.CreateShoppingList;

internal static class CreateShoppingListEndpoint {
  public static IEndpointRouteBuilder MapCreateShoppingListEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapPost("", CreateAsync)
      .WithName("CreateShoppingList")
      .Produces<IdResponse>(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status409Conflict)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status500InternalServerError)
      .AddEndpointFilter<AllowedOriginFilter>();
    return endpoints;
  }

  /// <summary>Create a shopping list.</summary>
  /// <remarks>Supply a name for a named list or null for a temporary list. Requires the Shopper role.</remarks>
  /// <param name="request">Name of the new list, or null for a temporary list.</param>
  /// <param name="context">Authenticated request context.</param>
  /// <param name="handler">Create shopping list use case.</param>
  /// <param name="cancellationToken">Request cancellation token.</param>
  /// <response code="201">List created; the response contains its ID and the Location header identifies it.</response>
  /// <response code="400">The list name is invalid.</response>
  /// <response code="409">A list with this name or a temporary list already exists.</response>
  /// <response code="401">No valid authentication token was provided.</response>
  /// <response code="403">The authenticated user lacks the Shopper role.</response>
  /// <response code="500">An unexpected server or database failure occurred.</response>
  internal static async Task<IResult> CreateAsync(
    CreateShoppingListRequest request, HttpContext context,
    CreateListUseCase.Handler handler,
    CancellationToken cancellationToken
  ) {
    Guid id = await handler.Handle(new CreateListUseCase.Command(
      ShoppingUser.GetUid(context),
      request.Name is null ? null : TextSanitizer.Sanitize(request.Name)),
      cancellationToken);
    return Results.Created($"/api/v1/shopping-lists/{id}", new IdResponse(id));
  }
}