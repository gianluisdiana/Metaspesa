using Grpc.Core;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Purchasing.Errors;
using Metaspesa.GrpcApi.Extensions;

namespace Metaspesa.GrpcApi.UnitTests.Purchasing;

public class PurchaseDomainExceptionExtensionsTests {
  [Fact(DisplayName = "Maps missing paid snapshot to NotFound")]
  public void ToRpcException_MapsMissingSnapshotToNotFound() {
    var exception = new PurchasePriceSnapshotNotFoundException(
      new ProductFormatId(7)).ToRpcException();

    Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    Assert.Equal("Purchase.PriceSnapshot.NotFound", exception.Status.Detail);
    Metadata.Entry detail = Assert.Single(exception.Trailers);
    Assert.DoesNotContain("7", detail.Value, StringComparison.Ordinal);
    Assert.Equal(
      "A price snapshot required for checkout was not found.",
      detail.Value);
  }

  [Fact(DisplayName = "Maps Purchasing validation to InvalidArgument")]
  public void ToRpcException_MapsValidationToInvalidArgument() {
    var exception = new EmptyPurchaseItemsException().ToRpcException();

    Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
  }
}
