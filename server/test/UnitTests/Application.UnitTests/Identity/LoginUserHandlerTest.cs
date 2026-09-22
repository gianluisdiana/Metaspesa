using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Identity.Errors;
using NSubstitute;
using static Metaspesa.Application.Identity.LoginUser;

namespace Metaspesa.Application.UnitTests.Identity;

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

  [Fact(DisplayName = "Throws invalid credentials when user does not exist")]
  public async Task Handler_ThrowsInvalidCredentialsException_WhenUserDoesNotExist() {
    // Arrange
    var query = new Query("estela", "SecurePass1!");

    _userRepository
      .GetUserAsync(
        Arg.Is<Username>(u => u.Value == query.Username),
        TestContext.Current.CancellationToken)
      .Returns((User?)null);

    // Act
    async Task action() => await _handler.Handle(query, TestContext.Current.CancellationToken);

    // Assert
    await Assert.ThrowsAsync<InvalidCredentialsException>(action);
  }

  [Fact(DisplayName = "Throws invalid credentials when password does not match")]
  public async Task Handler_ThrowsInvalidCredentialsException_WhenPasswordDoesNotMatch() {
    // Arrange
    var user = User.CreateShopper(
      new UserId(Guid.CreateVersion7()),
      new Username("estela"),
      new PasswordHash("hashed"));
    var query = new Query("estela", "WrongPassword");

    _userRepository
      .GetUserAsync(
        Arg.Is<Username>(u => u.Value == query.Username),
        TestContext.Current.CancellationToken)
      .Returns(user);

    _hasher.HashPassword(query.Password).Returns(new PasswordHash("other hashed"));

    // Act
    async Task action() => await _handler.Handle(query, TestContext.Current.CancellationToken);

    // Assert
    await Assert.ThrowsAsync<InvalidCredentialsException>(action);
  }

  [Fact(DisplayName = "Does not generate token when password does not match")]
  public async Task Handler_DoesNotGenerateToken_WhenPasswordDoesNotMatch() {
    // Arrange
    var user = User.CreateShopper(
      new UserId(Guid.CreateVersion7()),
      new Username("estela"),
      new PasswordHash("hashed"));
    var query = new Query("estela", "WrongPassword");

    _userRepository
      .GetUserAsync(
        Arg.Is<Username>(u => u.Value == query.Username),
        TestContext.Current.CancellationToken)
      .Returns(user);

    _hasher.HashPassword(query.Password).Returns(new PasswordHash("other hashed"));

    // Act
    await Assert.ThrowsAsync<InvalidCredentialsException>(
      async () => await _handler.Handle(query, TestContext.Current.CancellationToken));

    // Assert
    _tokenProvider.DidNotReceive().GenerateToken(Arg.Any<User>());
  }

  [Fact(DisplayName = "Returns token when credentials are valid")]
  public async Task Handler_ReturnsToken_WhenCredentialsAreValid() {
    // Arrange
    var user = User.CreateShopper(
      new UserId(Guid.CreateVersion7()),
      new Username("estela"),
      new PasswordHash("hashed"));
    var query = new Query("estela", "SecurePass1!");

    _userRepository
      .GetUserAsync(
        Arg.Is<Username>(u => u.Value == query.Username),
        TestContext.Current.CancellationToken)
      .Returns(user);

    _hasher.HashPassword(query.Password).Returns(user.PasswordHash);

    var expectedToken = new Token("jwt-value", DateTime.UtcNow.AddHours(1));
    _tokenProvider.GenerateToken(user).Returns(expectedToken);

    // Act
    Token result = await _handler.Handle(query, TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(expectedToken, result);
  }

  [Fact(DisplayName = "Generates token with the loaded user")]
  public async Task Handler_GeneratesToken_WithLoadedUser() {
    // Arrange
    var user = User.CreateShopper(
      new UserId(Guid.CreateVersion7()),
      new Username("estela"),
      new PasswordHash("hashed"));
    var query = new Query("estela", "SecurePass1!");

    _userRepository
      .GetUserAsync(
        Arg.Is<Username>(u => u.Value == query.Username),
        TestContext.Current.CancellationToken)
      .Returns(user);

    _hasher.HashPassword(query.Password).Returns(user.PasswordHash);

    _tokenProvider.GenerateToken(Arg.Any<User>())
      .Returns(new Token("token", DateTime.UtcNow));

    // Act
    await _handler.Handle(query, TestContext.Current.CancellationToken);

    // Assert
    _tokenProvider.Received(1).GenerateToken(user);
  }
}