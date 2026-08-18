using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Domain.Shopping;

public sealed class ShoppingItem(
  ProductFormatId productFormatId,
  PositiveAmount amount,
  bool isChecked
) {
  public ProductFormatId ProductFormatId { get; } = productFormatId;
  public PositiveAmount Amount { get; private set; } = amount;
  public bool IsChecked { get; private set; } = isChecked;

  internal void Update(PositiveAmount? amount, bool? isChecked) {
    if (amount.HasValue) {
      Amount = amount.Value;
    }
    if (isChecked.HasValue) {
      IsChecked = isChecked.Value;
    }
  }
}
