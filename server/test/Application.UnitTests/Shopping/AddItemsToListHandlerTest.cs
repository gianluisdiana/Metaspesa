using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.AddItemsToList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class AddItemsToListHandlerTest {
  private readonly IValidator<Command> _validator;
  private readonly IShoppingRepository _shoppingRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly Handler _handler;

  public AddItemsToListHandlerTest() {
    _validator = Substitute.For<IValidator<Command>>();
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _unitOfWork = Substitute.For<IUnitOfWork>();
    _handler = new Handler(_validator, _shoppingRepository, _unitOfWork);
  }

  [Fact(DisplayName = "Returns errors when validation fails")]
  public async Task Handler_ReturnsErrors_WhenValidationFails() {
    // Arrange
    var command = new Command(Guid.NewGuid(), "My List", []);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
  }

  [Fact(DisplayName = "Does not add items when validation fails")]
  public async Task Handler_DoesNotAddItems_WhenValidationFails() {
    // Arrange
    var command = new Command(Guid.NewGuid(), "My List", []);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.DidNotReceive().AddItemsToList(
      Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<IReadOnlyCollection<ShoppingItem>>());
  }

  [Fact(DisplayName = "Adds items to list via repository")]
  public async Task Handler_AddsItemsToList_ViaRepository() {
    // Arrange
    var userUid = Guid.NewGuid();
    const string ListName = "Weekly";
    List<CommandItem> items = [
      new("Milk", "1 litre", 2f, false),
      new("Bread", null, 1.5f, false),
    ];
    var command = new Command(userUid, ListName, items);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).AddItemsToList(
      userUid,
      ListName,
      Arg.Is<IReadOnlyCollection<ShoppingItem>>(x => x.Count == items.Count));
  }

  [Fact(DisplayName = "Maps item names to shopping items")]
  public async Task Handler_MapsItemNames_ToShoppingItems() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<CommandItem> items = [new("Milk", null, 2f, false)];
    var command = new Command(userUid, "Weekly", items);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).AddItemsToList(
      userUid,
      "Weekly",
      Arg.Is<IReadOnlyCollection<ShoppingItem>>(x => x.First().Name == "Milk"));
  }

  [Fact(DisplayName = "Maps item prices to shopping items")]
  public async Task Handler_MapsItemPrices_ToShoppingItems() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<CommandItem> items = [new("Milk", null, 3.99f, false)];
    var command = new Command(userUid, "Weekly", items);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).AddItemsToList(
      userUid,
      "Weekly",
      Arg.Is<IReadOnlyCollection<ShoppingItem>>(x => x.First().Price == new Price(3.99f)));
  }

  [Fact(DisplayName = "Maps item IsChecked to shopping items")]
  public async Task Handler_MapsItemIsChecked_ToShoppingItems() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<CommandItem> items = [new("Milk", null, 1f, true)];
    var command = new Command(userUid, "Weekly", items);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).AddItemsToList(
      userUid,
      "Weekly",
      Arg.Is<IReadOnlyCollection<ShoppingItem>>(x => x.First().IsChecked));
  }

  [Fact(DisplayName = "Saves changes to unit of work")]
  public async Task Handler_SavesChangesToUnitOfWork() {
    // Arrange
    var command = new Command(Guid.NewGuid(), "Weekly", [new("Milk", null, 2f, false)]);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Does not save changes when validation fails")]
  public async Task Handler_DoesNotSaveChanges_WhenValidationFails() {
    // Arrange
    var command = new Command(Guid.NewGuid(), "Weekly", []);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Returns success result when handling is successful")]
  public async Task Handler_ReturnsSuccessResult_WhenHandlingIsSuccessful() {
    // Arrange
    var command = new Command(Guid.NewGuid(), "Weekly", [new("Milk", null, 2f, false)]);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
  }
}
