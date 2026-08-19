using Metaspesa.Application.Abstractions.Purchasing;
using Metaspesa.Database.Entities;
using Metaspesa.Domain.Purchasing;

namespace Metaspesa.Database.Repositories;

internal class PostgreSqlPurchaseRepository(
  MainContext context
) : IPurchaseRepository {
  public void Add(Purchase purchase) => PostgreSqlExceptionMapper.Map(() => {
    ArgumentNullException.ThrowIfNull(purchase);

    context.Purchases.Add(new PurchaseDbEntity {
      UserUid = purchase.BuyerId?.Value,
      ShoppingListId = purchase.ShoppingListId?.Value,
      PurchasedAt = purchase.PurchasedAt,
      Items = [.. purchase.Items.Select(item => new PurchaseItemDbEntity {
        PriceSnapshotId = item.PriceSnapshotId.Value,
        Amount = item.Amount.Value,
      })],
    });
  }, "Couldn't add purchase.");
}