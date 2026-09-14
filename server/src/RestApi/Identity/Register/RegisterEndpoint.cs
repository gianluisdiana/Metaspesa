using Metaspesa.Application.Identity;
using Metaspesa.Infrastructure;
using Metaspesa.RestApi.Security;

namespace Metaspesa.RestApi.Identity.Register;

internal static class RegisterEndpoint {
  public static IEndpointRouteBuilder MapRegisterEndpoint(
    this IEndpointRouteBuilder endpoints
  ) {
    endpoints.MapPost("/registrations", HandleAsync)
      .AddEndpointFilter<AllowedOriginFilter>();

    return endpoints;
  }

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