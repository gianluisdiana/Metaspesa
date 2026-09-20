namespace Metaspesa.Domain.Shopping.Errors;

public class TemporaryShoppingListAlreadyExistsException : ShoppingDomainException {
  public TemporaryShoppingListAlreadyExistsException()
    : base("ShoppingList.Temporary.AlreadyExists",
      "A temporary shopping list already exists.") { }
  public TemporaryShoppingListAlreadyExistsException(string message)
    : base("ShoppingList.Temporary.AlreadyExists", message) { }
  public TemporaryShoppingListAlreadyExistsException(
    string message, Exception innerException
  ) : base("ShoppingList.Temporary.AlreadyExists", message, innerException) { }
}