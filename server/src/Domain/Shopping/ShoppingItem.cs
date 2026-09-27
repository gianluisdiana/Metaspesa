using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Domain.Shopping;

public sealed class ShoppingItem(
  ProductFormatId productFormatId,
  PositiveAmount amount,
  bool isChecked,
  ShoppingItemId? id = null,
  DateTime? deletedAt = null
) {

  public ShoppingItemId Id { get; } = id ?? new ShoppingItemId(Uid.Create());
  public ProductFormatId ProductFormatId { get; } = productFormatId;
  public PositiveAmount Amount { get; private set; } = amount;
  public bool IsChecked { get; private set; } = isChecked;
  public DateTime? DeletedAt { get; } = deletedAt;

  public static ShoppingItem Create(
    Guid productFormatId, int amount, bool isChecked
  ) {
    return new(
      new ProductFormatId(productFormatId),
      new PositiveAmount(amount),
      isChecked,
      new ShoppingItemId(Uid.Create()),
      null
    );
  }

  public static ShoppingItem Rehydrate(
    Guid id, Guid productFormatId, int amount, bool isChecked, DateTime? deletedAt
  ) {
    return new(
      new ProductFormatId(productFormatId),
      new PositiveAmount(amount),
      isChecked,
      new ShoppingItemId(id),
      deletedAt
    );
  }

  internal void Update(PositiveAmount? amount, bool? isChecked) {
    if (amount.HasValue) {
      Amount = amount.Value;
    }
    if (isChecked.HasValue) {
      IsChecked = isChecked.Value;
    }
  }
}