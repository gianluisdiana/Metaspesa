using Metaspesa.Domain.Identity;

namespace Metaspesa.Domain.Shopping.Errors;

public class DuplicateShoppingListOwnerException : ShoppingDomainException {
  public DuplicateShoppingListOwnerException() { }
  public DuplicateShoppingListOwnerException(UserId ownerId)
    : base("ShoppingList.Owner.Duplicate", $"Owner '{ownerId}' is duplicated.") { }
  public DuplicateShoppingListOwnerException(string message) : base(message) { }
  public DuplicateShoppingListOwnerException(string message, Exception innerException)
    : base(message, innerException) { }
}
