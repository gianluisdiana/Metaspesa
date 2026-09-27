using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using NSubstitute;
using static Metaspesa.Application.Shopping.AddItemsToList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class AddItemsToListHandlerTest {
  private readonly IShoppingListRepository _shoppingListRepository;
  private readonly IProductRepository _productRepository;

  private readonly Handler _handler;

  public AddItemsToListHandlerTest() {
    _shoppingListRepository = Substitute.For<IShoppingListRepository>();
    _productRepository = Substitute.For<IProductRepository>();

    _handler = new Handler(_shoppingListRepository, _productRepository);
  }

  [Fact(DisplayName = "Rejects shopping list that does not exist")]
  public async Task Handle_RejectsShoppingListThatDoesNotExist() {
    var command = new Command(
      Guid.CreateVersion7(),
      Guid.CreateVersion7(),
      [new AddItemsParams(Guid.CreateVersion7(), 2, true)]);

    _shoppingListRepository.GetAsync(
        command.UserUid,
        command.ShoppingListId,
        TestContext.Current.CancellationToken)
      .Returns((ShoppingList?)null);

    async Task action() => await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<ShoppingListNotFoundException>(action);
  }

  [Fact(DisplayName = "Rejects product format that doesn't exist")]
  public async Task Handle_RejectsProductFormatThatDoesNotExist() {
    var ownerId = Guid.CreateVersion7();
    var formatId = Guid.CreateVersion7();
    var shoppingList = ShoppingList.Create(ownerId, "Weekly");

    var command = new Command(
      ownerId,
      shoppingList.Id.Value,
      [new AddItemsParams(formatId, 2, true)]);

    _shoppingListRepository.GetAsync(
        ownerId,
        command.ShoppingListId,
        TestContext.Current.CancellationToken)
      .Returns(shoppingList);

    _productRepository.CheckFormatsExistAsync(
        Arg.Is<IEnumerable<Guid>>(ids =>
          ids.Count() == 1 && ids.First() == formatId),
        TestContext.Current.CancellationToken)
      .Returns(false);

    async Task action() => await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<ShoppingProductFormatNotFoundException>(action);
  }

  [Fact(DisplayName = "Saves aggregate after adding items")]
  public async Task Handle_SavesAggregateAfterAddingItems() {
    var ownerId = Guid.CreateVersion7();
    var formatId = Guid.CreateVersion7();
    var list = ShoppingList.Create(ownerId, "Weekly Groceries");
    var command = new Command(
      ownerId, list.Id.Value, [new AddItemsParams(formatId, 2, true)]);

    _shoppingListRepository.GetAsync(
        ownerId, list.Id.Value, TestContext.Current.CancellationToken)
      .Returns(list);
    _productRepository.CheckFormatsExistAsync(
        Arg.Is<IEnumerable<Guid>>(ids =>
          ids.Count() == 1 && ids.First() == formatId),
        TestContext.Current.CancellationToken)
      .Returns(true);

    await _handler.Handle(command, TestContext.Current.CancellationToken);

    await _shoppingListRepository.Received(1).SaveAsync(
      Arg.Is<ShoppingList>(l => l.Id == list.Id),
      TestContext.Current.CancellationToken);
  }
}