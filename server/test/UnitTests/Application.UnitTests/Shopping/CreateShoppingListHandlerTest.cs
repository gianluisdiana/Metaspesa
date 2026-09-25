using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using NSubstitute;
using static Metaspesa.Application.Shopping.CreateShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class CreateShoppingListHandlerTest {
  private readonly IShoppingListRepository _repository;
  private readonly Handler _handler;

  public CreateShoppingListHandlerTest() {
    _repository = Substitute.For<IShoppingListRepository>();

    _handler = new Handler(_repository);
  }

  [Fact]
  public async Task Handle_RejectsDuplicateNamedList() {
    var command = new Command(Guid.CreateVersion7(), "Weekly");
    _repository.ExistsAsync(command.UserUid,
        command.ShoppingListName, TestContext.Current.CancellationToken)
      .Returns(true);

    async Task action() => await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<ShoppingListAlreadyExistsException>(action);
  }

  [Fact]
  public async Task Handle_AddsShoppingList_IfItDoesNotExist() {
    var command = new Command(Guid.CreateVersion7(), "Weekly");
    _repository.ExistsAsync(Arg.Any<Guid>(),
        Arg.Any<string?>(), TestContext.Current.CancellationToken)
      .Returns(false);

    await _handler.Handle(command, TestContext.Current.CancellationToken);

    await _repository.Received(1).AddAsync(
      Arg.Is<ShoppingList>(list =>
        list.OwnerIds.Single().Value == command.UserUid &&
        list.Name == new ShoppingListName(command.ShoppingListName!)),
      TestContext.Current.CancellationToken);
  }

  [Fact]
  public async Task Handle_ReturnsPersistedIdForNamedList() {
    var command = new Command(Guid.CreateVersion7(), "Weekly");

    const int expectedId = 7;
    _repository
      .AddAsync(Arg.Any<ShoppingList>(), TestContext.Current.CancellationToken)
      .Returns(expectedId);

    int id = await _handler.Handle(command,
      TestContext.Current.CancellationToken);

    Assert.Equal(expectedId, id);
  }
}