using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.CreateShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class CreateShoppingListHandlerTest {
  private readonly IValidator<Command> _validator;
  private readonly IShoppingRepository _shoppingRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly Handler _handler;

  public CreateShoppingListHandlerTest() {
    _validator = Substitute.For<IValidator<Command>>();
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _unitOfWork = Substitute.For<IUnitOfWork>();
    _handler = new Handler(_validator, _shoppingRepository, _unitOfWork);
  }

  [Fact(DisplayName = "Returns errors when validation fails")]
  public async Task Handler_ReturnsErrors_WhenValidationFails() {
    // Arrange
    var command = new Command(Guid.NewGuid(), ShoppingListName: "My List");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
  }

  [Fact(DisplayName = "Does not create shopping list when validation fails")]
  public async Task Handler_DoesNotCreateShoppingList_WhenValidationFails() {
    // Arrange
    var command = new Command(Guid.NewGuid(), ShoppingListName: "My List");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.DidNotReceive().CreateShoppingList(
      Arg.Any<Guid>(), Arg.Any<string?>());
  }

  [Fact(DisplayName = "Creates named shopping list via repository")]
  public async Task Handler_CreatesNamedShoppingList_ViaRepository() {
    // Arrange
    var userUid = Guid.NewGuid();
    const string Name = "Weekly Groceries";
    var command = new Command(userUid, ShoppingListName: Name);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).CreateShoppingList(userUid, Name);
  }

  [Fact(DisplayName = "Creates temporary shopping list via repository when name is null")]
  public async Task Handler_CreatesTemporaryShoppingList_WhenNameIsNull() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, ShoppingListName: null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).CreateShoppingList(userUid, null);
  }

  [Fact(DisplayName = "Saves changes to unit of work")]
  public async Task Handler_SavesChangesToUnitOfWork() {
    // Arrange
    var command = new Command(Guid.NewGuid(), ShoppingListName: "My List");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Returns success result when handling is successful")]
  public async Task Handler_ReturnsSuccessResult_WhenHandlingIsSuccessful() {
    // Arrange
    var command = new Command(Guid.NewGuid(), ShoppingListName: "My List");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
  }
}
