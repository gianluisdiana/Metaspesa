using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Identity;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Protos.Auth;

namespace Metaspesa.GrpcApi.Services;

internal class IdentityGrpcService(
  RegisterUser.Handler registerHandler,
  LoginUser.Handler loginHandler
) : AuthService.AuthServiceBase {
  public override async Task<Empty> Register(
    RegisterRequest request, ServerCallContext context
  ) {
    await registerHandler.Handle(
      new RegisterUser.Command(
        GrpcTextSanitizer.SanitizeAscii(request.Username),
        request.Password),
      context.CancellationToken
    );

    return new Empty();
  }

  public override async Task<LoginResponse> Login(
    LoginRequest request, ServerCallContext context
  ) {
    Token token = await loginHandler.Handle(
      new LoginUser.Query(
        GrpcTextSanitizer.SanitizeAscii(request.Username),
        request.Password),
      context.CancellationToken
    );

    return new LoginResponse {
      Token = token.Value,
      ExpirationInUtc = token.ExpiresAt.ToString("o"),
    };
  }
}