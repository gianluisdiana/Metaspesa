namespace Metaspesa.Domain.Shopping.Errors;

public class InvalidShoppingListIdException : ShoppingDomainException {
  public InvalidShoppingListIdException() { }
  public InvalidShoppingListIdException(int value)
    : base("ShoppingList.Id.Invalid", $"Shopping list ID '{value}' is invalid.") { }
  public InvalidShoppingListIdException(string message) : base(message) { }
  public InvalidShoppingListIdException(string message, Exception innerException)
    : base(message, innerException) { }
}
