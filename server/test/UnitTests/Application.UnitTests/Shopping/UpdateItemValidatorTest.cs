using FluentValidation.TestHelper;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.UpdateItem;

namespace Metaspesa.Application.UnitTests.Shopping;

public class UpdateItemValidatorTest {
  private readonly IShoppingRepository _shoppingRepository;
  private readonly Validator _validator;

  public UpdateItemValidatorTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _validator = new Validator(_shoppingRepository);
  }

  [Fact(DisplayName = "Fails when shopping list does not exist")]
  public async Task Validator_Fails_WhenShoppingListDoesNotExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Nonexistent", 10, 2, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Nonexistent", TestContext.Current.CancellationToken)
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ShoppingListName)
      .WithErrorCode("ShoppingList.NotFound")
      .WithErrorMessage($"User {userUid} doesn't have a shopping list named 'Nonexistent'.")
      .WithCustomState(ErrorKind.Missing);
  }

  [Fact(DisplayName = "Fails with temporary list message when list name is null")]
  public async Task Validator_Fails_WithTemporaryListMessage_WhenListNameIsNull() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, null, 10, 2, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ShoppingListName)
      .WithErrorCode("ShoppingList.NotFound")
      .WithErrorMessage($"User {userUid} doesn't have a temporary shopping list.")
      .WithCustomState(ErrorKind.Missing);
  }

  [Fact(DisplayName = "Fails when no fields are provided")]
  public async Task Validator_Fails_WhenNoFieldsAreProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, null, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x)
      .WithErrorCode("ShoppingList.Item.NoFieldsToUpdate")
      .WithErrorMessage("At least one field must be provided to update the item.");
  }

  [Fact(DisplayName = "Fails when amount is zero")]
  public async Task Validator_Fails_WhenAmountIsZero() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, 0, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Amount)
      .WithErrorCode("ShoppingList.Item.InvalidAmount")
      .WithErrorMessage("Amount must be greater than zero.");
  }

  [Fact(DisplayName = "Fails when amount is negative")]
  public async Task Validator_Fails_WhenAmountIsNegative() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, -1, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Amount)
      .WithErrorCode("ShoppingList.Item.InvalidAmount")
      .WithErrorMessage("Amount must be greater than zero.");
  }

  [Fact(DisplayName = "Passes when amount is provided")]
  public async Task Validator_Passes_WhenAmountIsProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, 2, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Theory(DisplayName = "Passes when checked state is provided")]
  [InlineData(true)]
  [InlineData(false)]
  public async Task Validator_Passes_WhenCheckedStateIsProvided(bool isChecked) {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, null, isChecked);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }
}
