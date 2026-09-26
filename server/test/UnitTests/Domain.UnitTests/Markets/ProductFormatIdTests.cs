using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class ProductFormatIdTests {
  [Fact(DisplayName = "Rejects empty product format id")]
  public static void ProductFormatId_ThrowsSpecificException_WhenValueIsInvalid() =>
    Assert.Throws<InvalidProductFormatIdException>(() =>
      new ProductFormatId(Guid.Empty));

  [Fact(DisplayName = "Creates and compares product formats ids by value")]
  public static void ProductFormatId_CreatedAndEqual_WhenValueIsPositive() {
    var rawId = Guid.Parse("00000000-0000-7000-8000-000000000001");
    var id = new ProductFormatId(rawId);

    Assert.Equal(rawId, id.Value);
    Assert.Equal(id, new ProductFormatId(rawId));
  }
}