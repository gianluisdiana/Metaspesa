using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Identity.Errors;
using NSubstitute;
using static Metaspesa.Application.Identity.RegisterUser;

namespace Metaspesa.Application.UnitTests.Identity;

public class RegisterUserHandlerTest {
  private readonly IHasher _hasher;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly Handler _handler;

  public RegisterUserHandlerTest() {
    _hasher = Substitute.For<IHasher>();
    _userRepository = Substitute.For<IUserRepository>();
    _unitOfWork = Substitute.For<IUnitOfWork>();
    _hasher.Hash(Arg.Any<string>()).Returns("hashed");
    _handler = new Handler(_hasher, _userRepository, _unitOfWork);
  }

  [Fact(DisplayName = "Throws password validation exception when password is invalid")]
  public async Task Handler_ThrowsPasswordValidationException_WhenPasswordIsInvalid() {
    // Arrange
    var command = new Command("user", "password");

    // Act
    async Task action() => await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await Assert.ThrowsAsync<PasswordTooShortException>(action);
  }

  [Fact(DisplayName = "Does not hash password when validation fails")]
  public async Task Handler_DoesNotHashPassword_WhenValidationFails() {
    // Arrange
    var command = new Command("user", "password");

    // Act
    await Assert.ThrowsAsync<PasswordTooShortException>(
      async () => await _handler.Handle(command, TestContext.Current.CancellationToken));

    // Assert
    _hasher.DidNotReceive().Hash(Arg.Any<string>());
  }

  [Fact(DisplayName = "Throws conflict exception when username already exists")]
  public async Task Handler_ThrowsUsernameAlreadyExistsException_WhenUsernameAlreadyExists() {
    // Arrange
    const string Username = "estela";
    var command = new Command(Username, "SecurePass1!");
    _userRepository
      .CheckUsernameExistsAsync(
        Arg.Is<Username>(u => u.Value == Username),
        TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    async Task action() => await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await Assert.ThrowsAsync<UsernameAlreadyExistsException>(action);
  }

  [Fact(DisplayName = "Hashes password when validation passes")]
  public async Task Handler_HashesPassword_WhenValidationPasses() {
    // Arrange
    const string Password = "SecurePass1!";
    var command = new Command("estela", Password);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _hasher.Received(1).Hash(Password);
  }

  [Fact(DisplayName = "Saves user with hashed password via repository")]
  public async Task Handler_SavesUserWithHashedPassword_ViaRepository() {
    // Arrange
    const string HashedPassword = "hashed_value";
    var command = new Command("estela", "SecurePass1!");
    _hasher.Hash(Arg.Any<string>()).Returns(HashedPassword);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _userRepository.Received(1).SaveUser(
      Arg.Is<User>(u => u.PasswordHash.Value == HashedPassword));
  }

  [Fact(DisplayName = "Saves user as Shopper role via repository")]
  public async Task Handler_SavesUserAsShopperRole_ViaRepository() {
    // Arrange
    var command = new Command("estela", "SecurePass1!");

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _userRepository.Received(1).SaveUser(
      Arg.Is<User>(u => u.Role == Role.Shopper));
  }

  [Fact(DisplayName = "Saves changes to unit of work")]
  public async Task Handler_SavesChangesToUnitOfWork() {
    // Arrange
    var command = new Command("estela", "SecurePass1!");

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Completes when handling is successful")]
  public async Task Handler_Completes_WhenHandlingIsSuccessful() {
    // Arrange
    var command = new Command("estela", "SecurePass1!");

    // Act
    Exception? exception = await Record.ExceptionAsync(
      async () => await _handler.Handle(command, TestContext.Current.CancellationToken));

    // Assert
    Assert.Null(exception);
  }
}