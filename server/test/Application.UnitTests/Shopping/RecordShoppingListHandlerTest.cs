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
    Command command = new(Guid.NewGuid(), "Test List", []);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    Result result = await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
  }

  [Fact(DisplayName = "Updates registered items when price has changed")]
  public async Task Handler_UpdatesRegisteredItems_WhenPriceHasChanged() {
    // Arrange
    var userUid = Guid.NewGuid();
    var commonItem = new ShoppingItem("Item 1", null, new Price(2), true);
    List<CommandItem> items = [
      new CommandItem("Item 1", null, 2f, true),
      new CommandItem("Item 2", null, 3f, true),
    ];
    Command command = new(userUid, "Test List", items);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    List<Product> registeredItems = [
      new Product("Item 1", null, new Price(commonItem.Price.Value + 1)),
    ];
    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(registeredItems);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _productRepository.Received(1).UpdateRegisteredItems(
      userUid,
      Arg.Is<IReadOnlyCollection<ShoppingItem>>(x => x.Single() == commonItem));
  }

  [Fact(DisplayName = "Doesn't update registered items when price hasn't changed")]
  public async Task Handler_DoesNotUpdateRegisteredItems_WhenPriceHasNotChanged() {
    // Arrange
    var userUid = Guid.NewGuid();
    var commonItem = new ShoppingItem("Item 1", null, new Price(1), true);
    Command command = new(userUid, "Test List", [new CommandItem("Item 1", null, 1f, true)]);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    List<Product> registeredItems = [commonItem];
    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(registeredItems);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _productRepository.DidNotReceive().UpdateRegisteredItems(
      userUid, Arg.Any<IReadOnlyCollection<ShoppingItem>>());
  }

  [Fact(DisplayName = "Doesn't update items when there are no registered items")]
  public async Task Handler_DoesNotUpdateItems_WhenNoRegisteredItems() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List", [
      new CommandItem("Item 1", null, 2f, true),
      new CommandItem("Item 2", null, 3f, true),
    ]);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns([]);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _productRepository.DidNotReceive().UpdateRegisteredItems(
      userUid, Arg.Any<IReadOnlyCollection<ShoppingItem>>());
  }

  [Fact(DisplayName = "Doesn't update items when no registered items match the shopping list")]
  public async Task Handler_DoesNotUpdateItems_WhenNoRegisteredItemsMatch() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List", [
      new CommandItem("Item 1", null, 2f, true),
      new CommandItem("Item 2", null, 3f, true),
    ]);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    List<Product> registeredItems = [new Product("Item 3", null, new Price(1))];
    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(registeredItems);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _productRepository.DidNotReceive().UpdateRegisteredItems(
      userUid, Arg.Any<IReadOnlyCollection<ShoppingItem>>());
  }

  [Fact(DisplayName = "Registers new items")]
  public async Task Handler_RegistersNewItems() {
    // Arrange
    var userUid = Guid.NewGuid();
    var newItem = new ShoppingItem("Item 2", null, new Price(3), true);
    Command command = new(userUid, "Test List", [
      new CommandItem("Item 1", null, 2f, true),
      new CommandItem("Item 2", null, 3f, true),
    ]);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    List<Product> registeredItems = [new Product("Item 1", null, new Price(1))];
    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(registeredItems);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _productRepository.Received(1).RegisterItems(
      userUid,
      Arg.Is<IReadOnlyCollection<ShoppingItem>>(x => x.Single() == newItem));
  }

  [Fact(DisplayName = "Doesn't register items when there are no new items")]
  public async Task Handler_DoesNotRegisterItems_WhenNoNewItems() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List", [
      new CommandItem("Item 1", null, 2f, true),
      new CommandItem("Item 2", null, 3f, true),
    ]);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    List<Product> registeredItems = [
      new Product("Item 1", null, new Price(1)),
      new Product("Item 2", null, new Price(1)),
    ];
    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(registeredItems);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _productRepository.DidNotReceive().RegisterItems(
      userUid, Arg.Any<IReadOnlyCollection<ShoppingItem>>());
  }

  [Fact(DisplayName = "Always records the shopping list")]
  public async Task Handler_AlwaysRecordsShoppingList() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List", [new CommandItem("Item 1", null, 2f, true)]);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns([]);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).RecordShoppingList(
      userUid,
      Arg.Is<ShoppingList>(sl => sl.Name == "Test List"));
  }

  [Fact(DisplayName = "Does not record shopping list when validation fails")]
  public async Task Handler_DoesNotRecordShoppingList_WhenValidationFails() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List", []);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.DidNotReceive().RecordShoppingList(
      userUid, Arg.Any<ShoppingList>());
  }

  [Fact(DisplayName = "Saves changes to unit of work")]
  public async Task Handler_SavesChangesToUnitOfWork() {
    // Arrange
    var userUid = Guid.NewGuid();
    Command command = new(userUid, "Test List", [new CommandItem("Item 1", null, 2f, true)]);

    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns([]);

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
    Command command = new(userUid, "Test List", [new CommandItem("Item 1", null, 2f, true)]);

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