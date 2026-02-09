using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.GetRegisteredItems;

namespace Metaspesa.Application.UnitTests.Shopping;

public class GetRegisteredItemsHandlerTest {
  private readonly IProductRepository _productRepository;

  private readonly Handler handler;

  public GetRegisteredItemsHandlerTest() {
    _productRepository = Substitute.For<IProductRepository>();
    handler = new Handler(_productRepository);
  }

  [Fact(DisplayName = "Returns registered items from repository")]
  public async Task Handler_ReturnsRegisteredItemsFromRepository() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<RegisteredItem> expectedItems = [
      new("Test Item", null, null),
    ];
    _productRepository
      .GetRegisteredItemsAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(expectedItems);

    // Act
    Result<IReadOnlyCollection<RegisteredItem>> result = await handler.Handle(
      new Query(userUid), TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(expectedItems, result.Value);
  }

}