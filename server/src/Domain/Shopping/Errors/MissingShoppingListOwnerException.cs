namespace Metaspesa.Domain.Shopping.Errors;

public class MissingShoppingListOwnerException : ShoppingDomainException {
  public MissingShoppingListOwnerException()
    : base("ShoppingList.Owner.Missing", "Shopping list requires at least one owner.") { }
  public MissingShoppingListOwnerException(string message) : base(message) { }
  public MissingShoppingListOwnerException(string message, Exception innerException)
    : base(message, innerException) { }
}
