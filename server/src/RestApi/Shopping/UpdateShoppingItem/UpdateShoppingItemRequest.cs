namespace Metaspesa.RestApi.Shopping.UpdateShoppingItem;

/// <summary>Changes to a shopping item; at least one field is required.</summary>
/// <param name="Amount">New positive amount, or null to keep the current amount.</param>
/// <param name="Checked">New checked state, or null to keep the current state.</param>
internal sealed record UpdateShoppingItemRequest(int? Amount, bool? Checked);