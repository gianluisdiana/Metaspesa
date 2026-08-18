using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using NSubstitute;
using static Metaspesa.Application.Shopping.GetShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class GetShoppingListHandlerTest {
  [Fact(DisplayName = "Returns enriched application read model")]
  public async Task Handle_ReturnsEnrichedReadModel() {
    var ownerId = Guid.CreateVersion7();
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(ShoppingTestData.List(
        ownerId, "Weekly", ShoppingTestData.Item(7, 2, true)));
    IProductRepository productRepository = Substitute.For<IProductRepository>();
    productRepository.GetProductsAsync(
      Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
      .Returns(new Dictionary<int, MarketProduct> {
        [7] = ShoppingTestData.MarketProduct(7),
      });
    var handler = new Handler(repository, productRepository);

    Response result = await handler.Handle(
      new Query(ownerId, "Weekly"), TestContext.Current.CancellationToken);

    Assert.Equal("Weekly", result.ShoppingListName);
    ResponseItem item = Assert.Single(result.Items);
    Assert.Equal("Milk", item.ProductName);
    Assert.Equal(2, item.Amount);
    Assert.True(item.IsChecked);
  }

  [Fact(DisplayName = "Returns empty read model for empty list")]
  public async Task Handle_ReturnsEmptyItems_WhenListIsEmpty() {
    var ownerId = Guid.CreateVersion7();
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(ShoppingTestData.List(ownerId));
    IProductRepository productRepository = Substitute.For<IProductRepository>();
    productRepository.GetProductsAsync(
      Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
      .Returns(new Dictionary<int, MarketProduct>());
    var handler = new Handler(repository, productRepository);

    Response result = await handler.Handle(
      new Query(ownerId, "Weekly"), TestContext.Current.CancellationToken);

    Assert.Empty(result.Items);
  }

  [Fact(DisplayName = "Rejects missing list")]
  public async Task Handle_ThrowsExactException_WhenListIsMissing() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    IProductRepository productRepository = Substitute.For<IProductRepository>();
    var handler = new Handler(repository, productRepository);

    await Assert.ThrowsAsync<ShoppingListNotFoundException>(() => handler.Handle(
      new Query(Guid.CreateVersion7(), "Weekly"),
      TestContext.Current.CancellationToken));
  }
}
