using Metaspesa.Domain.Markets;

namespace Metaspesa.Domain.Shopping.Errors;

public class ShoppingItemNotFoundException : ShoppingDomainException {
  public ShoppingItemNotFoundException() { }
  public ShoppingItemNotFoundException(ProductFormatId productFormatId)
    : base(
      "ShoppingList.Item.NotFound",
      $"Product format '{productFormatId}' is not in the shopping list.") { }
  public ShoppingItemNotFoundException(string message) : base(message) { }
  public ShoppingItemNotFoundException(string message, Exception innerException)
    : base(message, innerException) { }
}