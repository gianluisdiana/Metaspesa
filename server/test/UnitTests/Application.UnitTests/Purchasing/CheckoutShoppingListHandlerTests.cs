using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Purchasing;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Purchasing;
using Metaspesa.Domain.Purchasing.Errors;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using NSubstitute;
using static Metaspesa.Application.Purchasing.CheckoutShoppingList;

namespace Metaspesa.Application.UnitTests.Purchasing;

public class CheckoutShoppingListHandlerTests {
  private static readonly DateTime PurchasedAt =
    new(2026, 8, 19, 12, 0, 0, DateTimeKind.Utc);

  private readonly IShoppingListRepository _shoppingRepository =
    Substitute.For<IShoppingListRepository>();
  private readonly IPurchasePriceSnapshotReader _snapshotReader =
    Substitute.For<IPurchasePriceSnapshotReader>();
  private readonly IPurchaseRepository _purchaseRepository =
    Substitute.For<IPurchaseRepository>();
  private readonly IClock _clock = Substitute.For<IClock>();

  [Fact(DisplayName = "Creates purchase, resets list, and commits once")]
  public async Task Handle_ChecksOutNamedList_WhenCheckedItemsHaveSnapshots() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = List(
      ownerId,
      "Weekly",
      Item(2, 3, true),
      Item(3, 1, false),
      Item(4, 2, true));
    _shoppingRepository.GetAsync(
      new UserId(ownerId),
      new ShoppingListId(1),
      TestContext.Current.CancellationToken)
      .Returns(list);
    _snapshotReader.GetLatestAsync(
      Arg.Any<IReadOnlyCollection<ProductFormatId>>(),
      TestContext.Current.CancellationToken)
      .Returns(new Dictionary<ProductFormatId, PriceSnapshotId> {
        [new ProductFormatId(2)] = new PriceSnapshotId(20),
        [new ProductFormatId(4)] = new PriceSnapshotId(40),
      });
    _clock.GetCurrentTime().Returns(PurchasedAt);
    _purchaseRepository.AddAsync(Arg.Any<Purchase>(),
      TestContext.Current.CancellationToken).Returns(18);
    Handler handler = CreateHandler();

    int purchaseId = await handler.Handle(
      new Command(ownerId, 1), TestContext.Current.CancellationToken);

    Assert.Equal(18, purchaseId);
    await _purchaseRepository.Received(1).AddAsync(Arg.Is<Purchase>(purchase =>
      purchase.Id == null &&
      purchase.BuyerId == new UserId(ownerId) &&
      purchase.ShoppingListId == new ShoppingListId(1) &&
      purchase.PurchasedAt == PurchasedAt &&
      purchase.Items.Count == 2 &&
      purchase.Items.Any(item =>
        item.PriceSnapshotId == new PriceSnapshotId(20) &&
        item.Amount == new PositiveAmount(3)) &&
      purchase.Items.Any(item =>
        item.PriceSnapshotId == new PriceSnapshotId(40) &&
        item.Amount == new PositiveAmount(2))),
      TestContext.Current.CancellationToken);
    Assert.All(list.Items, item => Assert.False(item.IsChecked));
    await _shoppingRepository.Received(1).UpdateAsync(
      list, TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Loads temporary list by missing name")]
  public async Task Handle_ChecksOutTemporaryList_WhenNameIsMissing() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = List(ownerId, null, Item(2, isChecked: true));
    _shoppingRepository.GetAsync(
      new UserId(ownerId), new ShoppingListId(1), TestContext.Current.CancellationToken)
      .Returns(list);
    _snapshotReader.GetLatestAsync(
      Arg.Any<IReadOnlyCollection<ProductFormatId>>(),
      TestContext.Current.CancellationToken)
      .Returns(new Dictionary<ProductFormatId, PriceSnapshotId> {
        [new ProductFormatId(2)] = new PriceSnapshotId(20),
      });
    _clock.GetCurrentTime().Returns(PurchasedAt);
    Handler handler = CreateHandler();

    await handler.Handle(
      new Command(ownerId, 1), TestContext.Current.CancellationToken);

    await _shoppingRepository.Received(1).GetAsync(
      new UserId(ownerId), new ShoppingListId(1), TestContext.Current.CancellationToken);
    await _purchaseRepository.Received(1).AddAsync(
      Arg.Is<Purchase>(purchase => purchase.ShoppingListId == new ShoppingListId(1)),
      TestContext.Current.CancellationToken);
  }

  [Fact]
  public async Task Handle_ChecksOutOwnedListById() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = List(ownerId, "Weekly", Item(2, isChecked: true));
    _shoppingRepository.GetAsync(new UserId(ownerId), new ShoppingListId(1),
      TestContext.Current.CancellationToken)
      .Returns(list);
    _snapshotReader.GetLatestAsync(
      Arg.Any<IReadOnlyCollection<ProductFormatId>>(),
      TestContext.Current.CancellationToken)
      .Returns(new Dictionary<ProductFormatId, PriceSnapshotId> {
        [new ProductFormatId(2)] = new PriceSnapshotId(20),
      });
    _clock.GetCurrentTime().Returns(PurchasedAt);
    _purchaseRepository.AddAsync(Arg.Any<Purchase>(),
      TestContext.Current.CancellationToken).Returns(18);
    Handler handler = CreateHandler();

