namespace Metaspesa.Domain.Shopping.Errors;

public class ShoppingItemNotFoundException : ShoppingDomainException {
  public ShoppingItemNotFoundException() { }
  public ShoppingItemNotFoundException(Guid shoppingItemId)
    : base(
      "ShoppingList.Item.NotFound",
      $"Shopping item '{shoppingItemId}' is not in the shopping list.") { }
  public ShoppingItemNotFoundException(string message) : base(message) { }
  public ShoppingItemNotFoundException(string message, Exception innerException)
    : base(message, innerException) { }
}