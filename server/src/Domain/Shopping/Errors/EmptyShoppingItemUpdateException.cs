namespace Metaspesa.Domain.Shopping.Errors;

public class EmptyShoppingItemUpdateException : ShoppingDomainException {
  public EmptyShoppingItemUpdateException()
    : base(
      "ShoppingList.Item.NoFieldsToUpdate",
      "At least one shopping item field must be provided.") { }
  public EmptyShoppingItemUpdateException(string message) : base(message) { }
  public EmptyShoppingItemUpdateException(string message, Exception innerException)
    : base(message, innerException) { }
}
