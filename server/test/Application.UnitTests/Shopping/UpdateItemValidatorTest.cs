using FluentValidation.TestHelper;
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
    var command = new Command(userUid, "Nonexistent", "Milk", null, null, null, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Nonexistent", TestContext.Current.CancellationToken)
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ShoppingListName)
      .WithErrorCode("ShoppingList.NotFound");
  }

  [Fact(DisplayName = "Doesn't fail when item does not exist in the list")]
  public async Task Validator_DoesNotFail_WhenItemDoesNotExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Nonexistent", null, null, null, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckItemExistsAsync(userUid, "Weekly", "Nonexistent", TestContext.Current.CancellationToken)
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x.OriginalItemName);
  }

  [Fact(DisplayName = "Fails when new name already exists in the list")]
  public async Task Validator_Fails_WhenNewNameAlreadyExists() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", "Butter", null, null, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckItemExistsAsync(userUid, "Weekly", "Milk", TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckItemExistsAsync(userUid, "Weekly", "Butter", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.NewName)
      .WithErrorCode("ShoppingList.Item.AlreadyExists");
  }

  [Fact(DisplayName = "Passes when new name equals original name (case-insensitive)")]
  public async Task Validator_Passes_WhenNewNameEqualsOriginalName() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", "milk", null, null, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckItemExistsAsync(userUid, "Weekly", "Milk", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x.NewName);
  }

  [Fact(DisplayName = "Fails when price is negative")]
  public async Task Validator_Fails_WhenPriceIsNegative() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", null, null, -1f, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckItemExistsAsync(userUid, "Weekly", "Milk", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Price)
      .WithErrorCode("ShoppingList.Items.Price.Negative");
  }

  [Fact(DisplayName = "Fails when quantity exceeds 50 characters")]
  public async Task Validator_Fails_WhenQuantityTooLong() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", null, new string('a', 51), null, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckItemExistsAsync(userUid, "Weekly", "Milk", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Quantity)
      .WithErrorCode("ShoppingList.Items.Quantity.TooLong");
  }

  [Fact(DisplayName = "Passes when all fields are valid")]
  public async Task Validator_Passes_WhenAllFieldsAreValid() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", "Whole Milk", "2 litres", 3.5f, true);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckItemExistsAsync(userUid, "Weekly", "Milk", TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckItemExistsAsync(userUid, "Weekly", "Whole Milk", TestContext.Current.CancellationToken)
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact(DisplayName = "Passes when price is zero")]
  public async Task Validator_Passes_WhenPriceIsZero() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", null, null, 0f, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckItemExistsAsync(userUid, "Weekly", "Milk", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x.Price);
  }

  [Fact(DisplayName = "Passes when quantity is exactly 50 characters")]
  public async Task Validator_Passes_WhenQuantityIsExactly50Chars() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", null, new string('a', 50), null, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckItemExistsAsync(userUid, "Weekly", "Milk", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
  }

  [Fact(DisplayName = "Does not check new name for duplicates when new name is null")]
  public async Task Validator_DoesNotCheckNewNameDuplicate_WhenNewNameIsNull() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", null, null, null, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);
    _shoppingRepository
      .CheckItemExistsAsync(userUid, "Weekly", "Milk", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    await _shoppingRepository.DidNotReceive().CheckItemExistsAsync(
      Arg.Any<Guid>(),
      Arg.Any<string?>(),
      Arg.Any<string>(),
      TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Fails with temporary list message when list name is null")]
  public async Task Validator_Fails_WithTemporaryListMessage_WhenListNameIsNull() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, null, "Milk", null, null, null, null);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ShoppingListName)
      .WithErrorCode("ShoppingList.NotFound")
      .WithErrorMessage($"User {userUid} doesn't have a temporary shopping list.");
  }

  [Fact(DisplayName = "Fails if no fields to update are provided")]
  public async Task Validator_Fails_IfNoFieldsToUpdateProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", null, null, null, null);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x)
      .WithErrorCode("ShoppingList.Item.NoFieldsToUpdate")
      .WithErrorMessage("At least one field must be provided to update the item.");
  }

  [Fact(DisplayName = "Does not fail if new name is provided")]
  public async Task Validator_DoesNotFail_IfNewNameIsProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", "Yogurt", null, null, null);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x);
  }

  [Fact(DisplayName = "Does not fail if quantity is provided")]
  public async Task Validator_DoesNotFail_IfQuantityIsProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", null, "2 litres", null, null);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x);
  }

  [Fact(DisplayName = "Does not fail if price is provided")]
  public async Task Validator_DoesNotFail_IfPriceIsProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", null, null, 3.5f, null);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x);
  }

  [Fact(DisplayName = "Does not fail if checked status is provided")]
  public async Task Validator_DoesNotFail_IfCheckedStatusIsProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk", null, null, null, true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x);
  }
}
