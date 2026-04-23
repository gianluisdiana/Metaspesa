using FluentValidation.TestHelper;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.RemoveItem;

namespace Metaspesa.Application.UnitTests.Shopping;

public class RemoveItemValidatorTest {
  private readonly IShoppingRepository _shoppingRepository;
  private readonly Validator _validator;

  public RemoveItemValidatorTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _validator = new Validator(_shoppingRepository);
  }

  [Fact(DisplayName = "Fails when shopping list does not exist")]
  public async Task Validator_Fails_WhenShoppingListDoesNotExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Nonexistent", "Milk");
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

  [Fact(DisplayName = "Fails when item does not exist in the list")]
  public async Task Validator_Fails_WhenItemDoesNotExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Nonexistent");
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
    result.ShouldHaveValidationErrorFor(x => x.ItemName)
      .WithErrorCode("ShoppingList.Item.NotFound")
      .WithErrorMessage($"Item 'Nonexistent' not found in the shopping list.")
      .WithCustomState(ErrorKind.Missing);
  }

  [Fact(DisplayName = "Passes when list and item both exist")]
  public async Task Validator_Passes_WhenListAndItemExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", "Milk");
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
    result.ShouldNotHaveAnyValidationErrors();
  }
}
