namespace Metaspesa.RestApi.Shopping.CheckoutShoppingList;

/// <summary>Identifier of a recorded purchase.</summary>
/// <param name="PurchaseId">Stable purchase ID.</param>
internal sealed record PurchaseResponse(Guid PurchaseId);