using System.Globalization;
using System.Text.Json.Nodes;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Markets;
using Metaspesa.Domain.Markets;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi;

namespace Metaspesa.RestApi.Markets.GetProducts;

internal static class GetProductsEndpoint {
  public static IEndpointRouteBuilder MapGetProductsEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapGet("/products", GetProductsAsync)
      .WithName("GetProducts")
      .Produces<ProductPageResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status500InternalServerError)
      .AddOpenApiOperationTransformer((operation, _, _) => {
        operation.Parameters = [
          QueryParameter("query", JsonSchemaType.String,
            description: "Case-insensitive product-name fragment. Omit to include all names."),
          QueryParameter("marketId", JsonSchemaType.Array,
            new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" },
            description: "Repeat to include products from several markets. IDs must be UUIDs."),
          QueryParameter("brand", JsonSchemaType.String,
            description: "Case-insensitive brand-name fragment. Omit to include all brands."),
          QueryParameter("page", JsonSchemaType.Integer, minimum: 1,
            defaultValue: 1, description: "One-based page number; defaults to 1."),
          QueryParameter("pageSize", JsonSchemaType.Integer, minimum: 1,
            maximum: Pagination.MaximumSize, defaultValue: 24,
            description: "Maximum products per page, from 1 to 100; defaults to 24."),
          QueryParameter("sort", JsonSchemaType.String,
            allowedValues: ["name", "priceAsc", "priceDesc"],
            description: "Sort by name or lowest latest format price. Defaults to name."),
        ];
        return Task.CompletedTask;
      });
    return endpoints;
  }

  /// <summary>Search catalog products.</summary>
  /// <remarks>Search market-owned products with optional filters. Each result includes formats with their latest observed prices. Pagination counts products, not formats.</remarks>
  /// <response code="200">Matching products and pagination counts. An empty page has no items.</response>
  /// <response code="400">A filter, market ID, page value, or sort value is invalid.</response>
  /// <response code="500">An unexpected server or database failure occurred.</response>
  internal static async Task<IResult> GetProductsAsync(
    HttpRequest request,
    GetMarketProducts.Handler handler,
    CancellationToken cancellationToken
  ) {
    GetMarketProductsFilter filter = ParseFilter(request.Query);
    PagedResult<CatalogProduct> result = await handler.Handle(
      new GetMarketProducts.Query(filter), cancellationToken);
    return Results.Ok(new ProductPageResponse(
      [.. result.Values.Select(ToProduct)], filter.Pagination.Index,
      filter.Pagination.Size, result.TotalCount,
      (int)Math.Ceiling((double)result.TotalCount / filter.Pagination.Size)));
  }

  internal static GetMarketProductsFilter ParseFilter(IQueryCollection query) {
    Pagination pagination;
    try {
      pagination = new Pagination(
        ParseInteger(query, "page", 1), ParseInteger(query, "pageSize", 24));
    } catch (ArgumentOutOfRangeException exception) {
      throw new BadHttpRequestException(exception.Message);
    }
    MarketId[] marketIds = query.TryGetValue("marketId", out StringValues marketValues)
      ? [.. marketValues.Select(ParseMarketId)]
      : [];
    string? sortValue = SingleValue(query, "sort");
    CatalogSort sort = sortValue switch {
      null or "name" => CatalogSort.Name,
      "priceAsc" => CatalogSort.PriceAsc,
      "priceDesc" => CatalogSort.PriceDesc,
      _ => throw new BadHttpRequestException("Sort must be name, priceAsc, or priceDesc."),
    };
    return new GetMarketProductsFilter(
      SingleValue(query, "query"), marketIds,
      SingleValue(query, "brand"), pagination, sort);
  }

  private static MarketId ParseMarketId(string? value) =>
    Guid.TryParse(value, out Guid id)
      ? new MarketId(id)
      : throw new BadHttpRequestException("marketId must be a UUID.");

  private static int ParseInteger(
    IQueryCollection query, string key, int defaultValue
  ) {
    string? value = SingleValue(query, key);
    if (value is null) {
      return defaultValue;
    }
    return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture,
      out int parsed)
      ? parsed : throw new BadHttpRequestException($"{key} must be an integer.");
  }

  private static string? SingleValue(IQueryCollection query, string key) {
    if (!query.TryGetValue(key, out StringValues values)) {
      return null;
    }
    if (values.Count != 1) {
      throw new BadHttpRequestException($"{key} must appear once.");
    }
    return values[0];
  }

  private static ProductResponse ToProduct(CatalogProduct product) => new(
    product.Id, product.Name, product.Brand,
    MarketResponse.FromSummary(product.Market),
    [.. product.Formats.Select(format => new FormatResponse(
      format.Id, new QuantityResponse(format.Quantity, format.Unit),
      new MoneyResponse(format.Price, format.Currency),
      format.ObservedAt) { ImageUrl = format.ImageUrl?.ToString() })]);

  private static OpenApiParameter QueryParameter(
    string name, JsonSchemaType type, OpenApiSchema? items = null,
    int? minimum = null, int? maximum = null, int? defaultValue = null,
    IReadOnlyCollection<string>? allowedValues = null, string? description = null
  ) => new() {
    Name = name,
    Description = description,
    In = ParameterLocation.Query,
    Required = false,
    Style = ParameterStyle.Form,
    Explode = true,
    Schema = new OpenApiSchema {
      Type = type,
      Items = items,
      Minimum = minimum?.ToString(CultureInfo.InvariantCulture),
      Maximum = maximum?.ToString(CultureInfo.InvariantCulture),
      Default = defaultValue is null ? null : JsonValue.Create(defaultValue.Value),
      Enum = allowedValues is null ? null :
        [.. allowedValues.Select(value => JsonValue.Create(value))],
    },
  };
}