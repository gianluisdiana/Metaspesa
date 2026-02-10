using FluentValidation.TestHelper;
using Metaspesa.Domain.Shopping;
using static Metaspesa.Application.Shopping.RecordShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class RecordShoppingListValidatorTest {
  private readonly Validator _validator;

  public RecordShoppingListValidatorTest() {
    _validator = new Validator();
  }

  [Fact(DisplayName = "Returns error when shopping list is empty")]
  public async Task Validate_ReturnError_WhenShoppingListIsEmpty() {
    // Arrange
    var command = new Command(
      Guid.NewGuid(),
      new ShoppingList("", [])
    );

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ShoppingList.Items)
      .WithErrorMessage("Shopping list must contain at least one item.")
      .WithErrorCode("ShoppingList.Items.Empty");
  }

  [Theory(DisplayName = "Returns error when item name is empty")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData(" ")]
  public async Task Validate_ReturnError_WhenItemNameIsEmpty(string? itemName) {
    // Arrange
    var command = new Command(
      Guid.NewGuid(),
      new ShoppingList("", [new(itemName!, null, 1, false)])
    );

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("ShoppingList.Items[0].Name")
      .WithErrorMessage("Item name must not be empty.")
      .WithErrorCode("ShoppingList.Items.Name.Empty");
  }

  [Fact(DisplayName = "Returns error when quantity is too long")]
  public async Task Validate_ReturnError_WhenQuantityIsTooLong() {
    // Arrange
    var command = new Command(
      Guid.NewGuid(),
      new ShoppingList("", [new("Milk", new string('a', 51), 1, false)])
    );

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("ShoppingList.Items[0].Quantity")
      .WithErrorMessage("Item quantity must not exceed 50 characters.")
      .WithErrorCode("ShoppingList.Items.Quantity.TooLong");
  }

  [Fact(DisplayName = "Returns error when price is negative")]
  public async Task Validate_ReturnError_WhenPriceIsNegative() {
    // Arrange
    var command = new Command(
      Guid.NewGuid(),
      new ShoppingList("", [new("Milk", "1 liter", -1, false)])
    );

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("ShoppingList.Items[0].Price")
      .WithErrorMessage("Item price must be a non-negative number.")
      .WithErrorCode("ShoppingList.Items.Price.Negative");
  }

  [Fact(DisplayName = "Returns no error when shopping list is valid")]
  public async Task Validate_ReturnNoError_WhenShoppingListIsValid() {
    // Arrange
    var command = new Command(
      Guid.NewGuid(),
      new ShoppingList("", [new("Milk", "1 liter", 1, false)])
    );

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }
}