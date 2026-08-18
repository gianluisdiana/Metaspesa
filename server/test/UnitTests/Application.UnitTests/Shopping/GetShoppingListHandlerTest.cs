using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.GetShoppingList;
using MarketProductRepository = Metaspesa.Application.Abstractions.Markets.IProductRepository;

namespace Metaspesa.Application.UnitTests.Shopping;

public class GetShoppingListHandlerTest {
  private readonly IShoppingRepository _shoppingRepository;
  private readonly MarketProductRepository _productRepository;
  private readonly Handler _handler;

  public GetShoppingListHandlerTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    _productRepository = Substitute.For<MarketProductRepository>();
    _handler = new Handler(_shoppingRepository, _productRepository);
  }

  [Fact(DisplayName = "Returns shopping list enriched with product read model")]
  public async Task Handler_ReturnsEnrichedShoppingList_WhenItExists() {
    var userUid = Guid.NewGuid();
    var format = new MarketProductFormat(
      new Metaspesa.Domain.SharedKernel.Quantity(
        1, new UnitOfMeasure("l")),
      new Money(1.25m),
      null);
    _shoppingRepository
      .GetShoppingListAsync(
        userUid,
        "Test List",
        TestContext.Current.CancellationToken)
      .Returns(new AShoppingList(
        "Test List",
        [new AShoppingItem(42, 3, true)]));
    _productRepository
      .GetProductsAsync(
        Arg.Is<IReadOnlyCollection<int>>(ids => ids.Single() == 42),
        TestContext.Current.CancellationToken)
      .Returns(new Dictionary<int, MarketProduct> {
        [42] = new("Milk", "Brand", [format]),
      });

    Result<Response> result = await _handler.Handle(
      new Query(userUid, "Test List"),
      TestContext.Current.CancellationToken);

    Assert.True(result.IsSuccess);
    Assert.Equal("Test List", result.Value.ShoppingListName);
    ResponseItem item = Assert.Single(result.Value.Items);
    Assert.Equal("Milk", item.ProductName);
    Assert.Equal(3, item.Amount);
    Assert.Equal(format, item.Format);
    Assert.True(item.IsChecked);
  }

  [Fact(DisplayName = "Returns missing error without querying products")]
  public async Task Handler_ReturnsMissingError_WhenListDoesNotExist() {
    var userUid = Guid.NewGuid();
    _shoppingRepository
      .GetShoppingListAsync(
        userUid,
        null,
        TestContext.Current.CancellationToken)
      .Returns((AShoppingList?)null);

    Result<Response> result = await _handler.Handle(
      new Query(userUid, null),
      TestContext.Current.CancellationToken);

    Assert.False(result.IsSuccess);
    DomainError error = Assert.Single(result.Errors);
    Assert.Equal("ShoppingList.NotFound", error.Code);
    Assert.Equal(ErrorKind.Missing, error.Kind);
    await _productRepository.DidNotReceive().GetProductsAsync(
      Arg.Any<IReadOnlyCollection<int>>(),
      Arg.Any<CancellationToken>());
  }
}
