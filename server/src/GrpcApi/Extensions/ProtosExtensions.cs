using Metaspesa.Application.Shopping;
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
    protoShoppingList.Products.AddRange(
      shoppingList.Select(item => item.ToProto()));
    return protoShoppingList;
  }

  private static Protos.Shopping.Product ToProto(this ShoppingItem item) {
    Protos.Shopping.Product product = ((Product)item).ToProto();
    product.Checked = item.IsChecked;
    return product;
  }

  public static Protos.Shopping.Product ToProto(this Product item) {
    var product = new Protos.Shopping.Product {
      Name = item.Name,
      Price = item.Price.Value,
      Checked = false,
    };
    if (!string.IsNullOrWhiteSpace(item.Quantity)) {
      product.Quantity = item.Quantity;
    }
    return product;
  }

  public static RecordShoppingList.CommandItem ToCommand(
    this Protos.Shopping.Product protoProduct
  ) {
    var product = new RecordShoppingList.CommandItem(
      protoProduct.Name,
      protoProduct.Quantity,
      protoProduct.Price,
      protoProduct.Checked
    );
    return product;
  }
}