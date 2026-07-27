using Grpc.Core;
using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.GrpcApi.Extensions;

internal static class IdentityDomainExceptionExtensions {
  public static RpcException ToRpcException(this IdentityDomainException exception) =>
    new(exception.GetStatus(), exception.GetMetadata());

  private static Status GetStatus(this IdentityDomainException exception) =>
    new(exception.GetStatusCode(), exception.Code);

  private static StatusCode GetStatusCode(this IdentityDomainException exception) =>
    exception switch {
      UsernameAlreadyExistsException => StatusCode.AlreadyExists,
      InvalidCredentialsException => StatusCode.Unauthenticated,
      _ => StatusCode.InvalidArgument,
    };

  private static Metadata GetMetadata(this IdentityDomainException exception) => [
    new(exception.Code, exception.Message),
  ];
}
