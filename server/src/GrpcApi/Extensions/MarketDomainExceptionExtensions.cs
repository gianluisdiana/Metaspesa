using Grpc.Core;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.GrpcApi.Extensions;

internal static class MarketDomainExceptionExtensions {
  public static RpcException ToRpcException(this MarketDomainException exception) =>
    new(
      new Status(StatusCode.InvalidArgument, exception.Code),
      [new Metadata.Entry(exception.Code, exception.Message)]);
}