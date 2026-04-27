using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Domain.Users;
using NSubstitute;
using static Metaspesa.Application.Auth.LoginUser;

namespace Metaspesa.Application.UnitTests.Auth;

public class LoginUserHandlerTest {
  private readonly IUserRepository _userRepository;
  private readonly IHasher _hasher;
  private readonly ITokenProvider _tokenProvider;
  private readonly Handler _handler;

  public LoginUserHandlerTest() {
    _userRepository = Substitute.For<IUserRepository>();
    _hasher = Substitute.For<IHasher>();
    _tokenProvider = Substitute.For<ITokenProvider>();
    _handler = new Handler(_userRepository, _hasher, _tokenProvider);
  }

  [Fact(DisplayName = "Returns error when user does not exist")]
  public async Task Handler_ReturnsError_WhenUserDoesNotExist() {
    // Arrange
    var query = new Query("estela", "SecurePass1!");

    _userRepository
      .GetUserByUsernameAsync(query.Username, TestContext.Current.CancellationToken)
      .Returns((User?)null);

    // Act
    Result<Token> result = await _handler.Handle(query, TestContext.Current.CancellationToken);

    // Assert
    Assert.Contains(result.Errors, e =>
      e.Code == "User.InvalidCredentials" && e.Kind == ErrorKind.Unauthenticated);
  }

  [Fact(DisplayName = "Returns error when password does not match")]
  public async Task Handler_ReturnsError_WhenPasswordDoesNotMatch() {
    // Arrange
    var user = new User(Guid.CreateVersion7(), "estela", "hashed", Role.Shopper);
    var query = new Query("estela", "WrongPassword");

    _userRepository
      .GetUserByUsernameAsync(query.Username, TestContext.Current.CancellationToken)
      .Returns(user);

    _hasher.VerifyHash(query.Password, user.HashedPassword).Returns(false);

    // Act
    Result<Token> result = await _handler.Handle(query, TestContext.Current.CancellationToken);

    // Assert
    Assert.Contains(result.Errors, e =>
      e.Code == "User.InvalidCredentials" && e.Kind == ErrorKind.Unauthenticated);
  }

  [Fact(DisplayName = "Does not generate token when password does not match")]
  public async Task Handler_DoesNotGenerateToken_WhenPasswordDoesNotMatch() {
    // Arrange
    var user = new User(Guid.CreateVersion7(), "estela", "hashed", Role.Shopper);
    var query = new Query("estela", "WrongPassword");

    _userRepository
      .GetUserByUsernameAsync(query.Username, TestContext.Current.CancellationToken)
      .Returns(user);

    _hasher.VerifyHash(query.Password, user.HashedPassword).Returns(false);

    // Act
    await _handler.Handle(query, TestContext.Current.CancellationToken);

    // Assert
    _tokenProvider.DidNotReceive().GenerateToken(Arg.Any<User>());
  }

  [Fact(DisplayName = "Returns token when credentials are valid")]
  public async Task Handler_ReturnsToken_WhenCredentialsAreValid() {
    // Arrange
    var user = new User(Guid.CreateVersion7(), "estela", "hashed", Role.Shopper);
    var query = new Query("estela", "SecurePass1!");

    _userRepository
      .GetUserByUsernameAsync(query.Username, TestContext.Current.CancellationToken)
      .Returns(user);

    _hasher.VerifyHash(query.Password, user.HashedPassword).Returns(true);

    var expectedToken = new Token("jwt-value", DateTime.UtcNow.AddHours(1));
    _tokenProvider.GenerateToken(user).Returns(expectedToken);

    // Act
    Result<Token> result = await _handler.Handle(query, TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(expectedToken, result.Value);
  }

  [Fact(DisplayName = "Generates token with the loaded user")]
  public async Task Handler_GeneratesToken_WithLoadedUser() {
    // Arrange
    var user = new User(Guid.CreateVersion7(), "estela", "hashed", Role.Shopper);
    var query = new Query("estela", "SecurePass1!");

    _userRepository
      .GetUserByUsernameAsync(query.Username, TestContext.Current.CancellationToken)
      .Returns(user);

    _hasher.VerifyHash(query.Password, user.HashedPassword).Returns(true);

    _tokenProvider.GenerateToken(Arg.Any<User>())
      .Returns(new Token("token", DateTime.UtcNow));

    // Act
    await _handler.Handle(query, TestContext.Current.CancellationToken);

    // Assert
    _tokenProvider.Received(1).GenerateToken(user);
  }
}
