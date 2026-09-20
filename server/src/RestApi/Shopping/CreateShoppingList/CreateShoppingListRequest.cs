namespace Metaspesa.RestApi.Shopping.CreateShoppingList;

/// <summary>Details for a new named or temporary shopping list.</summary>
/// <param name="Name">List name, or null to create a temporary list.</param>
internal sealed record CreateShoppingListRequest(string? Name);