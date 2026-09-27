namespace Metaspesa.Domain.Shopping.Errors;

public class InvalidShoppingItemIdException : ShoppingDomainException {
  public InvalidShoppingItemIdException() { }
  public InvalidShoppingItemIdException(Guid value)
    : base("ShoppingList.Item.Id.Invalid", $"Shopping item ID '{value}' is invalid.") { }
  public InvalidShoppingItemIdException(string message) : base(message) { }
  public InvalidShoppingItemIdException(string message, Exception innerException)
    : base(message, innerException) { }
}