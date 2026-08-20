using Metaspesa.GrpcApi.Protos.Shopping;

namespace Metaspesa.GrpcApi.UnitTests.Shopping;

public class ShoppingProtoContractTests {
  [Fact(DisplayName = "Add item uses product_format_uid at tag 1")]
  public void AddItem_UsesProductFormatUid_AtTagOne() {
    Assert.Equal(
      "product_format_uid",
      AShoppingItem.Descriptor.FindFieldByNumber(1)?.Name);
  }

  [Fact(DisplayName = "Update item uses product_format_uid at tag 2")]
  public void UpdateItem_UsesProductFormatUid_AtTagTwo() {
    Assert.Equal(
      "product_format_uid",
      UpdateItemRequest.Descriptor.FindFieldByNumber(2)?.Name);
  }

  [Fact(DisplayName = "Remove item uses product_format_uid at tag 2")]
  public void RemoveItem_UsesProductFormatUid_AtTagTwo() {
    Assert.Equal(
      "product_format_uid",
      RemoveItemRequest.Descriptor.FindFieldByNumber(2)?.Name);
  }
}