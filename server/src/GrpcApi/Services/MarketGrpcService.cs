using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Markets;
using Metaspesa.Domain.Users;
using DomainMarketSummary = Metaspesa.Domain.Markets.MarketSummary;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Protos.Markets;
using Microsoft.AspNetCore.Authorization;
using DomainMarket = Metaspesa.Domain.Markets.Market;

namespace Metaspesa.GrpcApi.Services;

[Authorize]
internal class MarketGrpcService(
  ICommandHandler<AddMarketProducts.Command> addProductsHandler,
  IQueryHandler<GetMarketProducts.Query, PagedResult<DomainMarket>> getProductsHandler,
  IQueryHandler<GetMarkets.Query, IReadOnlyCollection<DomainMarketSummary>> getMarketsHandler
) : MarketService.MarketServiceBase {
  [Authorize(Roles = nameof(Role.ProductManager))]
  public override async Task<Empty> AddProducts(
    AddProductsRequest request, ServerCallContext context
  ) {
    var registeredAt = DateOnly.FromDateTime(
      request.RegisteredAt.ToDateTime());
    var command = new AddMarketProducts.Command(
      [.. request.Products.Select(p => new AddMarketProducts.CommandProduct(
        p.Name, p.Price, p.Quantity, p.MarketName, p.BrandName,
        string.IsNullOrEmpty(p.ImageUrl) ? null : new Uri(p.ImageUrl)))],
      registeredAt);

    Result result = await addProductsHandler.Handle(command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }

  [Authorize(Roles = nameof(Role.Shopper))]
  public override async Task<GetMarketProductsResponse> GetMarketProducts(
    GetMarketProductsRequest request, ServerCallContext context
  ) {
    Pagination? pagination = request.HasPage && request.HasPageSize
      ? new Pagination(request.Page, request.PageSize)
      : null;

    var filter = new GetMarketProductsFilter(
      request.HasMarketName ? request.MarketName : null,
      request.HasBrandName ? request.BrandName : null,
      request.HasNameSegment ? request.NameSegment : null,
      pagination);

    Result<PagedResult<DomainMarket>> result =
      await getProductsHandler.Handle(
        new GetMarketProducts.Query(filter), context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    var response = new GetMarketProductsResponse {
      TotalProducts = result.Value.TotalCount,
    };
    response.Markets.AddRange(result.Value.Values.Select(m => m.ToProto()));
    return response;
  }

  [Authorize(Roles = nameof(Role.Shopper))]
  public override async Task<GetMarketsResponse> GetMarkets(
    Empty request, ServerCallContext context
  ) {
    Result<IReadOnlyCollection<DomainMarketSummary>> result =
      await getMarketsHandler.Handle(new GetMarkets.Query(), context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    var response = new GetMarketsResponse();
    response.Markets.AddRange(result.Value.Select(m => m.ToProto()));
    return response;
  }
}
