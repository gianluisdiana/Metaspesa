namespace Metaspesa.Domain.Shopping;

public record ShoppingItem(
  string Name,
  string? Quantity,
  float? Price,
  bool IsChecked
) : Product(Name, Quantity);