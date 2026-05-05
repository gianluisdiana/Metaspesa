using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Metaspesa.GrpcApi.Extensions;

internal static class HttpContextExtensions {
  public static Guid GetUserUid(this HttpContext context) {
    return Guid.TryParse(
      context.User.FindFirstValue(JwtRegisteredClaimNames.Sub), out Guid guid
    ) ? guid : Guid.Empty;
  }

  public static string GetPartitionKey(this HttpContext context) {
    return context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
      ?? context.Connection.RemoteIpAddress?.ToString()
      ?? "anonymous";
  }
}