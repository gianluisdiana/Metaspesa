using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Markets;
using Metaspesa.Domain.Identity;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Protos.Markets;
using Microsoft.AspNetCore.Authorization;
using MarketSummaryModel = Metaspesa.Application.Abstractions.Markets.MarketSummary;

namespace Metaspesa.GrpcApi.Services;

[Authorize]
internal class MarketGrpcService(
  AddMarketProducts.Handler addProductsHandler,
  GetMarketProducts.Handler getProductsHandler,
  GetMarkets.Handler getMarketsHandler
) : MarketService.MarketServiceBase {
  [Authorize(Roles = nameof(Role.ProductManager))]
  public override async Task<Empty> AddProducts(
    AddProductsRequest request, ServerCallContext context
  ) {
    var registeredAt = DateOnly.FromDateTime(
      request.RegisteredAt.ToDateTime());
    var command = new AddMarketProducts.Command(
      [.. request.Products.Select(p => new AddMarketProducts.CommandProduct(
        GrpcTextSanitizer.SanitizeAscii(p.Name),
        GrpcPriceConverter.ToDecimal(p.Price),
        p.Quantity,
        GrpcTextSanitizer.SanitizeAscii(p.UnitOfMeasure),
        GrpcTextSanitizer.SanitizeAscii(p.MarketName),
        GrpcTextSanitizer.SanitizeAscii(p.BrandName),
        string.IsNullOrEmpty(p.ImageUrl) ? null : new Uri(p.ImageUrl)))],
      registeredAt);

    await addProductsHandler.Handle(command, context.CancellationToken);

    return new Empty();
  }

  [AllowAnonymous]
  public override async Task<GetMarketProductsResponse> GetMarketProducts(
    GetMarketProductsRequest request, ServerCallContext context
  ) {
    Pagination? pagination = request.HasPage && request.HasPageSize
      ? new Pagination(request.Page, request.PageSize)
      : null;

    var filter = new GetMarketProductsFilter(
      request.HasMarketName ? request.MarketName : null,
      request.HasBrandNameSegment ? request.BrandNameSegment : null,
      request.HasNameSegment ? request.NameSegment : null,
      pagination);

    PagedResult<MarketCatalog> result =
      await getProductsHandler.Handle(
        new GetMarketProducts.Query(filter), context.CancellationToken);

    var response = new GetMarketProductsResponse {
      TotalProducts = result.TotalCount,
    };
    response.Markets.AddRange(result.Values.Select(m => m.ToProto()));
    return response;
  }

  [AllowAnonymous]
  public override async Task<GetMarketsResponse> GetMarkets(
    Empty request, ServerCallContext context
  ) {
    IReadOnlyCollection<MarketSummaryModel> result =
      await getMarketsHandler.Handle(new GetMarkets.Query(), context.CancellationToken);

    var response = new GetMarketsResponse();
    response.Markets.AddRange(result.Select(m => m.ToProto()));
    return response;
  }
}
