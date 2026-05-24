using FluentValidation.TestHelper;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.UpdateShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class UpdateShoppingListValidatorTest {
  private readonly IShoppingRepository _shoppingRepository;
  private readonly Validator _validator;

  public UpdateShoppingListValidatorTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _validator = new Validator(_shoppingRepository);
  }

  [Fact(DisplayName = "Fails when source shopping list does not exist")]
  public async Task Validator_Fails_WhenSourceShoppingListDoesNotExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, null, "Groceries");
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ShoppingListName)
      .WithErrorCode("ShoppingList.NotFound")
      .WithCustomState(ErrorKind.Missing);
  }

  [Fact(DisplayName = "Fails when new name already exists")]
  public async Task Validator_Fails_WhenNewNameAlreadyExists() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, null, "Groceries");
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Groceries", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.NewName)
      .WithErrorCode("ShoppingList.AlreadyExists")
      .WithCustomState(ErrorKind.Conflict);
  }

  [Fact(DisplayName = "Fails when no fields to update are provided")]
  public async Task Validator_Fails_WhenNoFieldsToUpdateAreProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, null, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.NewName)
      .WithErrorCode("ShoppingList.NoFieldsToUpdate");
  }

  [Fact(DisplayName = "Fails when new name is blank")]
  public async Task Validator_Fails_WhenNewNameIsBlank() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, null, " ");
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.NewName)
      .WithErrorCode("ShoppingList.NoFieldsToUpdate");
  }

  [Fact(DisplayName = "Passes when temporary list exists and new name is unused")]
  public async Task Validator_Passes_WhenTemporaryListExistsAndNewNameIsUnused() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, null, "Groceries");
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Groceries", TestContext.Current.CancellationToken)
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }
}