    int purchaseId = await handler.Handle(
      new Command(ownerId, 1), TestContext.Current.CancellationToken);

    Assert.Equal(18, purchaseId);
  }

  [Fact(DisplayName = "Rejects missing list without checkout work")]
  public async Task Handle_ThrowsShoppingException_WhenListIsMissing() {
    Handler handler = CreateHandler();

    await Assert.ThrowsAsync<ShoppingListNotFoundException>(() => handler.Handle(
      new Command(Guid.CreateVersion7(), 1),
      TestContext.Current.CancellationToken));

    await _snapshotReader.DidNotReceive().GetLatestAsync(
      Arg.Any<IReadOnlyCollection<ProductFormatId>>(),
      Arg.Any<CancellationToken>());
    await _purchaseRepository.DidNotReceive().AddAsync(
      Arg.Any<Purchase>(), Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Rejects list without checked items")]
  public async Task Handle_ThrowsPurchaseException_WhenNoItemIsChecked() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = List(ownerId, "Weekly", Item(2, isChecked: false));
    _shoppingRepository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListId>(), Arg.Any<CancellationToken>())
      .Returns(list);
    Handler handler = CreateHandler();

    await Assert.ThrowsAsync<EmptyPurchaseItemsException>(() => handler.Handle(
      new Command(ownerId, 1), TestContext.Current.CancellationToken));

    await _snapshotReader.DidNotReceive().GetLatestAsync(
      Arg.Any<IReadOnlyCollection<ProductFormatId>>(),
      Arg.Any<CancellationToken>());
    await _purchaseRepository.DidNotReceive().AddAsync(
      Arg.Any<Purchase>(), Arg.Any<CancellationToken>());
    Assert.False(list.Items.Single().IsChecked);
  }

  [Fact(DisplayName = "Rejects missing snapshot without resetting list")]
  public async Task Handle_ThrowsPurchaseException_WhenSnapshotIsMissing() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = List(
      ownerId, "Weekly", Item(2, isChecked: true), Item(3, isChecked: true));
    _shoppingRepository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListId>(), Arg.Any<CancellationToken>())
      .Returns(list);
    _snapshotReader.GetLatestAsync(
      Arg.Any<IReadOnlyCollection<ProductFormatId>>(),
      Arg.Any<CancellationToken>())
      .Returns(new Dictionary<ProductFormatId, PriceSnapshotId> {
        [new ProductFormatId(2)] = new PriceSnapshotId(20),
      });
    Handler handler = CreateHandler();

    await Assert.ThrowsAsync<PurchasePriceSnapshotNotFoundException>(() =>
      handler.Handle(
        new Command(ownerId, 1),
        TestContext.Current.CancellationToken));

    await _purchaseRepository.DidNotReceive().AddAsync(
      Arg.Any<Purchase>(), Arg.Any<CancellationToken>());
    Assert.All(list.Items, item => Assert.True(item.IsChecked));
    await _shoppingRepository.DidNotReceive().UpdateAsync(
      Arg.Any<ShoppingList>(), Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Propagates snapshot cancellation without mutation")]
  public async Task Handle_PropagatesCancellation_FromSnapshotReader() {
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = List(ownerId, "Weekly", Item(2, isChecked: true));
    _shoppingRepository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListId>(), Arg.Any<CancellationToken>())
      .Returns(list);
    _snapshotReader.GetLatestAsync(
      Arg.Any<IReadOnlyCollection<ProductFormatId>>(),
      Arg.Any<CancellationToken>())
      .Returns<Task<IReadOnlyDictionary<ProductFormatId, PriceSnapshotId>>>(_ =>
        throw new OperationCanceledException());
    Handler handler = CreateHandler();

    await Assert.ThrowsAsync<OperationCanceledException>(() => handler.Handle(
      new Command(ownerId, 1), TestContext.Current.CancellationToken));

    await _purchaseRepository.DidNotReceive().AddAsync(
      Arg.Any<Purchase>(), Arg.Any<CancellationToken>());
    Assert.True(list.Items.Single().IsChecked);
  }

  private Handler CreateHandler() => new(
    _shoppingRepository,
    _snapshotReader,
    _purchaseRepository,
    _clock);

  private static ShoppingList List(
    Guid ownerId,
    string? name,
    params ShoppingItem[] items
  ) => ShoppingList.Rehydrate(
    new ShoppingListId(1),
    [new UserId(ownerId)],
    name is null ? null : new ShoppingListName(name),
    null,
    items);

  private static ShoppingItem Item(
    int formatId, int amount = 1, bool isChecked = false
  ) => new(
    new ProductFormatId(formatId), new PositiveAmount(amount), isChecked);
}