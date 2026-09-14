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
      .AddEndpointFilter<AllowedOriginFilter>();

    return endpoints;
  }

  internal static async Task<IResult> HandleAsync(
    CredentialsRequest request,
    LoginUser.Handler handler,
    HttpContext context,
    IHostEnvironment environment,
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
        Secure = !environment.IsDevelopment(),
      });

    return Results.NoContent();
  }
}