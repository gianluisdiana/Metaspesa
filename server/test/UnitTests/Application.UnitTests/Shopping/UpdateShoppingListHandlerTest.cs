using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using NSubstitute;
using static Metaspesa.Application.Shopping.UpdateShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class UpdateShoppingListHandlerTest {
  [Fact(DisplayName = "Renames aggregate and commits")]
  public async Task Handle_RenamesListAndCommits() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = ShoppingTestData.List(ownerId, null);
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(list);
    IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new Handler(repository, unitOfWork);

    await handler.Handle(
      new Command(ownerId, null, "Weekly"), TestContext.Current.CancellationToken);

    Assert.Equal(new ShoppingListName("Weekly"), list.Name);
    Assert.False(list.IsTemporary);
    await repository.Received(1).UpdateAsync(list, TestContext.Current.CancellationToken);
    await unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Rejects conflicting name without commit")]
  public async Task Handle_ThrowsExactException_WhenNewNameExists() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = ShoppingTestData.List(ownerId, null);
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(list);
    repository.ExistsAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(true);
    IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new Handler(repository, unitOfWork);

    await Assert.ThrowsAsync<ShoppingListAlreadyExistsException>(() => handler.Handle(
      new Command(ownerId, null, "Weekly"), TestContext.Current.CancellationToken));

    Assert.True(list.IsTemporary);
    await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Renaming with same normalized name skips conflict lookup")]
  public async Task Handle_DoesNotCheckConflict_WhenNameIsUnchanged() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = ShoppingTestData.List(ownerId, "Weekly");
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(list);
    IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new Handler(repository, unitOfWork);

    await handler.Handle(
      new Command(ownerId, "Weekly", " Weekly "),
      TestContext.Current.CancellationToken);

    await repository.DidNotReceive().ExistsAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>());
    await unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
  }
}