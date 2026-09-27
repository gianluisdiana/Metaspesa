using Metaspesa.Domain.Markets;

namespace Metaspesa.Domain.Shopping.Errors;

public class ShoppingProductFormatNotFoundException : ShoppingDomainException {
  public ShoppingProductFormatNotFoundException() : base(
    "ShoppingList.Item.ReferenceUid.NotFound",
    "Product format does not exist."
  ) { }

  public ShoppingProductFormatNotFoundException(ProductFormatId productFormatId)
    : base(
      "ShoppingList.Item.ReferenceUid.NotFound",
      $"Product format '{productFormatId}' does not exist.") { }
  public ShoppingProductFormatNotFoundException(string message) : base(message) { }
  public ShoppingProductFormatNotFoundException(string message, Exception innerException)
    : base(message, innerException) { }
}