using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.UpdateItem;

namespace Metaspesa.Application.UnitTests.Shopping;

public class UpdateItemHandlerTest {
  private readonly IValidator<Command> _validator;
  private readonly IShoppingRepository _shoppingRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly Handler _handler;

  private static ShoppingItem DefaultCurrentItem => new("Milk", null, Price.Empty, false);

  public UpdateItemHandlerTest() {
    _validator = Substitute.For<IValidator<Command>>();
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _unitOfWork = Substitute.For<IUnitOfWork>();
    _handler = new Handler(_validator, _shoppingRepository, _unitOfWork);
  }

  [Fact(DisplayName = "Returns errors when validation fails")]
  public async Task Handler_ReturnsErrors_WhenValidationFails() {
    // Arrange
    var command = new Command(
      Guid.NewGuid(), "Weekly", "Milk", null, null, null, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
  }

  [Fact(DisplayName = "Does not update item when validation fails")]
  public async Task Handler_DoesNotUpdateItem_WhenValidationFails() {
    // Arrange
    var command = new Command(
      Guid.NewGuid(), "Weekly", "Milk", null, null, null, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.DidNotReceive().UpdateItem(
      Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<ShoppingItem>());
  }

  [Fact(DisplayName = "Updates item's name when new name is provided")]
  public async Task Handler_UpdatesItemName_WhenNewNameIsProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Milk", "Whole Milk", null, null, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid,
        command.ShoppingListName,
        command.OriginalItemName,
        TestContext.Current.CancellationToken)
      .Returns(DefaultCurrentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      Arg.Any<Guid>(),
      Arg.Any<string?>(),
      Arg.Any<string>(),
      Arg.Is<ShoppingItem>(x => x.Name == command.NewName));
  }

