using Metaspesa.Application.Identity;
using Metaspesa.Infrastructure;
using Metaspesa.RestApi.Security;

namespace Metaspesa.RestApi.Identity.Register;

internal static class RegisterEndpoint {
  public static IEndpointRouteBuilder MapRegisterEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapPost("/registrations", HandleAsync)
      .WithName("RegisterShopper")
      .Produces(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status409Conflict)
      .ProducesProblem(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status500InternalServerError)
      .AddEndpointFilter<AllowedOriginFilter>();

    return endpoints;
  }

  /// <summary>Register a shopper.</summary>
  /// <remarks>Create a shopper account. Requests must come from an allowed browser origin.</remarks>
  /// <response code="201">Account created; no response body.</response>
  /// <response code="400">Username or password violates registration rules, or the body is invalid.</response>
  /// <response code="403">The browser Origin header is missing or not allowed.</response>
  /// <response code="409">The username is already registered.</response>
  /// <response code="500">An unexpected server or database failure occurred.</response>
  internal static async Task<IResult> HandleAsync(
    CredentialsRequest request,
    RegisterUser.Handler handler,
    CancellationToken cancellationToken
  ) {
    await handler.Handle(
      new RegisterUser.Command(
        TextSanitizer.Sanitize(request.Username), request.Password),
      cancellationToken);

    return Results.Created();
  }
}