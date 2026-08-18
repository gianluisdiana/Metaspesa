using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using NSubstitute;
using static Metaspesa.Application.Shopping.UpdateItem;

namespace Metaspesa.Application.UnitTests.Shopping;

public class UpdateItemHandlerTest {
  [Fact(DisplayName = "Updates aggregate and commits")]
  public async Task Handle_UpdatesItemAndCommits() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = ShoppingTestData.List(
      ownerId, "Weekly", ShoppingTestData.Item(3));
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(list);
    IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new Handler(repository, unitOfWork);

    await handler.Handle(
      new Command(ownerId, "Weekly", 3, 4, true),
      TestContext.Current.CancellationToken);

    Assert.Equal(4, list.Items.Single().Amount.Value);
    Assert.True(list.Items.Single().IsChecked);
    await repository.Received(1).UpdateAsync(list, TestContext.Current.CancellationToken);
    await unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Rejects empty update without commit")]
  public async Task Handle_ThrowsExactException_WhenNoFieldsProvided() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = ShoppingTestData.List(
      ownerId, "Weekly", ShoppingTestData.Item(3));
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(list);
    IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new Handler(repository, unitOfWork);

    await Assert.ThrowsAsync<EmptyShoppingItemUpdateException>(() => handler.Handle(
      new Command(ownerId, "Weekly", 3, null, null),
      TestContext.Current.CancellationToken));

    await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
  }
}
