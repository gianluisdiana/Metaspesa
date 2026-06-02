using FluentValidation.TestHelper;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.AddItemsToList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class AddItemsToListValidatorTest {
  private readonly IShoppingRepository _shoppingRepository;
  private readonly IProductRepository _productRepository;
  private readonly Validator _validator;

  public AddItemsToListValidatorTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _productRepository = Substitute.For<IProductRepository>();
    _validator = new Validator(_shoppingRepository, _productRepository);
  }

  [Fact(DisplayName = "Fails when shopping list does not exist")]
  public async Task Validator_Fails_WhenShoppingListDoesNotExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Nonexistent", [new(10, 1, false)]);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Nonexistent", TestContext.Current.CancellationToken)
      .Returns(false);
    _productRepository.CheckProductExistsAsync(10, TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ShoppingListName)
      .WithErrorCode("ShoppingList.NotFound")
      .WithCustomState(ErrorKind.Missing);
  }

  [Fact(DisplayName = "Fails with temporary list message when list name is null")]
  public async Task Validator_Fails_WithTemporaryListMessage_WhenListNameIsNull() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, null, [new(10, 1, false)]);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns(false);
    _productRepository.CheckProductExistsAsync(10, TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ShoppingListName)
      .WithErrorCode("ShoppingList.NotFound")
      .WithErrorMessage($"User {userUid} doesn't have a temporary shopping list.")
      .WithCustomState(ErrorKind.Missing);
  }

  [Fact(DisplayName = "Fails when items collection is empty")]
  public async Task Validator_Fails_WhenItemsCollectionIsEmpty() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", []);
    _shoppingRepository.CheckShoppingListExistAsync(
      userUid, command.ShoppingListName, TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Items)
      .WithErrorCode("ShoppingList.Items.Empty");
  }

  [Fact(DisplayName = "Fails when item amount is zero")]
  public async Task Validator_Fails_WhenItemAmountIsZero() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", [new(10, 0, false)]);
    _shoppingRepository.CheckShoppingListExistAsync(
      userUid, command.ShoppingListName, TestContext.Current.CancellationToken)
      .Returns(true);
    _productRepository.CheckProductExistsAsync(10, TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Items[0].Amount")
      .WithErrorCode("ShoppingList.Item.Amount.Invalid")
      .WithCustomState(ErrorKind.Validation);
  }

  [Fact(DisplayName = "Fails when item amount is negative")]
  public async Task Validator_Fails_WhenItemAmountIsNegative() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", [new(10, -1, false)]);
    _shoppingRepository.CheckShoppingListExistAsync(
      userUid, command.ShoppingListName, TestContext.Current.CancellationToken)
      .Returns(true);
    _productRepository.CheckProductExistsAsync(10, TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Items[0].Amount")
      .WithErrorCode("ShoppingList.Item.Amount.Invalid")
      .WithCustomState(ErrorKind.Validation);
  }

  [Fact(DisplayName = "Fails when duplicate product reference UIDs are provided")]
  public async Task Validator_Fails_WhenDuplicateReferenceUidsAreProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", [
      new(10, 1, false),
      new(10, 2, true),
    ]);
    _shoppingRepository.CheckShoppingListExistAsync(
      userUid, command.ShoppingListName, TestContext.Current.CancellationToken)
      .Returns(true);
    _productRepository.CheckProductExistsAsync(10, TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    Assert.Contains(result.Errors, e =>
      e.ErrorCode == "ShoppingList.Items.DuplicateReferenceUid" &&
      (ErrorKind)e.CustomState == ErrorKind.Validation);
  }

  [Fact(DisplayName = "Fails when product reference UID does not exist")]
  public async Task Validator_Fails_WhenProductReferenceUidDoesNotExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", [new(10, 1, false)]);
    _shoppingRepository.CheckShoppingListExistAsync(
      userUid, command.ShoppingListName, TestContext.Current.CancellationToken)
      .Returns(true);
    _productRepository.CheckProductExistsAsync(10, TestContext.Current.CancellationToken)
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    Assert.Contains(result.Errors, e =>
      e.ErrorCode == "ShoppingList.Item.ReferenceUid.NotFound" &&
      (ErrorKind)e.CustomState == ErrorKind.Missing);
  }

  [Fact(DisplayName = "Passes when all fields are valid")]
  public async Task Validator_Passes_WhenAllFieldsAreValid() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", [new(10, 1, true)]);
    _shoppingRepository.CheckShoppingListExistAsync(
      userUid, command.ShoppingListName, TestContext.Current.CancellationToken)
      .Returns(true);
    _productRepository.CheckProductExistsAsync(10, TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }
}
