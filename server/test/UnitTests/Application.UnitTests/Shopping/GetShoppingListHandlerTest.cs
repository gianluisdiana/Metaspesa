using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.GetShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class GetShoppingListHandlerTest {
  private readonly IShoppingRepository _shoppingRepository;
  private readonly IMarketRepository _marketRepository;

  private readonly Handler handler;

  public GetShoppingListHandlerTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _marketRepository = Substitute.For<IMarketRepository>();
    handler = new Handler(_shoppingRepository, _marketRepository);
  }

  [Fact(DisplayName = "Returns shopping list enriched with market products if it exists")]
  public async Task Handler_ReturnsShoppingListEnrichedWithMarketProducts_IfItExists() {
    // Arrange
    var userUid = Guid.NewGuid();
    var format = new ProductFormat(new AQuantity(1, "l"), new Price(1.25m), null);
    _shoppingRepository
      .GetShoppingListAsync(userUid, "Test List", TestContext.Current.CancellationToken)
      .Returns(new AShoppingList("Test List", [new AShoppingItem(42, 3, true)]));
    _marketRepository
      .GetProductsAsync(
        Arg.Is<IReadOnlyCollection<int>>(ids => ids.Single() == 42),
        TestContext.Current.CancellationToken)
      .Returns(new Dictionary<int, MarketProduct> {
        [42] = new("Milk", new ProductBrand("Brand"), [format]),
      });

    // Act
    Result<Response> result = await handler.Handle(
      new Query(userUid, "Test List"), TestContext.Current.CancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Test List", result.Value.ShoppingListName);
    ResponseItem item = result.Value.Items.Single();
    Assert.Equal("Milk", item.ProductName);
    Assert.Equal(3, item.Amount);
    Assert.Equal(format, item.Format);
    Assert.True(item.IsChecked);
    await _shoppingRepository.Received(1).GetShoppingListAsync(
      userUid, "Test List", TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Returns missing error if repository returns null")]
  public async Task Handler_ReturnsMissingError_IfRepositoryReturnsNull() {
    // Arrange
    var userUid = Guid.NewGuid();
    _shoppingRepository
      .GetShoppingListAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns((AShoppingList?)null);

    // Act
    Result<Response> result = await handler.Handle(
      new Query(userUid, null), TestContext.Current.CancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
    DomainError error = result.Errors.Single();
    Assert.Equal("ShoppingList.NotFound", error.Code);
    Assert.Equal(ErrorKind.Missing, error.Kind);
    await _marketRepository.DidNotReceive().GetProductsAsync(
      Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>());
  }
}
