using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using NSubstitute;
using static Metaspesa.Application.Shopping.GetShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class GetShoppingListHandlerTest {
  private static readonly Guid FormatId = Guid.CreateVersion7();

  [Fact(DisplayName = "Returns enriched application read model")]
  public async Task Handle_ReturnsEnrichedReadModel() {
    var ownerId = Guid.CreateVersion7();
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListId>(), Arg.Any<CancellationToken>())
      .Returns(ShoppingTestData.List(
        ownerId, "Weekly", ShoppingTestData.Item(FormatId, 2, true)));
    IProductRepository productRepository = Substitute.For<IProductRepository>();
    productRepository.GetProductsAsync(
      Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
      .Returns(new Dictionary<Guid, MarketProduct> {
        [FormatId] = ShoppingTestData.MarketProduct(FormatId),
      });
    var handler = new Handler(repository, productRepository);

    Response result = await handler.Handle(
      new Query(ownerId, ShoppingTestData.ListId), TestContext.Current.CancellationToken);

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
      Arg.Any<UserId>(), Arg.Any<ShoppingListId>(), Arg.Any<CancellationToken>())
      .Returns(ShoppingTestData.List(ownerId));
    IProductRepository productRepository = Substitute.For<IProductRepository>();
    productRepository.GetProductsAsync(
      Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
      .Returns(new Dictionary<Guid, MarketProduct>());
    var handler = new Handler(repository, productRepository);

    Response result = await handler.Handle(
      new Query(ownerId, ShoppingTestData.ListId), TestContext.Current.CancellationToken);

    Assert.Empty(result.Items);
  }

  [Fact(DisplayName = "Rejects missing list")]
  public async Task Handle_ThrowsExactException_WhenListIsMissing() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    IProductRepository productRepository = Substitute.For<IProductRepository>();
    var handler = new Handler(repository, productRepository);

    await Assert.ThrowsAsync<ShoppingListNotFoundException>(() => handler.Handle(
      new Query(Guid.CreateVersion7(), Guid.CreateVersion7()),
      TestContext.Current.CancellationToken));
  }

  [Fact(DisplayName = "Rejects missing product projection")]
  public async Task Handle_ThrowsExactException_WhenProductFormatIsMissing() {
    var ownerId = Guid.CreateVersion7();
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(
      new UserId(ownerId), new ShoppingListId(ShoppingTestData.ListId), TestContext.Current.CancellationToken)
      .Returns(ShoppingTestData.List(ownerId, null, ShoppingTestData.Item(FormatId)));
    IProductRepository productRepository = Substitute.For<IProductRepository>();
    productRepository.GetProductsAsync(
      Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
      .Returns(new Dictionary<Guid, MarketProduct>());
    var handler = new Handler(repository, productRepository);

    await Assert.ThrowsAsync<ShoppingProductFormatNotFoundException>(() => handler.Handle(
      new Query(ownerId, ShoppingTestData.ListId), TestContext.Current.CancellationToken));
  }

  [Fact(DisplayName = "Does not load products when list is missing")]
  public async Task Handle_DoesNotLoadProducts_WhenListIsMissing() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    IProductRepository productRepository = Substitute.For<IProductRepository>();
    var handler = new Handler(repository, productRepository);

    await Assert.ThrowsAsync<ShoppingListNotFoundException>(() => handler.Handle(
      new Query(Guid.CreateVersion7(), Guid.CreateVersion7()),
      TestContext.Current.CancellationToken));

    await productRepository.DidNotReceive().GetProductsAsync(
      Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_ReturnsOwnedListSelectedById() {
    var ownerId = Guid.CreateVersion7();
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(new UserId(ownerId), new ShoppingListId(ShoppingTestData.ListId),
      TestContext.Current.CancellationToken)
      .Returns(ShoppingTestData.List(ownerId, "Weekly"));
    IProductRepository productRepository = Substitute.For<IProductRepository>();
    var handler = new Handler(repository, productRepository);

    Response result = await handler.Handle(
      new Query(ownerId, ShoppingTestData.ListId), TestContext.Current.CancellationToken);

    Assert.Equal(ShoppingTestData.ListId, result.Id);
  }

  [Fact]
  public async Task Handle_RejectsListIdNotOwnedByUser() {
    var ownerId = Guid.CreateVersion7();
    var requestedListId = Guid.CreateVersion7();
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(new UserId(ownerId),
      new ShoppingListId(ShoppingTestData.ListId),
      TestContext.Current.CancellationToken)
      .Returns(ShoppingTestData.List(ownerId));
    IProductRepository productRepository = Substitute.For<IProductRepository>();
    var handler = new Handler(repository, productRepository);

    await Assert.ThrowsAsync<ShoppingListNotFoundException>(() =>
      handler.Handle(new Query(ownerId, requestedListId),
        TestContext.Current.CancellationToken));
  }
}