using System.Globalization;
using Metaspesa.Application.Markets;
using Metaspesa.Domain.Identity;
using Metaspesa.Infrastructure;

namespace Metaspesa.RestApi.Markets;

internal static class SnapshotEndpoint {
  public static IEndpointRouteBuilder MapSnapshotEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapPost("/api/v1/markets/{marketName}/snapshots/{date}", HandleAsync)
      .RequireAuthorization(policy => policy
        .RequireAuthenticatedUser()
        .RequireRole(nameof(Role.ProductManager)));
    return endpoints;
  }

  internal static async Task<IResult> HandleAsync(
    string marketName,
    string date,
    SnapshotRequest request,
    AddMarketProducts.Handler handler,
    CancellationToken cancellationToken
  ) {
    if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
      DateTimeStyles.None, out DateOnly registeredAt)) {
      throw new BadHttpRequestException("Snapshot date must use YYYY-MM-DD.");
    }
    AddMarketProducts.Command command = ToCommand(marketName, registeredAt, request);
    await handler.Handle(command, cancellationToken);
    return Results.NoContent();
  }

  internal static AddMarketProducts.Command ToCommand(
    string marketName, DateOnly registeredAt, SnapshotRequest request
  ) {
    if (request.Items is null || request.Items.Any(item => item is null)) {
      throw new BadHttpRequestException("Items must be an array of product observations.");
    }
    var products = new List<AddMarketProducts.CommandProduct>();
    foreach (SnapshotItem? item in request.Items) {
      Uri? imageUrl = null;
      if (!string.IsNullOrEmpty(item!.ImageUrl) &&
        !Uri.TryCreate(item.ImageUrl, UriKind.Absolute, out imageUrl)) {
        throw new BadHttpRequestException("Image URL must be an absolute URL.");
      }
      products.Add(new AddMarketProducts.CommandProduct(
        item.Name is null ? null : TextSanitizer.Sanitize(item.Name),
        item.Price,
        item.Quantity,
        item.UnitOfMeasure is null ? null : TextSanitizer.Sanitize(item.UnitOfMeasure),
        TextSanitizer.Sanitize(marketName),
        item.BrandName is null ? null : TextSanitizer.Sanitize(item.BrandName),
        imageUrl));
    }
    return new AddMarketProducts.Command(products, registeredAt);
  }
}