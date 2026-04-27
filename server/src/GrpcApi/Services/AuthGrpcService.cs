using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Auth;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Protos.Auth;

namespace Metaspesa.GrpcApi.Services;

internal class AuthGrpcService(
  ICommandHandler<RegisterUser.Command> registerHandler,
  IQueryHandler<LoginUser.Query, Token> loginHandler
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

  public override async Task<LoginResponse> Login(
    LoginRequest request, ServerCallContext context
  ) {
    Result<Token> result = await loginHandler.Handle(
      new LoginUser.Query(request.Username, request.Password),
      context.CancellationToken
    );

    result.ThrowRpcExceptionIfFailed();

    return new LoginResponse {
      Token = result.Value.Value,
      ExpirationInUtc = result.Value.ExpiresAt.ToString("o"),
    };
  }
}
