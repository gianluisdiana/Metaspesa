using Metaspesa.Application.Abstractions.Core;
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
  private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

  [Fact(DisplayName = "Creates aggregate and commits")]
  public async Task Handle_AddsAggregateAndCommits_WhenRequestIsValid() {
    var ownerId = Guid.CreateVersion7();
    var handler = new Handler(_repository, _unitOfWork);

    await handler.Handle(
      new Command(ownerId, "  Weekly  "), TestContext.Current.CancellationToken);

    _repository.Received(1).Add(Arg.Is<ShoppingList>(list =>
      list.Id == null &&
      list.Name == new ShoppingListName("Weekly") &&
      list.OwnerIds.Single() == new UserId(ownerId)));
    await _unitOfWork.Received(1).SaveChangesAsync(
      TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Throws conflict and does not commit")]
  public async Task Handle_ThrowsExactException_WhenListAlreadyExists() {
    _repository.ExistsAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(true);
    var handler = new Handler(_repository, _unitOfWork);

    await Assert.ThrowsAsync<ShoppingListAlreadyExistsException>(() => handler.Handle(
      new Command(Guid.CreateVersion7(), "Weekly"),
      TestContext.Current.CancellationToken));

    _repository.DidNotReceive().Add(Arg.Any<ShoppingList>());
    await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
  }
}
