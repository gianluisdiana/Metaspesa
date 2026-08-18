namespace Metaspesa.Domain.Shopping.Errors;

public class ShoppingListAlreadyExistsException : ShoppingDomainException {
  public ShoppingListAlreadyExistsException()
    : base("ShoppingList.AlreadyExists", "Shopping list already exists.") { }
  public ShoppingListAlreadyExistsException(string message)
    : base("ShoppingList.AlreadyExists", message) { }
  public ShoppingListAlreadyExistsException(string message, Exception innerException)
    : base("ShoppingList.AlreadyExists", message, innerException) { }
}
