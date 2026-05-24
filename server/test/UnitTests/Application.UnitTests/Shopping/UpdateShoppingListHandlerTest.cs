using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.UpdateShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class UpdateShoppingListHandlerTest {
  private readonly IValidator<Command> _validator;
  private readonly IShoppingRepository _shoppingRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly Handler _handler;

  public UpdateShoppingListHandlerTest() {
    _validator = Substitute.For<IValidator<Command>>();
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _unitOfWork = Substitute.For<IUnitOfWork>();
    _handler = new Handler(_validator, _shoppingRepository, _unitOfWork);
  }

  [Fact(DisplayName = "Returns errors when validation fails")]
  public async Task Handler_ReturnsErrors_WhenValidationFails() {
    // Arrange
    var command = new Command(Guid.NewGuid(), null, "Groceries");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
  }

  [Fact(DisplayName = "Updates shopping list name via repository")]
  public async Task Handler_UpdatesShoppingListName_ViaRepository() {
    // Arrange
    var userUid = Guid.NewGuid();
    var command = new Command(userUid, null, "Groceries");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.Received(1).UpdateShoppingListName(userUid, null, "Groceries");
  }

  [Fact(DisplayName = "Saves changes to unit of work")]
  public async Task Handler_SavesChangesToUnitOfWork() {
    // Arrange
    var command = new Command(Guid.NewGuid(), null, "Groceries");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Does not update shopping list when validation fails")]
  public async Task Handler_DoesNotUpdateShoppingList_WhenValidationFails() {
    // Arrange
    var command = new Command(Guid.NewGuid(), null, "Groceries");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _shoppingRepository.DidNotReceive().UpdateShoppingListName(
      Arg.Any<Guid>(),
      Arg.Any<string?>(),
      Arg.Any<string?>());
  }

  [Fact(DisplayName = "Does not save changes when validation fails")]
  public async Task Handler_DoesNotSaveChanges_WhenValidationFails() {
    // Arrange
    var command = new Command(Guid.NewGuid(), null, "Groceries");
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
  }
}