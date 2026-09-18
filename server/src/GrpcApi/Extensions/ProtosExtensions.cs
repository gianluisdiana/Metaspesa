using System.Globalization;
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
    ProductFormatUid = item.Format.ProductFormatUid,
  };

  public static AddItemsToList.CommandItem ToAddItemsCommand(
    this Protos.Shopping.AShoppingItem protoItem
  ) => new(
    protoItem.ProductFormatUid,
    protoItem.Amount,
    protoItem.IsChecked
  );
}