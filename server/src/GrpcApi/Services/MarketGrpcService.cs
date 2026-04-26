using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Markets;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Protos.Markets;

namespace Metaspesa.GrpcApi.Services;

internal class MarketGrpcService(
  ICommandHandler<AddMarketProducts.Command> addProductsHandler
) : MarketService.MarketServiceBase {
  public override async Task<Empty> AddProducts(
    AddProductsRequest request, ServerCallContext context
  ) {
    DateOnly? registeredAt = request.RegisteredAt is null ?
      null :
      DateOnly.FromDateTime(request.RegisteredAt.ToDateTime());
    var command = new AddMarketProducts.Command(
      [.. request.Products.Select(p => new AddMarketProducts.CommandProduct(
        p.Name, p.Price, p.Quantity, p.MarketName, p.BrandName))],
      registeredAt);

    Result result = await addProductsHandler.Handle(command, context.CancellationToken);

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }
}
