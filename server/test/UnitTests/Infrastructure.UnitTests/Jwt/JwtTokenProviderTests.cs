using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Metaspesa.Infrastructure.UnitTests.Jwt;

public static class JwtTokenProviderTests {
  public class GenerateToken {
    private const string TestKey = "integration-test-secret-key-32-chars!!";
    private const string TestIssuer = "test-issuer";
    private const string TestAudience = "test-audience";
    private const int TestExpirationMinutes = 30;

    private static JwtTokenProvider CreateProvider(int expirationMinutes = TestExpirationMinutes) {
      JwtOptions opts = new() {
        Key = TestKey,
        KeySecretName = "unused-in-test",
        Issuer = TestIssuer,
        Audience = TestAudience,
        ExpirationMinutes = expirationMinutes,
      };
      return new JwtTokenProvider(Options.Create(opts));
    }

    private static User TestUser(Role role = Role.Shopper) =>
      new(Guid.CreateVersion7(), "testuser", "hashed", role);

    private static JwtSecurityToken ParseToken(string tokenValue) =>
      new JwtSecurityTokenHandler().ReadJwtToken(tokenValue);

    private readonly JwtTokenProvider _provider = CreateProvider();

    [Fact(DisplayName = "Returns a non-empty token value")]
    public void GenerateToken_ReturnsNonEmptyValue() {
      // Act
      Token token = _provider.GenerateToken(TestUser());

      // Assert
      Assert.NotEmpty(token.Value);
    }

    [Fact(DisplayName = "Includes NameIdentifier claim with user UID")]
    public void GenerateToken_IncludesNameIdentifierClaim_WithUserUid() {
      // Arrange
      User user = TestUser();

      // Act
      Token token = _provider.GenerateToken(user);
      JwtSecurityToken jwt = ParseToken(token.Value);

      // Assert
      string? uid = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
      Assert.Equal(user.Uid.ToString(), uid);
    }

    [Fact(DisplayName = "Includes Name claim with username")]
    public void GenerateToken_IncludesNameClaim_WithUsername() {
      // Arrange
      User user = TestUser();

      // Act
      Token token = _provider.GenerateToken(user);
      JwtSecurityToken jwt = ParseToken(token.Value);

      // Assert
      string? name = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
      Assert.Equal(user.Username, name);
    }

    [Fact(DisplayName = "Includes Role claim with user role")]
    public void GenerateToken_IncludesRoleClaim_WithUserRole() {
      // Arrange
      User user = TestUser(Role.ProductManager);

      // Act
      Token token = _provider.GenerateToken(user);
      JwtSecurityToken jwt = ParseToken(token.Value);

      // Assert
      string? role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
      Assert.Equal(Role.ProductManager.ToString(), role);
    }

    [Fact(DisplayName = "Sets issuer and audience from options")]
    public void GenerateToken_SetsIssuerAndAudience_FromOptions() {
      // Act
      Token token = _provider.GenerateToken(TestUser());
      JwtSecurityToken jwt = ParseToken(token.Value);

      // Assert
      Assert.Equal(TestIssuer, jwt.Issuer);
      Assert.Contains(TestAudience, jwt.Audiences);
    }

    [Fact(DisplayName = "Sets ExpiresAt approximately ExpirationMinutes from now")]
    public void GenerateToken_SetsExpiresAt_InConfiguredMinutes() {
      // Arrange
      DateTime before = DateTime.UtcNow;

      // Act
      Token token = _provider.GenerateToken(TestUser());
      DateTime after = DateTime.UtcNow;

      // Assert
      DateTime expectedMin = before.AddMinutes(TestExpirationMinutes);
      DateTime expectedMax = after.AddMinutes(TestExpirationMinutes);
      Assert.InRange(token.ExpiresAt, expectedMin, expectedMax);
    }

    [Fact(DisplayName = "Produces token that passes validation with correct key")]
    public void GenerateToken_ProducesToken_ThatPassesValidation() {
      // Arrange
      Token token = _provider.GenerateToken(TestUser());
      TokenValidationParameters validationParams = new() {
        ValidIssuer = TestIssuer,
        ValidAudience = TestAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
      };

      // Act
      Exception? exception = Record.Exception(() =>
        new JwtSecurityTokenHandler().ValidateToken(token.Value, validationParams, out _));

      // Assert
      Assert.Null(exception);
    }

    [Fact(DisplayName = "Produces token that fails validation with wrong key")]
    public void GenerateToken_ProducesToken_ThatFailsValidation_WithWrongKey() {
      // Arrange
      Token token = _provider.GenerateToken(TestUser());
      TokenValidationParameters wrongKeyParams = new() {
        ValidIssuer = TestIssuer,
        ValidAudience = TestAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes("wrong-key-that-is-also-32-chars!!")),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
      };

      // Act & Assert
      Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() =>
        new JwtSecurityTokenHandler().ValidateToken(token.Value, wrongKeyParams, out _));
    }

    [Fact(DisplayName = "Produces token that is expired after expiration window")]
    public void GenerateToken_ProducesToken_ExpiredAfterConfiguredWindow() {
      // Arrange
      JwtTokenProvider shortProvider = CreateProvider(expirationMinutes: -1);
      Token token = shortProvider.GenerateToken(TestUser());
      TokenValidationParameters validationParams = new() {
        ValidIssuer = TestIssuer,
        ValidAudience = TestAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
      };

      // Act & Assert
      Assert.Throws<SecurityTokenExpiredException>(() =>
        new JwtSecurityTokenHandler().ValidateToken(token.Value, validationParams, out _));
    }
  }
}
