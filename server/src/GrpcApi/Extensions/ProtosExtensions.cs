using System.Globalization;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.GrpcApi.Extensions;

internal static class ProtosExtensions {
  public static Protos.Shopping.ShoppingList ToProto(
    this GetShoppingList.Response shoppingList
  ) {
    var protoShoppingList = new Protos.Shopping.ShoppingList();
    if (!string.IsNullOrWhiteSpace(shoppingList.ShoppingListName)) {
      protoShoppingList.Name = shoppingList.ShoppingListName;
    }
    protoShoppingList.Items.AddRange(
      shoppingList.Items.Select(item => item.ToProto()));
    return protoShoppingList;
  }

  public static Protos.Shopping.ShoppingListSummary ToSummaryProto(
    this AShoppingList summary
  ) {
    var protoSummary = new Protos.Shopping.ShoppingListSummary();
    if (!string.IsNullOrWhiteSpace(summary.Name)) {
      protoSummary.Name = summary.Name;
    }
    return protoSummary;
  }

  private static Protos.Shopping.ShoppingItem ToProto(
    this GetShoppingList.ResponseItem item
  ) => new() {
    Name = item.ProductName,
    Quantity = string.Create(
      CultureInfo.InvariantCulture,
      $"{item.Format.Quantity.Value:G} {item.Format.Quantity.UnitOfMeasure}"),
    Price = GrpcPriceConverter.ToProto(item.Format.Price.Value),
    Checked = item.IsChecked,
  };

  public static Protos.Shopping.ShoppingItem ToProto(this Product item) {
    var product = new Protos.Shopping.ShoppingItem {
      Name = item.Name,
      Price = GrpcPriceConverter.ToProto(item.Price.Value),
      Checked = false,
    };
    if (!string.IsNullOrWhiteSpace(item.Quantity?.Value)) {
      product.Quantity = item.Quantity.Value;
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
          Quantity = string.Create(
            CultureInfo.InvariantCulture,
            $"{f.Quantity.Value:G} {f.Quantity.UnitOfMeasure}"),
          Price = GrpcPriceConverter.ToProto(f.Price.Value),
          ImageUrl = f.ImageUrl?.ToString() ?? string.Empty,
        }),
      },
    };

  public static RecordShoppingList.CommandItem ToCommand(
    this Protos.Shopping.ShoppingItem protoProduct
  ) => new(
    GrpcTextSanitizer.SanitizeAscii(protoProduct.Name),
    protoProduct.HasQuantity ? GrpcTextSanitizer.SanitizeAscii(protoProduct.Quantity) : null,
    protoProduct.HasPrice ? GrpcPriceConverter.ToDecimal(protoProduct.Price) : 0m,
    protoProduct.Checked
  );

  public static AddItemsToList.CommandItem ToAddItemsCommand(
    this Protos.Shopping.AShoppingItem protoItem
  ) => new(
    protoItem.ReferenceUid,
    protoItem.Amount,
    protoItem.IsChecked
  );
}
