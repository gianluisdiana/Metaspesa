using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Metaspesa.RestApi.Shopping;

internal static class ShoppingUser {
  public static Guid GetUid(HttpContext context) =>
    Guid.TryParse(context.User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
      context.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid id)
      ? id : throw new UnauthorizedAccessException("Missing user ID claim.");
}