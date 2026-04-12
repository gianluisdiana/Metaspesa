namespace Metaspesa.Domain.Shopping;

public record ShoppingItem(
  string Name,
  string? Quantity,
  Price Price,
  bool IsChecked
) : Product(Name, Quantity, Price);