using Metaspesa.Application.Abstractions.Users;

namespace Metaspesa.RestApi.Identity.CreateToken;

internal sealed record TokenResponse(
  string AccessToken,
  string TokenType,
  DateTime ExpiresAt) {
  internal static TokenResponse FromToken(Token token) =>
    new(token.Value, "Bearer", token.ExpiresAt);
};