using Metaspesa.RestApi.Identity.CreateSession;
using Metaspesa.RestApi.Identity.CreateToken;
using Metaspesa.RestApi.Identity.Register;

namespace Metaspesa.RestApi.Identity;

internal static class IdentityEndpoints {
  public static IEndpointRouteBuilder MapIdentityEndpoints(
    this IEndpointRouteBuilder endpoints
  ) {
    RouteGroupBuilder auth = endpoints.MapGroup("/api/v1/auth");

    auth.MapRegisterEndpoint();
    auth.MapCreateSessionEndpoint();
    auth.MapCreateTokenEndpoint();

    return endpoints;
  }
}