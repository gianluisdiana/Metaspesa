namespace Metaspesa.Domain.Shopping.Errors;

public class EmptyShoppingItemsException : ShoppingDomainException {
  public EmptyShoppingItemsException()
    : base("ShoppingList.Items.Empty", "At least one item must be provided.") { }
  public EmptyShoppingItemsException(string message) : base(message) { }
  public EmptyShoppingItemsException(string message, Exception innerException)
    : base(message, innerException) { }
}
