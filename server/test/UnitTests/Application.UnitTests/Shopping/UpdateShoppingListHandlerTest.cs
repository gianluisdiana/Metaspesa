using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using NSubstitute;
using static Metaspesa.Application.Shopping.UpdateShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class UpdateShoppingListHandlerTest {
  private readonly IShoppingListRepository _repository;

  private readonly Handler _handler;

  public UpdateShoppingListHandlerTest() {
    _repository = Substitute.For<IShoppingListRepository>();
    _handler = new Handler(_repository);
  }

  [Fact(DisplayName = "Rejects conflicting name without commit")]
  public async Task Handle_ThrowsExactException_WhenNewNameExists() {
    var ownerId = Guid.CreateVersion7();
    const string newName = "New Name";
    _repository.ExistsAsync(ownerId, newName, TestContext.Current.CancellationToken)
      .Returns(true);
    var command = new Command(ownerId, Guid.CreateVersion7(), newName);

    Task action() => _handler.Handle(
      command, TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<ShoppingListAlreadyExistsException>(action);
  }

  [Fact(DisplayName = "Renames aggregate and commits")]
  public async Task Handle_RenamesListAndCommits() {
    var ownerId = Guid.CreateVersion7();
    const string newName = "New Name";
    _repository.ExistsAsync(ownerId, newName, TestContext.Current.CancellationToken)
      .Returns(false);

    var list = ShoppingList.Create(ownerId, "Old Name");
    _repository.GetAsync(ownerId, list.Id.Value, TestContext.Current.CancellationToken)
      .Returns(list);
    var command = new Command(ownerId, list.Id.Value, newName);

    await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    await _repository.Received(1).SaveAsync(
      Arg.Is<ShoppingList>(list => list.Id == list.Id),
      TestContext.Current.CancellationToken);
  }
}