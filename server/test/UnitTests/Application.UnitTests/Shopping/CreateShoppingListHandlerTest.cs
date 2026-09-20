using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using NSubstitute;
using static Metaspesa.Application.Shopping.CreateShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class CreateShoppingListHandlerTest {
  private readonly IShoppingListRepository _repository =
    Substitute.For<IShoppingListRepository>();

  [Fact]
  public async Task Handle_ReturnsPersistedIdForNamedList() {
    var ownerId = Guid.CreateVersion7();
    _repository.AddAsync(Arg.Any<ShoppingList>(),
      TestContext.Current.CancellationToken).Returns(7);
    var handler = new Handler(_repository);

    int id = await handler.Handle(new Command(ownerId, "  Weekly  "),
      TestContext.Current.CancellationToken);

    Assert.Equal(7, id);
    await _repository.Received(1).AddAsync(Arg.Is<ShoppingList>(list =>
      list.Name == new ShoppingListName("Weekly") &&
      list.OwnerIds.Single() == new UserId(ownerId)),
      TestContext.Current.CancellationToken);
  }

  [Fact]
  public async Task Handle_CreatesTemporaryListWhenNameIsMissing() {
    var ownerId = Guid.CreateVersion7();
    var handler = new Handler(_repository);

    await handler.Handle(new Command(ownerId, null),
      TestContext.Current.CancellationToken);

    await _repository.Received(1).AddAsync(Arg.Is<ShoppingList>(list =>
      list.IsTemporary && list.OwnerIds.Single() == new UserId(ownerId)),
      TestContext.Current.CancellationToken);
  }

  [Fact]
  public async Task Handle_RejectsDuplicateNamedList() {
    _repository.ExistsAsync(Arg.Any<UserId>(),
      Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>()).Returns(true);
    var handler = new Handler(_repository);

    await Assert.ThrowsAsync<ShoppingListAlreadyExistsException>(() =>
      handler.Handle(new Command(Guid.CreateVersion7(), "Weekly"),
        TestContext.Current.CancellationToken));

    await _repository.DidNotReceive().AddAsync(Arg.Any<ShoppingList>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_UsesDistinctCodeForDuplicateTemporaryList() {
    var ownerId = Guid.CreateVersion7();
    _repository.ExistsAsync(new UserId(ownerId), null,
      TestContext.Current.CancellationToken).Returns(true);
    var handler = new Handler(_repository);

    TemporaryShoppingListAlreadyExistsException exception =
      await Assert.ThrowsAsync<TemporaryShoppingListAlreadyExistsException>(() =>
        handler.Handle(new Command(ownerId, null),
          TestContext.Current.CancellationToken));

    Assert.Equal("ShoppingList.Temporary.AlreadyExists", exception.Code);
  }
}