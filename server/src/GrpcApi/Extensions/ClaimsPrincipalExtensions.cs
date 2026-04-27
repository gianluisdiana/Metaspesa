using System.Security.Claims;

namespace Metaspesa.GrpcApi.Extensions;

internal static class ClaimsPrincipalExtensions {
  public static Guid GetUserUid(this HttpContext context) {
    return Guid.TryParse(
      context.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid guid
    ) ? guid : Guid.Empty;
  }
}