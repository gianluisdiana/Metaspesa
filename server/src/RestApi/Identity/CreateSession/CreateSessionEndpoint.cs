using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Identity;
using Metaspesa.Infrastructure;
using Metaspesa.RestApi.Security;
using Microsoft.Extensions.Options;

namespace Metaspesa.RestApi.Identity.CreateSession;

internal static class CreateSessionEndpoint {
  public static IEndpointRouteBuilder MapCreateSessionEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapPost("/sessions", HandleAsync)
      .WithName("CreateBrowserSession")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status500InternalServerError)
      .AddEndpointFilter<AllowedOriginFilter>();

    return endpoints;
  }

  /// <summary>Create a browser session.</summary>
  /// <remarks>Authenticate a shopper and set an HTTP-only session cookie. Requests must come from an allowed browser origin.</remarks>
  /// <response code="204">Authenticated. A session cookie is set; no response body.</response>
  /// <response code="400">The request body is invalid.</response>
  /// <response code="401">The username or password is incorrect.</response>
  /// <response code="403">The browser Origin header is missing or not allowed.</response>
  /// <response code="500">An unexpected server or database failure occurred.</response>
  internal static async Task<IResult> HandleAsync(
    CredentialsRequest request,
    LoginUser.Handler handler,
    HttpContext context,
    IOptions<SessionCookieOptions> sessionOptions,
    CancellationToken cancellationToken
  ) {
    Token token = await handler.Handle(
      new LoginUser.Query(
        TextSanitizer.Sanitize(request.Username),
        request.Password),
      cancellationToken);
    context.Response.Cookies.Append(
      sessionOptions.Value.CookieName,
      token.Value,
      new CookieOptions {
        Expires = token.ExpiresAt,
        HttpOnly = true,
        Path = "/",
        SameSite = SameSiteMode.Lax,
        Secure = true,
      });

    return Results.NoContent();
  }
}