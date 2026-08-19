using Grpc.Core;
using Metaspesa.Domain.Purchasing.Errors;

namespace Metaspesa.GrpcApi.Extensions;

internal static class PurchaseDomainExceptionExtensions {
  public static RpcException ToRpcException(this PurchaseDomainException exception) =>
    new(
      new Status(exception.GetStatusCode(), exception.Code),
      [new Metadata.Entry(exception.Code, exception.GetPublicMessage())]);

  private static string GetPublicMessage(this PurchaseDomainException exception) =>
    exception switch {
      PurchasePriceSnapshotNotFoundException =>
        "A price snapshot required for checkout was not found.",
      _ => exception.Message,
    };

  private static StatusCode GetStatusCode(this PurchaseDomainException exception) =>
    exception switch {
      PurchasePriceSnapshotNotFoundException => StatusCode.NotFound,
      _ => StatusCode.InvalidArgument,
    };
}