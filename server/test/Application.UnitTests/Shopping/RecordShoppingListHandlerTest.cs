using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.RecordShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class RecordShoppingListHandlerTest {
  private readonly IValidator<Command> _validator;
  private readonly IProductRepository _productRepository;
  private readonly IShoppingRepository _shoppingRepository;
  private readonly IUnitOfWork _unitOfWork;

  private readonly Handler _handler;

  public RecordShoppingListHandlerTest() {
    _validator = Substitute.For<IValidator<Command>>();
    _productRepository = Substitute.For<IProductRepository>();
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _unitOfWork = Substitute.For<IUnitOfWork>();

    _handler = new Handler(
      _validator, _productRepository, _shoppingRepository, _unitOfWork);
  }

  [Fact(DisplayName = "Returns errors when validation fails")]
  public async Task Handler_ReturnsErrors_WhenValidationFails() {
    // Arrange
    Command command = new(Guid.NewGuid(), new ShoppingList("Test List", []));

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    Result result = await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
  }

  [Fact(DisplayName = "Updates already registered items")]
  public async Task Handler_UpdatesRegisteredItems() {
    // Arrange
    var userUid = Guid.NewGuid();
    var commonItem = new ShoppingItem("Item 1", null, 2, true);
    List<ShoppingItem> items = [
      commonItem,
      new ShoppingItem("Item 2", null, 3, true),
    ];
    ShoppingList shoppingList = new("Test List", items);
    Command command = new(userUid, shoppingList);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    List<RegisteredItem> registeredItems = [
      new RegisteredItem(commonItem.Name, null, 1)
    ];
    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(registeredItems);

    // Act
    await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    _productRepository.Received(1).UpdateItems(
      userUid,
      Arg.Is<IReadOnlyCollection<ShoppingItem>>(x => x.Single() == commonItem));
  }

  [Fact(DisplayName = "Doesn't update items when there are no registered items")]
  public async Task Handler_DoesNotUpdateItems_WhenNoRegisteredItems() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<ShoppingItem> items = [
      new ShoppingItem("Item 1", null, 2, true),
      new ShoppingItem("Item 2", null, 3, true)
    ];
    ShoppingList shoppingList = new("Test List", items);
    Command command = new(userUid, shoppingList);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns([]);

    // Act
    await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    _productRepository.DidNotReceive().UpdateItems(
      userUid, Arg.Any<IReadOnlyCollection<ShoppingItem>>());
  }

  [Fact(DisplayName = "Doesn't update items with completely new shopping list")]
  public async Task Handler_DoesNotUpdateItems_WhenCompletelyNewShoppingList() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<ShoppingItem> items = [
      new ShoppingItem("Item 1", null, 2, true),
      new ShoppingItem("Item 2", null, 3, true)
    ];
    ShoppingList shoppingList = new("Test List", items);
    Command command = new(userUid, shoppingList);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    List<RegisteredItem> registeredItems = [
      new RegisteredItem("Item 3", null, 1)
    ];
    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(registeredItems);

    // Act
    await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    _productRepository.DidNotReceive().UpdateItems(
      userUid, Arg.Any<IReadOnlyCollection<ShoppingItem>>());
  }

  [Fact(DisplayName = "Registers new items")]
  public async Task Handler_RegistersNewItems() {
    // Arrange
    var userUid = Guid.NewGuid();

    var newItem = new ShoppingItem("Item 2", null, 3, true);
    List<ShoppingItem> items = [
      new ShoppingItem("Item 1", null, 2, true),
      newItem
    ];
    ShoppingList shoppingList = new("Test List", items);
    Command command = new(userUid, shoppingList);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    List<RegisteredItem> registeredItems = [
      new RegisteredItem("Item 1", null, 1)
    ];
    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(registeredItems);

    // Act
    await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    _productRepository.Received(1).RegisterItems(
      userUid,
      Arg.Is<IReadOnlyCollection<ShoppingItem>>(x => x.Single() == newItem));
  }

  [Fact(DisplayName = "Doesn't register items when there are no new items")]
  public async Task Handler_DoesNotRegisterItems_WhenNoNewItems() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<ShoppingItem> items = [
      new ShoppingItem("Item 1", null, 2, true),
      new ShoppingItem("Item 2", null, 3, true)
    ];
    ShoppingList shoppingList = new("Test List", items);
    Command command = new(userUid, shoppingList);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    List<RegisteredItem> registeredItems = [
      new RegisteredItem("Item 1", null, 1),
      new RegisteredItem("Item 2", null, 1)
    ];
    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(registeredItems);

    // Act
    await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    _productRepository.DidNotReceive().RegisterItems(
      userUid, Arg.Any<IReadOnlyCollection<ShoppingItem>>());
  }

  [Fact(DisplayName = "Records shopping list when it has a name")]
  public async Task Handler_RecordsShoppingList_WhenItHasName() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<ShoppingItem> items = [
      new ShoppingItem("Item 1", null, 2, true),
      new ShoppingItem("Item 2", null, 3, true)
    ];
    ShoppingList shoppingList = new("Test List", items);
    Command command = new(userUid, shoppingList);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns([]);

    // Act
    await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).RecordShoppingList(userUid, shoppingList);
  }

  [Fact(DisplayName = "Doesn't record shopping list when it doesn't have a name")]
  public async Task Handler_DoesNotRecordShoppingList_WhenItDoesNotHaveName() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<ShoppingItem> items = [
      new ShoppingItem("Item 1", null, 2, true),
      new ShoppingItem("Item 2", null, 3, true)
    ];
    ShoppingList shoppingList = new("", items);
    Command command = new(userUid, shoppingList);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns([]);

    // Act
    await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.DidNotReceive().RecordShoppingList(
      userUid, Arg.Any<ShoppingList>());
  }

  [Fact(DisplayName = "Saves changes to unit of work")]
  public async Task Handler_SavesChangesToUnitOfWork() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<ShoppingItem> items = [
      new ShoppingItem("Item 1", null, 2, true),
      new ShoppingItem("Item 2", null, 3, true)
    ];
    ShoppingList shoppingList = new("Test List", items);
    Command command = new(userUid, shoppingList);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns([]);

    // Act
    await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.Received(1)
      .SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Returns success result when handling is successful")]
  public async Task Handler_ReturnsSuccessResult_WhenHandlingIsSuccessful() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<ShoppingItem> items = [
      new ShoppingItem("Item 1", null, 2, true),
      new ShoppingItem("Item 2", null, 3, true)
    ];
    ShoppingList shoppingList = new("Test List", items);
    Command command = new(userUid, shoppingList);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns([]);

    // Act
    Result result = await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
  }
}
