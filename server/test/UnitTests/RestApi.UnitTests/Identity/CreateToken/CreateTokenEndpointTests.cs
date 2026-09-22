using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Identity;
using Metaspesa.Domain.Identity;
using Metaspesa.RestApi.Identity;
using Metaspesa.RestApi.Identity.CreateToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;

namespace Metaspesa.RestApi.UnitTests.Identity.CreateToken;

public class CreateTokenEndpointTests {
  private class FakeLoginUserHandler : LoginUser.Handler {
    private readonly ITokenProvider _tokenProvider;
    public FakeLoginUserHandler(
      IUserRepository repository, IHasher hasher, ITokenProvider tokenProvider
    ) : base(repository, hasher, tokenProvider) {
      _tokenProvider = tokenProvider;
      var user = User.CreateShopper(
        new UserId(Guid.CreateVersion7()),
        new Username("estela"),
        new PasswordHash("hashed"));
      repository.GetUserAsync(
        Arg.Any<Username>(), TestContext.Current.CancellationToken)
        .Returns(user);
      hasher.HashPassword(Arg.Any<string>()).Returns(user.PasswordHash);
      _tokenProvider.GenerateToken(user).Returns(
        new Token("jwt-token", DateTime.UtcNow.AddHours(1)));
    }

    public void WithTokenExpiration(DateTime expiresAt) {
      _tokenProvider.GenerateToken(Arg.Any<User>()).Returns(
        new Token("jwt-token", expiresAt));
    }
  }

  private readonly LoginUser.Handler _handler;

  public CreateTokenEndpointTests() {
    IUserRepository repository = Substitute.For<IUserRepository>();
    IHasher hasher = Substitute.For<IHasher>();
    ITokenProvider tokenProvider = Substitute.For<ITokenProvider>();
    _handler = new FakeLoginUserHandler(repository, hasher, tokenProvider);
  }

  [Fact(DisplayName = "Machine login returns access token")]
  public async Task Handle_ReturnsAccessToken_WhenProductManagerLogsIn() {
    var request = new CredentialsRequest("estela", "SecurePass1!");

    IResult result = await CreateTokenEndpoint.HandleAsync(
      request,
      _handler,
      TestContext.Current.CancellationToken);

    Ok<TokenResponse> response = Assert.IsType<Ok<TokenResponse>>(result);
    Assert.Equal("jwt-token", response.Value?.AccessToken);
  }

  [Fact(DisplayName = "Machine login returns Bearer token type")]
  public async Task Handle_ReturnsBearerType_WhenProductManagerLogsIn() {
    var request = new CredentialsRequest("estela", "SecurePass1!");

    IResult result = await CreateTokenEndpoint.HandleAsync(
      request,
      _handler,
      TestContext.Current.CancellationToken);

    Ok<TokenResponse> response = Assert.IsType<Ok<TokenResponse>>(result);
    Assert.Equal("Bearer", response.Value?.TokenType);
  }

  [Fact(DisplayName = "Machine login returns token expiration")]
  public async Task Handle_ReturnsExpiration_WhenProductManagerLogsIn() {
    var expiresAt = new DateTime(2026, 9, 13, 12, 0, 0, DateTimeKind.Utc);
    var request = new CredentialsRequest("estela", "SecurePass1!");
    (_handler as FakeLoginUserHandler)!.WithTokenExpiration(expiresAt);

    IResult result = await CreateTokenEndpoint.HandleAsync(
      request,
      _handler,
      TestContext.Current.CancellationToken);

    Ok<TokenResponse> response = Assert.IsType<Ok<TokenResponse>>(result);
    Assert.Equal(expiresAt, response.Value?.ExpiresAt);
  }
}