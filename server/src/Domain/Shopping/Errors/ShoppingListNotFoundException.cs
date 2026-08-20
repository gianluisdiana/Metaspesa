namespace Metaspesa.Domain.Shopping.Errors;

public class ShoppingListNotFoundException : ShoppingDomainException {
  public ShoppingListNotFoundException()
    : base("ShoppingList.NotFound", "Shopping list was not found.") { }
  public ShoppingListNotFoundException(string message)
    : base("ShoppingList.NotFound", message) { }
  public ShoppingListNotFoundException(string message, Exception innerException)
    : base("ShoppingList.NotFound", message, innerException) { }
}