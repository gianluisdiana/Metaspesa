using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.GrpcApi.Extensions;

internal static class ProtosExtensions {
  public static Protos.Shopping.ShoppingList ToProto(
    this ShoppingList shoppingList
  ) {
    var protoShoppingList = new Protos.Shopping.ShoppingList();
    if (!shoppingList.IsTemporary()) {
      protoShoppingList.Name = shoppingList.Name;
    }
    protoShoppingList.Items.AddRange(
      shoppingList.Select(item => item.ToProto()));
    return protoShoppingList;
  }

  private static Protos.Shopping.ShoppingItem ToProto(this ShoppingItem item) {
    Protos.Shopping.ShoppingItem product = ((Product)item).ToProto();
    product.Checked = item.IsChecked;
    return product;
  }

  public static Protos.Shopping.ShoppingItem ToProto(this Product item) {
    var product = new Protos.Shopping.ShoppingItem {
      Name = item.Name,
      Price = item.Price.Value,
      Checked = false,
    };
    if (!string.IsNullOrWhiteSpace(item.Quantity)) {
      product.Quantity = item.Quantity;
    }
    return product;
  }

  public static Protos.Markets.MarketSummary ToProto(this MarketSummary summary) =>
    new() { Name = summary.Name, LogoUrl = summary.LogoUrl?.ToString() ?? string.Empty };

  public static Protos.Markets.Market ToProto(this Market market) =>
    new() {
      Name = market.Name,
      Products = { market.Products.Select(p => p.ToProto()) },
    };

  private static Protos.Markets.MarketProduct ToProto(this MarketProduct product) =>
    new() {
      Name = product.Name,
      BrandName = product.Brand.Name,
      Formats = {
        product.Formats.Select(f => new Protos.Markets.MarketProductFormat {
          Quantity = f.Quantity,
          Price = f.Price.Value,
          ImageUrl = f.ImageUrl?.ToString() ?? string.Empty,
        }),
      },
    };

  public static RecordShoppingList.CommandItem ToCommand(
    this Protos.Shopping.ShoppingItem protoProduct
  ) => new(
    protoProduct.Name,
    protoProduct.HasQuantity ? protoProduct.Quantity : null,
    protoProduct.HasPrice ? protoProduct.Price : 0f,
    protoProduct.Checked
  );

  public static AddItemsToList.CommandItem ToAddItemsCommand(
    this Protos.Shopping.ShoppingItem protoItem
  ) => new(
    protoItem.Name,
    protoItem.HasQuantity ? protoItem.Quantity : null,
    protoItem.HasPrice ? protoItem.Price : 0f,
    protoItem.Checked
  );
}