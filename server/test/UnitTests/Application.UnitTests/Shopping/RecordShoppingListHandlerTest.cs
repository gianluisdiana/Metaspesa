using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.RecordShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class RecordShoppingListHandlerTest {
  [Fact(DisplayName = "Keeps transitional missing-list result")]
  public async Task Handle_ReturnsMissingResult_WhenListDoesNotExist() {
    IShoppingPurchaseRepository repository = Substitute.For<IShoppingPurchaseRepository>();
    var handler = new Handler(repository, Substitute.For<IUnitOfWork>());

    Result result = await handler.Handle(
      new Command(Guid.CreateVersion7(), "Weekly"),
      TestContext.Current.CancellationToken);

    Assert.False(result.IsSuccess);
    Assert.Equal("ShoppingList.NotFound", result.Errors.Single().Code);
  }

  [Fact(DisplayName = "Keeps transitional checked-item validation")]
  public async Task Handle_ReturnsValidationResult_WhenNoItemIsChecked() {
    var ownerId = Guid.CreateVersion7();
    IShoppingPurchaseRepository repository = Substitute.For<IShoppingPurchaseRepository>();
    repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(ShoppingTestData.List(
        ownerId, "Weekly", ShoppingTestData.Item(2, isChecked: false)));
    var handler = new Handler(repository, Substitute.For<IUnitOfWork>());

    Result result = await handler.Handle(
      new Command(ownerId, "Weekly"), TestContext.Current.CancellationToken);

    Assert.False(result.IsSuccess);
    Assert.Equal("ShoppingList.MissingCheckedItems", result.Errors.Single().Code);
    repository.DidNotReceive().Record(Arg.Any<UserId>(), Arg.Any<ShoppingList>());
  }

  [Fact(DisplayName = "Records, resets, and commits through compatibility boundary")]
  public async Task Handle_PreservesPurchaseWorkflow_WhenCheckedItemsExist() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = ShoppingTestData.List(
      ownerId, "Weekly", ShoppingTestData.Item(2, 3, true));
    IShoppingPurchaseRepository repository = Substitute.For<IShoppingPurchaseRepository>();
    repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(list);
    IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new Handler(repository, unitOfWork);

    Result result = await handler.Handle(
      new Command(ownerId, "Weekly"), TestContext.Current.CancellationToken);

    Assert.True(result.IsSuccess);
    repository.Received(1).Record(new UserId(ownerId), list);
    repository.Received(1).Reset(new UserId(ownerId), new ShoppingListName("Weekly"));
    await unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
  }
}
