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

  private static AShoppingItem CurrentItem => new(10, 2, false);

  public UpdateItemHandlerTest() {
    _validator = Substitute.For<IValidator<Command>>();
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _unitOfWork = Substitute.For<IUnitOfWork>();
    _handler = new Handler(_validator, _shoppingRepository, _unitOfWork);
  }

  [Fact(DisplayName = "Returns errors when validation fails")]
  public async Task Handler_ReturnsErrors_WhenValidationFails() {
    // Arrange
    var command = new Command(Guid.NewGuid(), "Weekly", 10, 3, null);
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
    var command = new Command(Guid.NewGuid(), "Weekly", 10, 3, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.DidNotReceive().UpdateItem(
      Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<AShoppingItem>());
  }

  [Fact(DisplayName = "Updates item amount when amount is provided")]
  public async Task Handler_UpdatesItemAmount_WhenAmountIsProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, 5, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid, command.ShoppingListName, command.ProductReferenceUid,
        TestContext.Current.CancellationToken)
      .Returns(CurrentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      userUid,
      "Weekly",
      Arg.Is<AShoppingItem>(x => x.Amount == 5));
  }

  [Theory(DisplayName = "Updates item checked state when checked state is provided")]
  [InlineData(true)]
  [InlineData(false)]
  public async Task Handler_UpdatesItemCheckedState_WhenCheckedStateIsProvided(bool isChecked) {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, null, isChecked);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid, command.ShoppingListName, command.ProductReferenceUid,
        TestContext.Current.CancellationToken)
      .Returns(CurrentItem with { IsChecked = !isChecked });

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      userUid,
      "Weekly",
      Arg.Is<AShoppingItem>(x => x.IsChecked == isChecked));
  }

  [Fact(DisplayName = "Keeps current amount when amount is not provided")]
  public async Task Handler_KeepsCurrentAmount_WhenAmountIsNotProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, null, true);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid, command.ShoppingListName, command.ProductReferenceUid,
        TestContext.Current.CancellationToken)
      .Returns(CurrentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      userUid,
      "Weekly",
      Arg.Is<AShoppingItem>(x => x.Amount == CurrentItem.Amount));
  }

  [Fact(DisplayName = "Keeps current checked state when checked state is not provided")]
  public async Task Handler_KeepsCurrentCheckedState_WhenCheckedStateIsNotProvided() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, 5, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid, command.ShoppingListName, command.ProductReferenceUid,
        TestContext.Current.CancellationToken)
      .Returns(CurrentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      userUid,
      "Weekly",
      Arg.Is<AShoppingItem>(x => x.IsChecked == CurrentItem.IsChecked));
  }

  [Fact(DisplayName = "Keeps current reference UID")]
  public async Task Handler_KeepsCurrentReferenceUid() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, 5, true);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid, command.ShoppingListName, command.ProductReferenceUid,
        TestContext.Current.CancellationToken)
      .Returns(CurrentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateItem(
      userUid,
      "Weekly",
      Arg.Is<AShoppingItem>(x => x.ReferenceUid == CurrentItem.ReferenceUid));
  }

  [Fact(DisplayName = "Saves changes to unit of work")]
  public async Task Handler_SavesChangesToUnitOfWork() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, 5, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid, command.ShoppingListName, command.ProductReferenceUid,
        TestContext.Current.CancellationToken)
      .Returns(CurrentItem);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Returns success result when handling is successful")]
  public async Task Handler_ReturnsSuccessResult_WhenHandlingIsSuccessful() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, 5, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid, command.ShoppingListName, command.ProductReferenceUid,
        TestContext.Current.CancellationToken)
      .Returns(CurrentItem);

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
  }

  [Fact(DisplayName = "Returns Missing error when item is not found")]
  public async Task Handler_ReturnsMissingError_WhenItemIsNotFound() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, 5, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid, command.ShoppingListName, command.ProductReferenceUid,
        TestContext.Current.CancellationToken)
      .Returns((AShoppingItem?)null);

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(ErrorKind.Missing, result.Errors.Single().Kind);
  }

  [Fact(DisplayName = "Does not update item when item is not found")]
  public async Task Handler_DoesNotUpdateItem_WhenItemIsNotFound() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, 5, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid, command.ShoppingListName, command.ProductReferenceUid,
        TestContext.Current.CancellationToken)
      .Returns((AShoppingItem?)null);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.DidNotReceive().UpdateItem(
      Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<AShoppingItem>());
  }

  [Fact(DisplayName = "Does not save changes when item is not found")]
  public async Task Handler_DoesNotSaveChanges_WhenItemIsNotFound() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, "Weekly", 10, 5, null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _shoppingRepository.GetItemAsync(
        userUid, command.ShoppingListName, command.ProductReferenceUid,
        TestContext.Current.CancellationToken)
      .Returns((AShoppingItem?)null);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
  }
}
