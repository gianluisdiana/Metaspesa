using Metaspesa.Domain.Shopping;

namespace Metaspesa.GrpcApi.Extensions;

internal static class ProtosExtensions {
  public static Protos.Shopping.ShoppingList ToProto(
    this ShoppingList shoppingList
  ) {
    var protoShoppingList = new Protos.Shopping.ShoppingList();
    if (!string.IsNullOrWhiteSpace(shoppingList.Name)) {
      protoShoppingList.Name = shoppingList.Name;
    }
    protoShoppingList.Products.AddRange(
      shoppingList.Select(item => item.ToProto()));
    return protoShoppingList;
  }

  private static Protos.Shopping.Product ToProto(this ShoppingItem item) {
    var product = new Protos.Shopping.Product {
      Name = item.Name,
      Checked = item.IsChecked,
    };
    if (item.Price is not null) {
      product.Price = item.Price.Value;
    }
    if (!string.IsNullOrWhiteSpace(item.Quantity)) {
      product.Quantity = item.Quantity;
    }
    return product;
  }

  public static Protos.Shopping.Product ToProto(this RegisteredItem item) {
    var product = new Protos.Shopping.Product {
      Name = item.Name,
      Checked = false,
    };
    if (item.LastPrice is not null) {
      product.Price = item.LastPrice.Value;
    }
    if (!string.IsNullOrWhiteSpace(item.Quantity)) {
      product.Quantity = item.Quantity;
    }
    return product;
  }

  public static ShoppingList ToDomain(
    this Protos.Shopping.ShoppingList protoShoppingList
  ) {
    const float Epsilon = 0.01f;

    var shoppingList = new ShoppingList(
      protoShoppingList.Name,
      protoShoppingList.Products.Select(p => new ShoppingItem(
        p.Name, p.Quantity, Math.Abs(p.Price) < Epsilon ? null : p.Price, p.Checked
      )).ToList()
    );
    return shoppingList;
  }
}