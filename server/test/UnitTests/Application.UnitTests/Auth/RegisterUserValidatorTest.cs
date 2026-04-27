using FluentValidation.TestHelper;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Users;
using NSubstitute;
using static Metaspesa.Application.Auth.RegisterUser;

namespace Metaspesa.Application.UnitTests.Auth;

public class RegisterUserValidatorTest {
  private readonly IUserRepository _userRepository;
  private readonly Validator _validator;

  public RegisterUserValidatorTest() {
    _userRepository = Substitute.For<IUserRepository>();
    _validator = new Validator(_userRepository);
  }

  [Fact(DisplayName = "Fails when username is empty")]
  public async Task Validator_Fails_WhenUsernameIsEmpty() {
    // Arrange
    var command = new Command(string.Empty, "SecurePass1!");

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Username)
      .WithErrorCode("User.UsernameEmpty");
  }

  [Fact(DisplayName = "Fails when password is too short")]
  public async Task Validator_Fails_WhenPasswordIsTooShort() {
    // Arrange
    var command = new Command("estela", "Abc!12345");

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Password)
      .WithErrorCode("User.PasswordTooShort");
  }

  [Fact(DisplayName = "Fails when password has no uppercase letter")]
  public async Task Validator_Fails_WhenPasswordMissingUppercase() {
    // Arrange
    var command = new Command("estela", "nouppercas3!");

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Password)
      .WithErrorCode("User.PasswordMissingUppercase");
  }

  [Fact(DisplayName = "Fails when password has no lowercase letter")]
  public async Task Validator_Fails_WhenPasswordMissingLowercase() {
    // Arrange
    var command = new Command("estela", "NOLOWERCASE3!");

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Password)
      .WithErrorCode("User.PasswordMissingLowercase");
  }

  [Fact(DisplayName = "Fails when password has no digit")]
  public async Task Validator_Fails_WhenPasswordMissingDigit() {
    // Arrange
    var command = new Command("estela", "NoDigitHere!!");

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Password)
      .WithErrorCode("User.PasswordMissingDigit");
  }

  [Fact(DisplayName = "Fails when password has no special character")]
  public async Task Validator_Fails_WhenPasswordMissingSpecialChar() {
    // Arrange
    var command = new Command("estela", "NoSpecial1234");

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Password)
      .WithErrorCode("User.PasswordMissingSpecialChar");
  }

  [Fact(DisplayName = "Fails with Conflict error kind when username is already taken")]
  public async Task Validator_Fails_WhenUsernameAlreadyExists() {
    // Arrange
    const string Username = "estela";
    var command = new Command(Username, "SecurePass1!");
    _userRepository
      .CheckUsernameExistsAsync(Username, TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Username)
      .WithErrorCode("User.UsernameAlreadyExists")
      .WithCustomState(ErrorKind.Conflict);
  }

  [Fact(DisplayName = "Passes with valid credentials and unique username")]
  public async Task Validator_Passes_WhenCredentialsAreValid() {
    // Arrange
    const string Username = "estela";
    var command = new Command(Username, "SecurePass1!");
    _userRepository
      .CheckUsernameExistsAsync(Username, TestContext.Current.CancellationToken)
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }
}
