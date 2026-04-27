using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Domain.Users;
using NSubstitute;
using static Metaspesa.Application.Auth.RegisterUser;

namespace Metaspesa.Application.UnitTests.Auth;

public class RegisterUserHandlerTest {
  private readonly IValidator<Command> _validator;
  private readonly IHasher _hasher;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly Handler _handler;

  public RegisterUserHandlerTest() {
    _validator = Substitute.For<IValidator<Command>>();
    _hasher = Substitute.For<IHasher>();
    _userRepository = Substitute.For<IUserRepository>();
    _unitOfWork = Substitute.For<IUnitOfWork>();
    _handler = new Handler(_validator, _hasher, _userRepository, _unitOfWork);
  }

  [Fact(DisplayName = "Returns errors when validation fails")]
  public async Task Handler_ReturnsErrors_WhenValidationFails() {
    // Arrange
    var command = new Command("user", "password");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
  }

  [Fact(DisplayName = "Does not hash password when validation fails")]
  public async Task Handler_DoesNotHashPassword_WhenValidationFails() {
    // Arrange
    var command = new Command("user", "password");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _hasher.DidNotReceive().Hash(Arg.Any<string>());
  }

  [Fact(DisplayName = "Does not save user when validation fails")]
  public async Task Handler_DoesNotSaveUser_WhenValidationFails() {
    // Arrange
    var command = new Command("user", "password");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _userRepository.DidNotReceive().SaveUser(Arg.Any<User>());
  }

  [Fact(DisplayName = "Hashes password when validation passes")]
  public async Task Handler_HashesPassword_WhenValidationPasses() {
    // Arrange
    const string Password = "SecurePass1!";
    var command = new Command("estela", Password);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

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
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _hasher.Hash(Arg.Any<string>()).Returns(HashedPassword);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _userRepository.Received(1).SaveUser(
      Arg.Is<User>(u => u.HashedPassword == HashedPassword));
  }

  [Fact(DisplayName = "Saves user as Shopper role via repository")]
  public async Task Handler_SavesUserAsShopperRole_ViaRepository() {
    // Arrange
    var command = new Command("estela", "SecurePass1!");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

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
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Returns success result when handling is successful")]
  public async Task Handler_ReturnsSuccessResult_WhenHandlingIsSuccessful() {
    // Arrange
    var command = new Command("estela", "SecurePass1!");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
  }
}
