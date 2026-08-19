using System.Globalization;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Shopping;

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
    this GetShoppingListSummaries.Response summary
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
      $"{item.Format.Quantity.Amount:G} {item.Format.Quantity.UnitOfMeasure.Value}"),
    Price = GrpcPriceConverter.ToProto(item.Format.Price.Amount),
    Checked = item.IsChecked,
  };

  public static Protos.Markets.MarketSummary ToProto(this MarketSummary summary) =>
    new() { Name = summary.Name, LogoUrl = summary.LogoUrl?.ToString() ?? string.Empty };

  public static Protos.Markets.Market ToProto(this MarketCatalog market) =>
    new() {
      Name = market.Name,
      Products = { market.Products.Select(p => p.ToProto()) },
    };

  private static Protos.Markets.MarketProduct ToProto(this MarketProduct product) =>
    new() {
      Name = product.Name,
      BrandName = product.BrandName,
      Formats = {
        product.Formats.Select(f => new Protos.Markets.MarketProductFormat {
          Quantity = string.Create(
            CultureInfo.InvariantCulture,
            $"{f.Quantity.Amount:G} {f.Quantity.UnitOfMeasure.Value}"),
          Price = GrpcPriceConverter.ToProto(f.Price.Amount),
          ImageUrl = f.ImageUrl?.ToString() ?? string.Empty,
        }),
      },
    };

  public static AddItemsToList.CommandItem ToAddItemsCommand(
    this Protos.Shopping.AShoppingItem protoItem
  ) => new(
    protoItem.ReferenceUid,
    protoItem.Amount,
    protoItem.IsChecked
  );
}
