using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Metaspesa.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Metaspesa.RestApi.UnitTests.Markets;

public static class IngestionAuthenticationTests {
  [Theory]
  [InlineData(true, false, true)]
  [InlineData(false, true, true)]
  [InlineData(false, false, false)]
  public static async Task Authenticate_DefaultSchemeAcceptsBearerHeaderOrCookie(
    bool bearer, bool cookie, bool expected
  ) {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
      "test-signing-key-at-least-thirty-two-bytes-long"));
    var services = new ServiceCollection();
    IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(
      new Dictionary<string, string?> {
        ["Jwt:Issuer"] = "test",
        ["Jwt:Audience"] = "test",
        ["Jwt:Key"] = "test-signing-key-at-least-thirty-two-bytes-long",
        ["Jwt:KeySecretName"] = "test-key",
        ["BrowserSession:CookieName"] = "session",
      }).Build();
    services.AddSingleton(configuration);
    services.AddLogging();
    services.AddInfrastructure();
    services.AddRestApi(configuration);
    await using ServiceProvider provider = services.BuildServiceProvider();
    var token = new JwtSecurityToken("test", "test",
      [new Claim(ClaimTypes.Role, "ProductManager")],
      new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
      new DateTime(9999, 1, 1, 0, 0, 0, DateTimeKind.Utc),
      new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
    string encoded = new JwtSecurityTokenHandler().WriteToken(token);
    var context = new DefaultHttpContext { RequestServices = provider };
    if (bearer) {
      context.Request.Headers.Authorization = $"Bearer {encoded}";
    }
    if (cookie) {
      context.Request.Headers.Cookie = $"session={encoded}";
    }

    AuthenticateResult result = await context.AuthenticateAsync();

    Assert.Equal(expected, result.Succeeded);
  }
}