using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Identity;
using Metaspesa.Domain.Identity;
using Metaspesa.RestApi.Identity;
using Metaspesa.RestApi.Identity.CreateToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;

namespace Metaspesa.RestApi.UnitTests.Identity.CreateToken;

public static class CreateTokenEndpointTests {
  [Fact(DisplayName = "Machine login returns access token")]
  public static async Task Handle_ReturnsAccessToken_WhenProductManagerLogsIn() {
    IUserRepository repository = Substitute.For<IUserRepository>();
    IHasher hasher = Substitute.For<IHasher>();
    ITokenProvider tokenProvider = Substitute.For<ITokenProvider>();
    var user = User.Rehydrate(
      new UserId(Guid.CreateVersion7()),
      new Username("scraper"),
      new PasswordHash("hashed"),
      Role.ProductManager);
    repository.GetUserByUsernameAsync(
      Arg.Any<Username>(), TestContext.Current.CancellationToken)
      .Returns(user);
    hasher.VerifyHash("SecurePass1!", "hashed").Returns(true);
    var expiresAt = new DateTime(2026, 9, 13, 12, 0, 0, DateTimeKind.Utc);
    tokenProvider.GenerateToken(user).Returns(new Token("jwt-token", expiresAt));
    var handler = new LoginUser.Handler(repository, hasher, tokenProvider);

    IResult result = await CreateTokenEndpoint.HandleAsync(
      new CredentialsRequest("scraper", "SecurePass1!"),
      handler,
      TestContext.Current.CancellationToken);

    Ok<TokenResponse> response = Assert.IsType<Ok<TokenResponse>>(result);
    Assert.Equal("jwt-token", response.Value?.AccessToken);
  }

  [Fact(DisplayName = "Machine login returns Bearer token type")]
  public static async Task Handle_ReturnsBearerType_WhenProductManagerLogsIn() {
    IUserRepository repository = Substitute.For<IUserRepository>();
    IHasher hasher = Substitute.For<IHasher>();
    ITokenProvider tokenProvider = Substitute.For<ITokenProvider>();
    var user = User.Rehydrate(
      new UserId(Guid.CreateVersion7()),
      new Username("scraper"),
      new PasswordHash("hashed"),
      Role.ProductManager);
    repository.GetUserByUsernameAsync(
      Arg.Any<Username>(), TestContext.Current.CancellationToken)
      .Returns(user);
    hasher.VerifyHash("SecurePass1!", "hashed").Returns(true);
    var expiresAt = new DateTime(2026, 9, 13, 12, 0, 0, DateTimeKind.Utc);
    tokenProvider.GenerateToken(user).Returns(new Token("jwt-token", expiresAt));
    var handler = new LoginUser.Handler(repository, hasher, tokenProvider);

    IResult result = await CreateTokenEndpoint.HandleAsync(
      new CredentialsRequest("scraper", "SecurePass1!"),
      handler,
      TestContext.Current.CancellationToken);

    Ok<TokenResponse> response = Assert.IsType<Ok<TokenResponse>>(result);
    Assert.Equal("Bearer", response.Value?.TokenType);
  }

  [Fact(DisplayName = "Machine login returns token expiration")]
  public static async Task Handle_ReturnsExpiration_WhenProductManagerLogsIn() {
    IUserRepository repository = Substitute.For<IUserRepository>();
    IHasher hasher = Substitute.For<IHasher>();
    ITokenProvider tokenProvider = Substitute.For<ITokenProvider>();
    var user = User.Rehydrate(
      new UserId(Guid.CreateVersion7()),
      new Username("scraper"),
      new PasswordHash("hashed"),
      Role.ProductManager);
    repository.GetUserByUsernameAsync(
      Arg.Any<Username>(), TestContext.Current.CancellationToken)
      .Returns(user);
    hasher.VerifyHash("SecurePass1!", "hashed").Returns(true);
    var expiresAt = new DateTime(2026, 9, 13, 12, 0, 0, DateTimeKind.Utc);
    tokenProvider.GenerateToken(user).Returns(new Token("jwt-token", expiresAt));
    var handler = new LoginUser.Handler(repository, hasher, tokenProvider);

    IResult result = await CreateTokenEndpoint.HandleAsync(
      new CredentialsRequest("scraper", "SecurePass1!"),
      handler,
      TestContext.Current.CancellationToken);

    Ok<TokenResponse> response = Assert.IsType<Ok<TokenResponse>>(result);
    Assert.Equal(expiresAt, response.Value?.ExpiresAt);
  }
}