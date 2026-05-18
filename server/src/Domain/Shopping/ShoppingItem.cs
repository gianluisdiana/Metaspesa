namespace Metaspesa.Domain.Shopping;

public record ShoppingItem(
  string Name,
  Quantity? Quantity,
  Price Price,
  bool IsChecked
) : Product(Name, Quantity, Price);
