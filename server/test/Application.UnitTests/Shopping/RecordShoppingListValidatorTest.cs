using FluentValidation.TestHelper;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.RecordShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class RecordShoppingListValidatorTest {
  private readonly IShoppingRepository _shoppingRepository;
  private readonly Validator _validator;

  public RecordShoppingListValidatorTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();

    _validator = new Validator(_shoppingRepository);
  }

  [Fact(DisplayName = "Returns error when shopping list has no checked items")]
  public async Task Validate_ReturnError_WhenShoppingListHasNoCheckedItems() {
    // Arrange
    var command = new Command(
      Guid.NewGuid(),
      "Test List",
      [new CommandItem("Milk", null, 1f, false)]
    );

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ShoppingListItems)
      .WithErrorMessage("Shopping list must contain at least one checked item.")
      .WithErrorCode("ShoppingList.MissingCheckedItems");
  }

  [Fact(DisplayName = "Returns error when shopping list is not found")]
  public async Task Validate_ReturnError_WhenShoppingListIsNotFound() {
    // Arrange
    var command = new Command(
      Guid.NewGuid(),
      "Nonexistent List",
      [new CommandItem("Milk", null, 1f, true)]
    );

    string? listName = command.ShoppingListName;
    _shoppingRepository
      .CheckShoppingListExistAsync(
        command.UserUid, listName, Arg.Any<CancellationToken>())
      .Returns(false);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(nameof(Command.ShoppingListName))
      .WithErrorCode("ShoppingList.NotFound");
  }

  [Theory(DisplayName = "Returns error when item name is empty")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData(" ")]
  public async Task Validate_ReturnError_WhenItemNameIsEmpty(string? itemName) {
    // Arrange
    var command = new Command(
      Guid.NewGuid(),
      "Test List",
      [new CommandItem(itemName!, null, 1f, true)]
    );

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("ShoppingListItems[0].Name")
      .WithErrorMessage("Item name must not be empty.")
      .WithErrorCode("ShoppingList.Items.Name.Empty");
  }

  [Fact(DisplayName = "Returns error when item price is negative")]
  public async Task Validate_ReturnError_WhenItemPriceIsNegative() {
    // Arrange
    var command = new Command(
      Guid.NewGuid(),
      "Test List",
      [new CommandItem("Milk", null, -1f, true)]
    );

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("ShoppingListItems[0].Price")
      .WithErrorMessage("Item price must be greater than or equal to 0.")
      .WithErrorCode("ShoppingList.Items.Price.Negative");
  }

  [Fact(DisplayName = "Returns error when quantity is too long")]
  public async Task Validate_ReturnError_WhenQuantityIsTooLong() {
    // Arrange
    var command = new Command(
      Guid.NewGuid(),
      "Test List",
      [new CommandItem("Milk", new string('a', 51), 1f, true)]
    );

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("ShoppingListItems[0].Quantity")
      .WithErrorMessage("Item quantity must not exceed 50 characters.")
      .WithErrorCode("ShoppingList.Items.Quantity.TooLong");
  }

  [Fact(DisplayName = "Returns no error when shopping list is valid")]
  public async Task Validate_ReturnNoError_WhenShoppingListIsValid() {
    // Arrange
    var command = new Command(
      Guid.NewGuid(),
      "Test List",
      [new CommandItem("Milk", "1 liter", 1f, true)]
    );

    string? listName = command.ShoppingListName;
    _shoppingRepository
      .CheckShoppingListExistAsync(
        command.UserUid, listName, Arg.Any<CancellationToken>())
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }
}
