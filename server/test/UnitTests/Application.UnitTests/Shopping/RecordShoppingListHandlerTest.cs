using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.RecordShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class RecordShoppingListHandlerTest {
  private readonly IShoppingRepository _shoppingRepository;
  private readonly IUnitOfWork _unitOfWork;

  private readonly Handler _handler;

  public RecordShoppingListHandlerTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _unitOfWork = Substitute.For<IUnitOfWork>();

    _handler = new Handler(_shoppingRepository, _unitOfWork);
  }

  [Fact(DisplayName = "Returns error when shopping list does not exist")]
  public async Task Handler_ReturnsError_WhenShoppingListDoesNotExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List");
    _shoppingRepository
      .GetShoppingListAsync(userUid, "Test List", TestContext.Current.CancellationToken)
      .Returns((AShoppingList?)null);

    var expectedError = new DomainError(
      Code: "ShoppingList.NotFound",
      Description: $"User {userUid} doesn't have a shopping list named '{command.ShoppingListName}'.",
      Kind: ErrorKind.Missing
    );

    // Act
    Result result = await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(expectedError, result.Errors.Single());
  }

  [Fact(DisplayName = "Returns error when temporary shopping list does not exist")]
  public async Task Handler_ReturnsError_WhenTemporaryShoppingListDoesNotExist() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, null);
    _shoppingRepository
      .GetShoppingListAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns((AShoppingList?)null);

    var expectedError = new DomainError(
      Code: "ShoppingList.NotFound",
      Description: $"User {userUid} doesn't have a temporary shopping list.",
      Kind: ErrorKind.Missing
    );

    // Act
    Result result = await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(expectedError, result.Errors.Single());
  }

  [Fact(DisplayName = "Returns error when shopping list has no checked items")]
  public async Task Handler_ReturnsError_WhenShoppingListHasNoCheckedItems() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List");
    _shoppingRepository
      .GetShoppingListAsync(userUid, "Test List", TestContext.Current.CancellationToken)
      .Returns(new AShoppingList("Test List", [new AShoppingItem(1, 2, false)]));

    var expectedError = new DomainError(
      Code: "ShoppingList.MissingCheckedItems",
      Description: "Shopping list must contain at least one checked item.",
      Kind: ErrorKind.Validation
    );

    // Act
    Result result = await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(expectedError, result.Errors.Single());
  }

  [Fact(DisplayName = "Records checked items from current shopping list")]
  public async Task Handler_RecordsCheckedItems_FromCurrentShoppingList() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List");
    _shoppingRepository
      .GetShoppingListAsync(userUid, "Test List", TestContext.Current.CancellationToken)
      .Returns(new AShoppingList("Test List", [
        new AShoppingItem(1, 2, true),
        new AShoppingItem(2, 1, false),
      ]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).RecordShoppingList(
      userUid,
      Arg.Is<AShoppingList>(sl =>
        sl.Name == "Test List" &&
        sl.Items.Count == 1 &&
        sl.Items.Single().ReferenceUid == 1));
  }

  [Fact(DisplayName = "Does not record shopping list when it has no checked items")]
  public async Task Handler_DoesNotRecordShoppingList_WhenListHasNoCheckedItems() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List");
    _shoppingRepository
      .GetShoppingListAsync(userUid, "Test List", TestContext.Current.CancellationToken)
      .Returns(new AShoppingList("Test List", [new AShoppingItem(1, 2, false)]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.DidNotReceive().RecordShoppingList(
      userUid, Arg.Any<AShoppingList>());
  }

  [Fact(DisplayName = "Saves changes to unit of work")]
  public async Task Handler_SavesChangesToUnitOfWork() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List");
    _shoppingRepository
      .GetShoppingListAsync(userUid, "Test List", TestContext.Current.CancellationToken)
      .Returns(new AShoppingList("Test List", [new AShoppingItem(1, 2, true)]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.Received(1)
      .SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Returns success result when handling is successful")]
  public async Task Handler_ReturnsSuccessResult_WhenHandlingIsSuccessful() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List");
    _shoppingRepository
      .GetShoppingListAsync(userUid, "Test List", TestContext.Current.CancellationToken)
      .Returns(new AShoppingList("Test List", [new AShoppingItem(1, 2, true)]));

    // Act
    Result result = await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
  }

  [Fact(DisplayName = "Resets shopping list after recording")]
  public async Task Handler_ResetsShoppingList_AfterRecording() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List");
    _shoppingRepository
      .GetShoppingListAsync(userUid, "Test List", TestContext.Current.CancellationToken)
      .Returns(new AShoppingList("Test List", [new AShoppingItem(1, 2, true)]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).ResetShoppingList(userUid, "Test List");
  }
}
