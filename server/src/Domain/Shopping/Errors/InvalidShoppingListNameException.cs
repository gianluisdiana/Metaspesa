namespace Metaspesa.Domain.Shopping.Errors;

public class InvalidShoppingListNameException : ShoppingDomainException {
  public InvalidShoppingListNameException()
    : base("ShoppingList.Name.Invalid", "Shopping list name cannot be empty.") { }
  public InvalidShoppingListNameException(string message) : base(message) { }
  public InvalidShoppingListNameException(string message, Exception innerException)
    : base(message, innerException) { }
}
