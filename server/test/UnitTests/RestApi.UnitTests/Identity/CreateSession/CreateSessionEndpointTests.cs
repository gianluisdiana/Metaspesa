using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Identity;
using Metaspesa.Domain.Identity;
using Metaspesa.Infrastructure;
using Metaspesa.RestApi.Identity;
using Metaspesa.RestApi.Identity.CreateSession;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Metaspesa.RestApi.UnitTests.Identity.CreateSession;

public static class CreateSessionEndpointTests {
  [Fact(DisplayName = "Browser login returns 204 No Content")]
  public static async Task Handle_ReturnsNoContent_WhenCredentialsAreValid() {
    IUserRepository repository = Substitute.For<IUserRepository>();
    IHasher hasher = Substitute.For<IHasher>();
    ITokenProvider tokenProvider = Substitute.For<ITokenProvider>();
    var user = User.CreateShopper(
      new UserId(Guid.CreateVersion7()),
      new Username("estela"),
      new PasswordHash("hashed"));
    repository.GetUserByUsernameAsync(
      Arg.Any<Username>(), TestContext.Current.CancellationToken)
      .Returns(user);
    hasher.VerifyHash("SecurePass1!", "hashed").Returns(true);
    tokenProvider.GenerateToken(user).Returns(
      new Token("jwt-token", DateTime.UtcNow.AddHours(1)));
    var handler = new LoginUser.Handler(repository, hasher, tokenProvider);
    var context = new DefaultHttpContext();
    IWebHostEnvironment environment = Substitute.For<IWebHostEnvironment>();
    environment.EnvironmentName.Returns(Environments.Development);

    IResult result = await CreateSessionEndpoint.HandleAsync(
      new CredentialsRequest("estela", "SecurePass1!"),
      handler,
      context,
      environment,
      Options.Create(new SessionCookieOptions { CookieName = "metaspesa_session" }),
      TestContext.Current.CancellationToken);

    Assert.IsType<NoContent>(result);
  }

  [Fact(DisplayName = "Browser login uses configured session cookie name")]
  public static async Task Handle_UsesConfiguredSessionCookieName_WhenCredentialsAreValid() {
    IUserRepository repository = Substitute.For<IUserRepository>();
    IHasher hasher = Substitute.For<IHasher>();
    ITokenProvider tokenProvider = Substitute.For<ITokenProvider>();
    var user = User.CreateShopper(
      new UserId(Guid.CreateVersion7()),
      new Username("estela"),
      new PasswordHash("hashed"));
    repository.GetUserByUsernameAsync(
      Arg.Any<Username>(), TestContext.Current.CancellationToken)
      .Returns(user);
    hasher.VerifyHash("SecurePass1!", "hashed").Returns(true);
    tokenProvider.GenerateToken(user).Returns(
      new Token("jwt-token", DateTime.UtcNow.AddHours(1)));
    var handler = new LoginUser.Handler(repository, hasher, tokenProvider);
    var context = new DefaultHttpContext();
    IWebHostEnvironment environment = Substitute.For<IWebHostEnvironment>();
    environment.EnvironmentName.Returns(Environments.Development);

    await CreateSessionEndpoint.HandleAsync(
      new CredentialsRequest("estela", "SecurePass1!"),
      handler,
      context,
      environment,
      Options.Create(new SessionCookieOptions { CookieName = "custom_session" }),
      TestContext.Current.CancellationToken);

    string setCookie = context.Response.Headers.SetCookie.ToString();
    Assert.StartsWith(
      "custom_session=jwt-token;", setCookie, StringComparison.Ordinal);
  }

