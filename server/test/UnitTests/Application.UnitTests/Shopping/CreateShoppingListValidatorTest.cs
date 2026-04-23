using FluentValidation.TestHelper;
using Metaspesa.Application.Abstractions.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.CreateShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class CreateShoppingListValidatorTest {
  private readonly IShoppingRepository _shoppingRepository;
  private readonly Validator _validator;

  public CreateShoppingListValidatorTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _validator = new Validator(_shoppingRepository);
  }

  [Fact(DisplayName = "Fails when named list already exists for user")]
  public async Task Validator_Fails_WhenNamedListAlreadyExists() {
    // Arrange
    var userUid = Guid.NewGuid();
    const string Name = "Groceries";
    var command = new Command(userUid, Name);

    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, Name, TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ShoppingListName)
      .WithErrorCode("ShoppingList.AlreadyExists");
  }

  [Fact(DisplayName = "Fails when temporary list already exists for user")]
  public async Task Validator_Fails_WhenTemporaryListAlreadyExists() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, null);

    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ShoppingListName)
      .WithErrorCode("ShoppingList.AlreadyExists");
  }

  [Fact(DisplayName = "Passes when named list does not exist for user")]
  public async Task Validator_Passes_WhenNamedListDoesNotExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "New List");

    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "New List", TestContext.Current.CancellationToken)
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact(DisplayName = "Passes when temporary list does not exist for user")]
  public async Task Validator_Passes_WhenTemporaryListDoesNotExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, null);

    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }
}
