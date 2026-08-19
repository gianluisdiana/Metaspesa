using Grpc.Core;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.GrpcApi.Extensions;

internal static class ShoppingDomainExceptionExtensions {
  public static RpcException ToRpcException(this ShoppingDomainException exception) =>
    new(
      new Status(exception.GetStatusCode(), exception.Code),
      [new Metadata.Entry(exception.Code, exception.Message)]);

  private static StatusCode GetStatusCode(this ShoppingDomainException exception) =>
    exception switch {
      ShoppingListAlreadyExistsException => StatusCode.AlreadyExists,
      ShoppingListNotFoundException or
      ShoppingItemNotFoundException or
      ShoppingProductFormatNotFoundException => StatusCode.NotFound,
      _ => StatusCode.InvalidArgument,
    };
}