  [Fact(DisplayName = "Browser login session cookie is HttpOnly")]
  public static async Task Handle_SetsHttpOnlyCookie_WhenCredentialsAreValid() {
    IUserRepository repository = Substitute.For<IUserRepository>();
    IHasher hasher = Substitute.For<IHasher>();
    ITokenProvider tokenProvider = Substitute.For<ITokenProvider>();
    var user = User.CreateShopper(
      new UserId(Guid.CreateVersion7()),
      new Username("estela"),
      new PasswordHash("hashed"));
    repository.GetUserByUsernameAsync(
      Arg.Any<Username>(), TestContext.Current.CancellationToken)
      .Returns(user);
    hasher.VerifyHash("SecurePass1!", "hashed").Returns(true);
    tokenProvider.GenerateToken(user).Returns(
      new Token("jwt-token", DateTime.UtcNow.AddHours(1)));
    var handler = new LoginUser.Handler(repository, hasher, tokenProvider);
    var context = new DefaultHttpContext();
    IWebHostEnvironment environment = Substitute.For<IWebHostEnvironment>();
    environment.EnvironmentName.Returns(Environments.Development);

    await CreateSessionEndpoint.HandleAsync(
      new CredentialsRequest("estela", "SecurePass1!"),
      handler,
      context,
      environment,
      Options.Create(new SessionCookieOptions { CookieName = "metaspesa_session" }),
      TestContext.Current.CancellationToken);

    string setCookie = context.Response.Headers.SetCookie.ToString();
    Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
  }

  [Fact(DisplayName = "Browser login session cookie uses SameSite Lax")]
  public static async Task Handle_SetsSameSiteLaxCookie_WhenCredentialsAreValid() {
    IUserRepository repository = Substitute.For<IUserRepository>();
    IHasher hasher = Substitute.For<IHasher>();
    ITokenProvider tokenProvider = Substitute.For<ITokenProvider>();
    var user = User.CreateShopper(
      new UserId(Guid.CreateVersion7()),
      new Username("estela"),
      new PasswordHash("hashed"));
    repository.GetUserByUsernameAsync(
      Arg.Any<Username>(), TestContext.Current.CancellationToken)
      .Returns(user);
    hasher.VerifyHash("SecurePass1!", "hashed").Returns(true);
    tokenProvider.GenerateToken(user).Returns(
      new Token("jwt-token", DateTime.UtcNow.AddHours(1)));
    var handler = new LoginUser.Handler(repository, hasher, tokenProvider);
    var context = new DefaultHttpContext();
    IWebHostEnvironment environment = Substitute.For<IWebHostEnvironment>();
    environment.EnvironmentName.Returns(Environments.Development);

    await CreateSessionEndpoint.HandleAsync(
      new CredentialsRequest("estela", "SecurePass1!"),
      handler,
      context,
      environment,
      Options.Create(new SessionCookieOptions { CookieName = "metaspesa_session" }),
      TestContext.Current.CancellationToken);

    string setCookie = context.Response.Headers.SetCookie.ToString();
    Assert.Contains("samesite=lax", setCookie, StringComparison.OrdinalIgnoreCase);
  }

  [Fact(DisplayName = "Browser login session cookie is secure outside development")]
  public static async Task Handle_SetsSecureCookie_WhenEnvironmentIsNotDevelopment() {
    IUserRepository repository = Substitute.For<IUserRepository>();
    IHasher hasher = Substitute.For<IHasher>();
    ITokenProvider tokenProvider = Substitute.For<ITokenProvider>();
    var user = User.CreateShopper(
      new UserId(Guid.CreateVersion7()),
      new Username("estela"),
      new PasswordHash("hashed"));
    repository.GetUserByUsernameAsync(
      Arg.Any<Username>(), TestContext.Current.CancellationToken)
      .Returns(user);
    hasher.VerifyHash("SecurePass1!", "hashed").Returns(true);
    tokenProvider.GenerateToken(user).Returns(
      new Token("jwt-token", DateTime.UtcNow.AddHours(1)));
    var handler = new LoginUser.Handler(repository, hasher, tokenProvider);
    var context = new DefaultHttpContext();
    IWebHostEnvironment environment = Substitute.For<IWebHostEnvironment>();
    environment.EnvironmentName.Returns(Environments.Production);

    await CreateSessionEndpoint.HandleAsync(
      new CredentialsRequest("estela", "SecurePass1!"),
      handler,
      context,
      environment,
      Options.Create(new SessionCookieOptions { CookieName = "metaspesa_session" }),
      TestContext.Current.CancellationToken);

    string setCookie = context.Response.Headers.SetCookie.ToString();
    Assert.Contains("secure", setCookie, StringComparison.OrdinalIgnoreCase);
  }
}