using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Identity;
using Metaspesa.Infrastructure;

namespace Metaspesa.RestApi.Identity.CreateToken;

internal static class CreateTokenEndpoint {
  public static IEndpointRouteBuilder MapCreateTokenEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapPost("/tokens", HandleAsync);

    return endpoints;
  }

  internal static async Task<IResult> HandleAsync(
    CredentialsRequest request,
    LoginUser.Handler handler,
    CancellationToken cancellationToken
  ) {
    Token token = await handler.Handle(
      new LoginUser.Query(
        TextSanitizer.Sanitize(request.Username),
        request.Password),
      cancellationToken);

    return Results.Ok(TokenResponse.FromToken(token));
  }
}