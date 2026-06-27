namespace Metaspesa.Domain.Shopping;

public record AShoppingItem(
  int ReferenceUid,
  int Amount,
  bool IsChecked
);