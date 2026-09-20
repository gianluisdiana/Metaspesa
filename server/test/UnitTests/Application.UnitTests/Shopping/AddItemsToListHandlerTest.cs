using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using NSubstitute;
using static Metaspesa.Application.Shopping.AddItemsToList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class AddItemsToListHandlerTest {
  private readonly IShoppingListRepository _repository =
    Substitute.For<IShoppingListRepository>();
  private readonly IProductRepository _productRepository =
    Substitute.For<IProductRepository>();
  private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

  [Fact(DisplayName = "Adds validated formats through aggregate")]
  public async Task Handle_AddsItemsAndCommits_WhenFormatsExist() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = ShoppingTestData.List(ownerId);
    _repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListId>(), Arg.Any<CancellationToken>())
      .Returns(list);
    _productRepository.GetProductsAsync(
      Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
      .Returns(new Dictionary<int, MarketProduct> {
        [10] = ShoppingTestData.MarketProduct(10),
      });
    var handler = new Handler(_repository, _productRepository, _unitOfWork);

    await handler.Handle(
      new Command(ownerId, 1, [new CommandItem(10, 2, true)]),
      TestContext.Current.CancellationToken);

    ShoppingItem item = Assert.Single(list.Items);
    Assert.Equal(10, item.ProductFormatId.Value);
    Assert.Equal(2, item.Amount.Value);
    Assert.True(item.IsChecked);
    await _repository.Received(1).UpdateAsync(list, TestContext.Current.CancellationToken);
    await _unitOfWork.Received(1).SaveChangesAsync(
      TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Rejects empty additions without commit")]
  public async Task Handle_ThrowsExactException_WhenItemsAreEmpty() {
    var handler = new Handler(_repository, _productRepository, _unitOfWork);

    await Assert.ThrowsAsync<EmptyShoppingItemsException>(() => handler.Handle(
      new Command(Guid.CreateVersion7(), 1, []),
      TestContext.Current.CancellationToken));

    await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Rejects missing format without mutating list")]
  public async Task Handle_ThrowsExactException_WhenFormatIsMissing() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = ShoppingTestData.List(ownerId);
    _repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListId>(), Arg.Any<CancellationToken>())
      .Returns(list);
    _productRepository.GetProductsAsync(
      Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
      .Returns(new Dictionary<int, MarketProduct>());
    var handler = new Handler(_repository, _productRepository, _unitOfWork);

    await Assert.ThrowsAsync<ShoppingProductFormatNotFoundException>(() => handler.Handle(
      new Command(ownerId, 1, [new CommandItem(10, 2, false)]),
      TestContext.Current.CancellationToken));

    Assert.Empty(list.Items);
    await _repository.DidNotReceive().UpdateAsync(
      Arg.Any<ShoppingList>(), Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Loads owner list and distinct product formats")]
  public async Task Handle_UsesOwnerIdAndDistinctFormatIds() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = ShoppingTestData.List(ownerId, null);
    _repository.GetAsync(
      new UserId(ownerId), new ShoppingListId(1), TestContext.Current.CancellationToken)
      .Returns(list);
    _productRepository.GetProductsAsync(
      Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
      .Returns(new Dictionary<int, MarketProduct> {
        [10] = ShoppingTestData.MarketProduct(10),
      });
    var handler = new Handler(_repository, _productRepository, _unitOfWork);

    await Assert.ThrowsAsync<DuplicateShoppingItemException>(() => handler.Handle(
      new Command(ownerId, 1, [
        new CommandItem(10, 1, false),
        new CommandItem(10, 2, true),
      ]),
      TestContext.Current.CancellationToken));

    await _repository.Received(1).GetAsync(
      new UserId(ownerId), new ShoppingListId(1), TestContext.Current.CancellationToken);
    await _productRepository.Received(1).GetProductsAsync(
      Arg.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Single() == 10),
      TestContext.Current.CancellationToken);
    await _unitOfWork.DidNotReceive().SaveChangesAsync(
      Arg.Any<CancellationToken>());
  }
}