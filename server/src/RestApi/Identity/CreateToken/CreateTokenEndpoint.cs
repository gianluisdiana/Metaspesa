using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Identity;
using Metaspesa.Infrastructure;

namespace Metaspesa.RestApi.Identity.CreateToken;

internal static class CreateTokenEndpoint {
  public static IEndpointRouteBuilder MapCreateTokenEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapPost("/tokens", HandleAsync)
      .WithName("CreateMachineToken")
      .Produces<TokenResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status500InternalServerError);

    return endpoints;
  }

  /// <summary>Create a machine token.</summary>
  /// <remarks>Authenticate credentials and return a bearer token for non-browser clients.</remarks>
  /// <response code="200">Authenticated. Returns a bearer token and its expiry.</response>
  /// <response code="400">The request body is invalid.</response>
  /// <response code="401">The username or password is incorrect.</response>
  /// <response code="500">An unexpected server or database failure occurred.</response>
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