  [Fact(DisplayName = "Updates item's quantity when new quantity is provided")]
  public async Task Handler_UpdatesItemQuantity_WhenNewQuantityIsProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Milk", null, "2 litres", null, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid,
        command.ShoppingListName,
        command.OriginalItemName,
        TestContext.Current.CancellationToken)
      .Returns(DefaultCurrentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      Arg.Any<Guid>(),
      Arg.Any<string?>(),
      Arg.Any<string>(),
      Arg.Is<ShoppingItem>(x => x.Quantity == command.Quantity));
  }

  [Theory(DisplayName = "Updates item's price when new price is provided")]
  [InlineData(3.99f)]
  [InlineData(0f)]
  public async Task Handler_UpdatesItemPrice_WhenNewPriceIsProvided(float newPrice) {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Milk", null, null, newPrice, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid,
        command.ShoppingListName,
        command.OriginalItemName,
        TestContext.Current.CancellationToken)
      .Returns(DefaultCurrentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      Arg.Any<Guid>(),
      Arg.Any<string?>(),
      Arg.Any<string>(),
      Arg.Is<ShoppingItem>(x => x.Price == new Price(command.Price!.Value)));
  }

  [Theory(DisplayName = "Updates item's checked status when new checked status is provided")]
  [InlineData(true)]
  [InlineData(false)]
  public async Task Handler_UpdatesItemCheckedStatus_WhenNewCheckedStatusIsProvided(
    bool isChecked
  ) {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Milk", null, null, null, isChecked);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid,
        command.ShoppingListName,
        command.OriginalItemName,
        TestContext.Current.CancellationToken)
      .Returns(DefaultCurrentItem with { IsChecked = !isChecked });

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      Arg.Any<Guid>(),
      Arg.Any<string?>(),
      Arg.Any<string>(),
      Arg.Is<ShoppingItem>(x => x.IsChecked == command.IsChecked));
  }

  [Theory(DisplayName = "Keeps current item name without new name")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData(" ")]
  public async Task Handler_KeepsCurrentName_WithoutNewName(string? newName) {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Milk", newName, null, null, null);
    ShoppingItem currentItem = new("Milk", "1 litre", new Price(2f), true);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid,
        command.ShoppingListName,
        command.OriginalItemName,
        TestContext.Current.CancellationToken)
      .Returns(currentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      Arg.Any<Guid>(),
      Arg.Any<string?>(),
      Arg.Any<string>(),
      Arg.Is<ShoppingItem>(x => x.Name == currentItem.Name));
  }

  [Theory(DisplayName = "Keeps current item quantity without new quantity")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData(" ")]
  public async Task Handler_KeepsCurrentQuantity_WithoutNewQuantity(string? newQuantity) {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Milk", null, newQuantity, null, null);
    ShoppingItem currentItem = new("Milk", "1 litre", new Price(2f), true);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid,
        command.ShoppingListName,
        command.OriginalItemName,
        TestContext.Current.CancellationToken)
      .Returns(currentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      Arg.Any<Guid>(),
      Arg.Any<string?>(),
      Arg.Any<string>(),
      Arg.Is<ShoppingItem>(x => x.Quantity == currentItem.Quantity));
  }

  [Fact(DisplayName = "Keeps current item price without new price")]
  public async Task Handler_KeepsCurrentPrice_WithoutNewPrice() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Milk", null, null, null, null);
    ShoppingItem currentItem = new("Milk", "1 litre", new Price(2f), true);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid,
        command.ShoppingListName,
        command.OriginalItemName,
        TestContext.Current.CancellationToken)
      .Returns(currentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      Arg.Any<Guid>(),
      Arg.Any<string?>(),
      Arg.Any<string>(),
      Arg.Is<ShoppingItem>(x => x.Price == currentItem.Price));
  }

  [Fact(DisplayName = "Keeps current item checked status without new checked status")]
  public async Task Handler_KeepsCurrentCheckedStatus_WithoutNewCheckedStatus() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Milk", null, null, null, null);
    ShoppingItem currentItem = new("Milk", "1 litre", new Price(2f), true);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid,
        command.ShoppingListName,
        command.OriginalItemName,
        TestContext.Current.CancellationToken)
      .Returns(currentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      Arg.Any<Guid>(),
      Arg.Any<string?>(),
      Arg.Any<string>(),
      Arg.Is<ShoppingItem>(x => x.IsChecked == currentItem.IsChecked));
  }

  [Fact(DisplayName = "Saves changes to unit of work")]
  public async Task Handler_SavesChangesToUnitOfWork() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Milk", null, null, null, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid,
        command.ShoppingListName,
        command.OriginalItemName,
        TestContext.Current.CancellationToken)
      .Returns(DefaultCurrentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Returns success result when handling is successful")]
  public async Task Handler_ReturnsSuccessResult_WhenHandlingIsSuccessful() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Milk", null, null, null, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid,
        command.ShoppingListName,
        command.OriginalItemName,
        TestContext.Current.CancellationToken)
      .Returns(DefaultCurrentItem);

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
  }

  [Fact(DisplayName = "Returns Missing error when item not found")]
  public async Task Handler_ReturnsMissingError_WhenItemNotFound() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Nonexistent", null, null, null, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository
      .GetItemAsync(userUid, command.ShoppingListName, "Nonexistent", TestContext.Current.CancellationToken)
      .Returns((ShoppingItem?)null);

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    DomainError error = result.Errors.Single();
    var expectedError = new DomainError(
      "ShoppingList.Item.NotFound",
      $"Item '{command.OriginalItemName}' not found.",
      ErrorKind.Missing);
    Assert.Equal(expectedError, error);
  }

  [Fact(DisplayName = "Does not call UpdateItem when item not found")]
  public async Task Handler_DoesNotUpdateItem_WhenItemNotFound() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Nonexistent", null, null, null, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository
      .GetItemAsync(userUid, command.ShoppingListName, "Nonexistent", TestContext.Current.CancellationToken)
      .Returns((ShoppingItem?)null);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.DidNotReceive().UpdateItem(
      Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<ShoppingItem>());
  }

  [Fact(DisplayName = "Does not save changes when item not found")]
  public async Task Handler_DoesNotSaveChanges_WhenItemNotFound() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(
      userUid, "Weekly", "Nonexistent", null, null, null, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository
      .GetItemAsync(userUid, command.ShoppingListName, "Nonexistent", TestContext.Current.CancellationToken)
      .Returns((ShoppingItem?)null);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
  }
}
