using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Auth;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Protos.Auth;

namespace Metaspesa.GrpcApi.Services;

internal sealed class AuthGrpcService(
  ICommandHandler<RegisterUser.Command> registerHandler
) : AuthService.AuthServiceBase {
  public override async Task<Empty> Register(
    RegisterRequest request, ServerCallContext context
  ) {
    Result result = await registerHandler.Handle(
      new RegisterUser.Command(request.Username, request.Password),
      context.CancellationToken
    );

    result.ThrowRpcExceptionIfFailed();

    return new Empty();
  }
}
