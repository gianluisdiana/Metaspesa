using Metaspesa.Application.Abstractions.Markets;
using GetMarketsUseCase = Metaspesa.Application.Markets.GetMarkets;

namespace Metaspesa.RestApi.Markets.GetMarkets;

internal static class GetMarketsEndpoint {
  public static IEndpointRouteBuilder MapGetMarketsEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapGet("/markets", HandleAsync)
      .WithName("GetMarkets")
      .Produces<MarketListResponse>()
      .ProducesProblem(StatusCodes.Status500InternalServerError);
    return endpoints;
  }

  /// <summary>List registered markets.</summary>
  /// <remarks>Return markets available for catalog filtering and product display.</remarks>
  /// <response code="200">Available markets and their stable IDs.</response>
  /// <response code="500">An unexpected server or database failure occurred.</response>
  internal static async Task<IResult> HandleAsync(
    GetMarketsUseCase.Handler handler,
    CancellationToken cancellationToken
  ) {
    IReadOnlyCollection<MarketSummary> markets = await handler.Handle(
      cancellationToken);
    return Results.Ok(new MarketListResponse(
      [.. markets.Select(MarketResponse.FromSummary)]));
  }
}