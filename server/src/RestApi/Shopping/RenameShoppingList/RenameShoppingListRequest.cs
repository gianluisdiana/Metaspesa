namespace Metaspesa.RestApi.Shopping.RenameShoppingList;

/// <summary>New name for a shopping list.</summary>
/// <param name="Name">Required nonblank name for the list.</param>
internal sealed record RenameShoppingListRequest(string? Name);