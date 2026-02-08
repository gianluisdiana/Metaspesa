namespace Metaspesa.Domain.Shopping;

public record RegisteredItem(
  string Name,
  string? Quantity,
  float? LastPrice
) : Product(Name, Quantity);
