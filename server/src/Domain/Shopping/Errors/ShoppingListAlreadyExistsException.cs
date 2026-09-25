namespace Metaspesa.Domain.Shopping.Errors;

public class ShoppingListAlreadyExistsException : ShoppingDomainException {
  public ShoppingListAlreadyExistsException(
    Guid ownerId, string? name
  ) : base(
    "ShoppingList.AlreadyExists",
    $"The user {ownerId} already has a shopping list named {name}."
  ) { }
  public ShoppingListAlreadyExistsException() : this(Guid.Empty, string.Empty) { }
  public ShoppingListAlreadyExistsException(string message)
    : base("ShoppingList.AlreadyExists", message) { }
  public ShoppingListAlreadyExistsException(string message, Exception innerException)
    : base("ShoppingList.AlreadyExists", message, innerException) { }
}