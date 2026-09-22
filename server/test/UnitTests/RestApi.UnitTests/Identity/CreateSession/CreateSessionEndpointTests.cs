using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Identity;
using Metaspesa.Domain.Identity;
using Metaspesa.Infrastructure;
using Metaspesa.RestApi.Identity;
using Metaspesa.RestApi.Identity.CreateSession;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Metaspesa.RestApi.UnitTests.Identity.CreateSession;

public class CreateSessionEndpointTests {
  private class FakeLoginUserHandler : LoginUser.Handler {
    public FakeLoginUserHandler(
      IUserRepository repository, IHasher hasher, ITokenProvider tokenProvider
    ) : base(repository, hasher, tokenProvider) {
      var user = User.CreateShopper(
        new UserId(Guid.CreateVersion7()),
        new Username("estela"),
        new PasswordHash("hashed"));
      repository.GetUserByUsernameAsync(
          Arg.Any<Username>(), TestContext.Current.CancellationToken)
        .Returns(user);
      hasher.VerifyHash(Arg.Any<string>(), user.PasswordHash.Value).Returns(true);
      tokenProvider.GenerateToken(user).Returns(
        new Token("jwt-token", DateTime.UtcNow.AddHours(1)));
    }
  }

  private readonly LoginUser.Handler _handler;
  private readonly DefaultHttpContext _context;

  public CreateSessionEndpointTests() {
    IUserRepository repository = Substitute.For<IUserRepository>();
    IHasher hasher = Substitute.For<IHasher>();
    ITokenProvider tokenProvider = Substitute.For<ITokenProvider>();
    _handler = new FakeLoginUserHandler(repository, hasher, tokenProvider);
    _context = new DefaultHttpContext();
  }

  [Fact(DisplayName = "Browser login returns 204 No Content")]
  public async Task Handle_ReturnsNoContent_WhenCredentialsAreValid() {
    var request = new CredentialsRequest("estela", "SecurePass1!");

    IResult result = await CreateSessionEndpoint.HandleAsync(
      request,
      _handler,
      _context,
      Options.Create(new SessionCookieOptions { CookieName = "metaspesa_session" }),
      TestContext.Current.CancellationToken);

    Assert.IsType<NoContent>(result);
  }

  [Fact(DisplayName = "Browser login uses configured session cookie name")]
  public async Task Handle_UsesConfiguredSessionCookieName_WhenCredentialsAreValid() {
    var request = new CredentialsRequest("estela", "SecurePass1!");

    await CreateSessionEndpoint.HandleAsync(
      request,
      _handler,
      _context,
      Options.Create(new SessionCookieOptions { CookieName = "custom_session" }),
      TestContext.Current.CancellationToken);

    string setCookie = _context.Response.Headers.SetCookie.ToString();
    Assert.StartsWith(
      "custom_session=jwt-token;", setCookie, StringComparison.Ordinal);
  }

  [Fact(DisplayName = "Browser login session cookie is HttpOnly")]
  public async Task Handle_SetsHttpOnlyCookie_WhenCredentialsAreValid() {
    var request = new CredentialsRequest("estela", "SecurePass1!");

    await CreateSessionEndpoint.HandleAsync(
      request,
      _handler,
      _context,
      Options.Create(new SessionCookieOptions { CookieName = "metaspesa_session" }),
      TestContext.Current.CancellationToken);

    string setCookie = _context.Response.Headers.SetCookie.ToString();
    Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
  }

  [Fact(DisplayName = "Browser login session cookie uses SameSite Lax")]
  public async Task Handle_SetsSameSiteLaxCookie_WhenCredentialsAreValid() {
    var request = new CredentialsRequest("estela", "SecurePass1!");

    await CreateSessionEndpoint.HandleAsync(
      request,
      _handler,
      _context,
      Options.Create(new SessionCookieOptions { CookieName = "metaspesa_session" }),
      TestContext.Current.CancellationToken);

    string setCookie = _context.Response.Headers.SetCookie.ToString();
    Assert.Contains("samesite=lax", setCookie, StringComparison.OrdinalIgnoreCase);
  }

  [Fact(DisplayName = "Browser login session cookie is always secure")]
  public async Task Handle_AlwaysSetsSecureCookie() {
    var request = new CredentialsRequest("estela", "SecurePass1!");

    await CreateSessionEndpoint.HandleAsync(
      request,
      _handler,
      _context,
      Options.Create(new SessionCookieOptions { CookieName = "metaspesa_session" }),
      TestContext.Current.CancellationToken);

    string setCookie = _context.Response.Headers.SetCookie.ToString();
    Assert.Contains("secure", setCookie, StringComparison.OrdinalIgnoreCase);
  }
}