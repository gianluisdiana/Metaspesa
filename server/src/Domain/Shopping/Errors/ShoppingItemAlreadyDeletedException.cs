namespace Metaspesa.Domain.Shopping.Errors;

internal class ShoppingItemAlreadyDeletedException : ShoppingDomainException {
  public ShoppingItemAlreadyDeletedException() { }

  public ShoppingItemAlreadyDeletedException(ShoppingItemId shoppingItemId)
    : base(
      "ShoppingList.Item.AlreadyDeleted",
      $"Shopping item '{shoppingItemId}' has already been deleted.") { }

  public ShoppingItemAlreadyDeletedException(string message) : base(message) { }
  public ShoppingItemAlreadyDeletedException(
    string message, Exception innerException
  ) : base(message, innerException) { }
}