using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class ProductTests {
  private static readonly ImageUrl Image =
    new(new Uri("https://example.com/product.png"));

  [Fact(DisplayName = "Product references market by id and owns formats")]
  public static void Product_Created_WithBoundedFormats() {
    var format = new ProductFormat(
      new ProductFormatId(10),
      new Quantity(1, new UnitOfMeasure("kg")),
      Image);
    Product product = CreateProduct([format]);

    Assert.Equal(new MarketId(1), product.MarketId);
    Assert.Equal(format, Assert.Single(product.Formats));
    Assert.DoesNotContain(
      typeof(Product).GetProperties(),
      property => property.PropertyType == typeof(Market));
    Assert.DoesNotContain(
      typeof(Product).GetProperties(),
      property => property.PropertyType == typeof(PriceSnapshot));
  }

  [Fact(DisplayName = "Adds a product format")]
  public static void Product_AddFormat_AddsValidFormat() {
    Product product = CreateProduct();
    var id = new ProductFormatId(10);
    var quantity = new Quantity(1, new UnitOfMeasure("kg"));

    product.AddFormat(id, quantity, Image);

    ProductFormat format = Assert.Single(product.Formats);
    Assert.Equal(id, format.Id);
    Assert.Equal(quantity, format.Quantity);
    Assert.Equal(Image, format.ImageUrl);
  }

  [Fact(DisplayName = "Rejects duplicate product format without mutation")]
  public static void Product_AddFormat_ThrowsSpecificException_WhenIdExists() {
    var id = new ProductFormatId(10);
    var original = new ProductFormat(
      id,
      new Quantity(1, new UnitOfMeasure("kg")),
      Image);
    Product product = CreateProduct([original]);

    Assert.Throws<DuplicateProductFormatException>(
      () => product.AddFormat(
        id,
        new Quantity(2, new UnitOfMeasure("kg")),
        new ImageUrl(new Uri("https://example.com/other.png"))));
    Assert.Same(original, Assert.Single(product.Formats));
  }

  [Fact(DisplayName = "Updates an owned product format")]
  public static void Product_UpdateFormat_UpdatesValues() {
    var id = new ProductFormatId(10);
    var format = new ProductFormat(
      id,
      new Quantity(1, new UnitOfMeasure("kg")),
      Image);
    Product product = CreateProduct([format]);
    var quantity = new Quantity(2, new UnitOfMeasure("kg"));
    var image = new ImageUrl(new Uri("https://example.com/other.png"));

    product.UpdateFormat(id, quantity, image);

    Assert.Equal(quantity, format.Quantity);
    Assert.Equal(image, format.ImageUrl);
  }

  [Fact(DisplayName = "Rejects unknown product format without mutation")]
  public static void Product_UpdateFormat_ThrowsSpecificException_WhenIdIsUnknown() {
    var original = new ProductFormat(
      new ProductFormatId(10),
      new Quantity(1, new UnitOfMeasure("kg")),
      Image);
    Product product = CreateProduct([original]);

    Assert.Throws<ProductFormatNotFoundException>(
      () => product.UpdateFormat(
        new ProductFormatId(20),
        new Quantity(2, new UnitOfMeasure("kg")),
        Image));
    Assert.Same(original, Assert.Single(product.Formats));
  }

  [Fact(DisplayName = "Rejects duplicate formats when rehydrating product")]
  public static void Product_ThrowsSpecificException_WhenFormatsContainDuplicateId() {
    var id = new ProductFormatId(10);

    Assert.Throws<DuplicateProductFormatException>(
      () => CreateProduct([
        new ProductFormat(id, new Quantity(1, new UnitOfMeasure("kg")), Image),
        new ProductFormat(id, new Quantity(2, new UnitOfMeasure("kg")), Image),
      ]));
  }

  private static Product CreateProduct(
    IEnumerable<ProductFormat>? formats = null
  ) => new(
    new ProductId(1),
    new ProductName("Milk"),
    new BrandName("Acme"),
    new MarketId(1),
    formats);
}
