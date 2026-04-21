using FluentValidation.TestHelper;
using Metaspesa.Application.Abstractions.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.AddItemsToList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class AddItemsToListValidatorTest {
  private readonly IShoppingRepository _shoppingRepository;
  private readonly Validator _validator;

  public AddItemsToListValidatorTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _validator = new Validator(_shoppingRepository);
  }

  [Fact(DisplayName = "Fails when shopping list does not exist")]
  public async Task Validator_Fails_WhenShoppingListDoesNotExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Nonexistent", [new("Milk", null, 1f, false)]);
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

  [Fact(DisplayName = "Fails when items collection is empty")]
  public async Task Validator_Fails_WhenItemsCollectionIsEmpty() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", []);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Items)
      .WithErrorCode("ShoppingList.Items.Empty");
  }

  [Theory(DisplayName = "Fails when item name is empty")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData(" ")]
  public async Task Validator_Fails_WhenItemNameIsEmpty(string? name) {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", [new(name!, null, 1f, false)]);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Items[0].Name")
      .WithErrorCode("ShoppingList.Items.Name.Empty");
  }

  [Fact(DisplayName = "Fails when item price is negative")]
  public async Task Validator_Fails_WhenItemPriceIsNegative() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", [new("Milk", null, -1f, false)]);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Items[0].Price")
      .WithErrorCode("ShoppingList.Items.Price.Negative");
  }

  [Fact(DisplayName = "Fails when item quantity exceeds 50 characters")]
  public async Task Validator_Fails_WhenItemQuantityTooLong() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", [new("Milk", new string('a', 51), 1f, false)]);
    _shoppingRepository
      .CheckShoppingListExistAsync(userUid, "Weekly", TestContext.Current.CancellationToken)
      .Returns(true);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Items[0].Quantity")
      .WithErrorCode("ShoppingList.Items.Quantity.TooLong");
  }

  [Fact(DisplayName = "Passes when all fields are valid")]
  public async Task Validator_Passes_WhenAllFieldsAreValid() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", [new("Milk", "1 litre", 2f, false)]);
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
