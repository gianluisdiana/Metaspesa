using System.Text.Json.Serialization;

namespace Metaspesa.RestApi.Shopping.GetShoppingList;

/// <summary>Shopping item with current product and price information.</summary>
/// <param name="ProductFormatId">Stable format ID used to update or remove this item.</param>
/// <param name="ProductName">Product display name.</param>
/// <param name="Brand">Product brand display name.</param>
/// <param name="Market">Market that sells the product.</param>
/// <param name="Quantity">Quantity sold in one product format.</param>
/// <param name="UnitPrice">Latest observed price for one product format.</param>
/// <param name="Amount">Positive number of units on the list.</param>
/// <param name="Checked">Whether this item is marked for checkout.</param>
/// <param name="ImageUrl">Absolute product image URL when available; otherwise omitted.</param>
internal sealed record ShoppingItemResponse(
  int ProductFormatId, string ProductName, string Brand,
  ShoppingMarketResponse Market, ShoppingQuantityResponse Quantity,
  ShoppingMoneyResponse UnitPrice, int Amount, bool Checked,
  [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  string? ImageUrl);