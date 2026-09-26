using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using NSubstitute;
using static Metaspesa.Application.Shopping.RemoveItem;

namespace Metaspesa.Application.UnitTests.Shopping;

public class RemoveItemHandlerTest {
  private static readonly Guid FormatId = Guid.CreateVersion7();

  [Fact(DisplayName = "Removes aggregate item and commits")]
  public async Task Handle_RemovesItemAndCommits() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = ShoppingTestData.List(
      ownerId, "Weekly", ShoppingTestData.Item(FormatId));
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListId>(), Arg.Any<CancellationToken>())
      .Returns(list);
    IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new Handler(repository, unitOfWork);

    await handler.Handle(
      new Command(ownerId, ShoppingTestData.ListId, FormatId), TestContext.Current.CancellationToken);

    Assert.Empty(list.Items);
    await repository.Received(1).UpdateAsync(list, TestContext.Current.CancellationToken);
    await unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Rejects missing list without commit")]
  public async Task Handle_ThrowsExactException_WhenListIsMissing() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new Handler(repository, unitOfWork);

    await Assert.ThrowsAsync<ShoppingListNotFoundException>(() => handler.Handle(
      new Command(Guid.CreateVersion7(), ShoppingTestData.ListId, FormatId),
      TestContext.Current.CancellationToken));

    await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Rejects missing item without persistence")]
  public async Task Handle_DoesNotPersist_WhenItemIsMissing() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = ShoppingTestData.List(ownerId, "Weekly");
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListId>(), Arg.Any<CancellationToken>())
      .Returns(list);
    IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new Handler(repository, unitOfWork);

    await Assert.ThrowsAsync<ShoppingItemNotFoundException>(() => handler.Handle(
      new Command(ownerId, ShoppingTestData.ListId, FormatId),
      TestContext.Current.CancellationToken));

    await repository.DidNotReceive().UpdateAsync(
      Arg.Any<ShoppingList>(), Arg.Any<CancellationToken>());
    await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
  }
}