namespace Metaspesa.RestApi.Shopping.GetShoppingList;

/// <summary>Market that sells a shopping item.</summary>
/// <param name="Id">Stable market ID.</param>
/// <param name="Name">Market display name.</param>
internal sealed record ShoppingMarketResponse(int Id, string Name);