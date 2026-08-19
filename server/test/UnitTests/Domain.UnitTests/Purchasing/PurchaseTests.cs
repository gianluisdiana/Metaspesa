using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Purchasing;
using Metaspesa.Domain.Purchasing.Errors;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.UnitTests.Purchasing;

public class PurchaseTests {
  private static readonly DateTime PurchasedAt =
    new(2026, 8, 19, 12, 0, 0, DateTimeKind.Utc);

  [Fact(DisplayName = "Creates immutable purchase")]
  public void Create_ExposesReceiptState() {
    var buyerId = new UserId(Guid.CreateVersion7());
    var shoppingListId = new ShoppingListId(4);
    var purchase = Purchase.Create(
      buyerId,
      shoppingListId,
      [Item(7, 2)],
      PurchasedAt);

    Assert.Null(purchase.Id);
    Assert.Equal(buyerId, purchase.BuyerId);
    Assert.Equal(shoppingListId, purchase.ShoppingListId);
    Assert.Equal(PurchasedAt, purchase.PurchasedAt);
    Assert.Single(purchase.Items);
    Assert.All(
      typeof(Purchase).GetProperties(),
      property => Assert.False(property.CanWrite));
  }

  [Fact(DisplayName = "Rehydrates purchase with deleted references")]
  public void Rehydrate_AcceptsMissingBuyerAndList() {
    var purchase = Purchase.Rehydrate(
      new PurchaseId(5), null, null, [Item(7, 2)], PurchasedAt);

    Assert.Equal(new PurchaseId(5), purchase.Id);
    Assert.Null(purchase.BuyerId);
    Assert.Null(purchase.ShoppingListId);
  }

  [Fact(DisplayName = "Rejects empty purchase")]
  public void Create_ThrowsExactException_WhenItemsAreEmpty() {
    Assert.Throws<EmptyPurchaseItemsException>(() =>
      Purchase.Create(null, null, [], PurchasedAt));
  }

  [Fact(DisplayName = "Rejects duplicate paid snapshot")]
  public void Create_ThrowsExactException_WhenSnapshotIsDuplicated() {
    Assert.Throws<DuplicatePurchaseItemException>(() =>
      Purchase.Create(null, null, [Item(7, 1), Item(7, 2)], PurchasedAt));
  }

  [Theory(DisplayName = "Rejects invalid purchase time")]
  [ClassData<InvalidPurchaseTimes>]
  public void Create_ThrowsExactException_WhenTimeIsInvalid(DateTime value) {
    Assert.Throws<InvalidPurchasedAtException>(() =>
      Purchase.Create(null, null, [Item(7, 1)], value));
  }

  private static PurchaseItem Item(int snapshotId, int amount) =>
    new(new PriceSnapshotId(snapshotId), new PositiveAmount(amount));

  private sealed class InvalidPurchaseTimes() : TheoryData<DateTime>(
    default,
    new DateTime(2026, 8, 19, 12, 0, 0, DateTimeKind.Local),
    new DateTime(2026, 8, 19, 12, 0, 0, DateTimeKind.Unspecified)
  );
}
