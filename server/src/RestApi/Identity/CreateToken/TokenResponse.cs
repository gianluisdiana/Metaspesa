using Metaspesa.Application.Abstractions.Users;

namespace Metaspesa.RestApi.Identity.CreateToken;

/// <summary>Bearer token issued after successful machine authentication.</summary>
/// <param name="AccessToken">JWT sent in the Authorization Bearer header.</param>
/// <param name="TokenType">Authorization scheme; always Bearer.</param>
/// <param name="ExpiresAt">UTC instant after which the token is invalid.</param>
internal sealed record TokenResponse(
  string AccessToken,
  string TokenType,
  DateTime ExpiresAt) {
  internal static TokenResponse FromToken(Token token) =>
    new(token.Value, "Bearer", token.ExpiresAt);
};