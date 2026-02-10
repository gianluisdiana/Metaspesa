using Grpc.Core;
using Metaspesa.Application.Abstractions.Core;

namespace Metaspesa.GrpcApi.Extensions;

internal static class ResultExtensions {
  public static void ThrowRpcExceptionIfFailed(this Result result) {
    if (!result.IsSuccess) {
      throw new RpcException(result.GetStatus(), result.GetMetadata());
    }
  }

  private static Status GetStatus(this Result result) =>
    new(result.GetStatusCode(), string.Empty);

  private static StatusCode GetStatusCode(this Result result) =>
    result.Errors.FirstOrDefault()?.Kind switch {
      null => StatusCode.OK,
      ErrorKind.Validation => StatusCode.InvalidArgument,
      _ => StatusCode.Internal,
    };

  private static Metadata GetMetadata(this Result result) => [
    .. result.Errors.Select(e => new Metadata.Entry(e.Code, e.Description))
  ];
}
