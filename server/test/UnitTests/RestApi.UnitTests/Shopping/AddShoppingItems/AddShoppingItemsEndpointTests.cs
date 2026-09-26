using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using Metaspesa.RestApi.Shopping.AddShoppingItems;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using static Metaspesa.RestApi.UnitTests.Shopping.ShoppingEndpointTestData;

namespace Metaspesa.RestApi.UnitTests.Shopping.AddShoppingItems;

public static class AddShoppingItemsEndpointTests {
  [Fact]
  public static async Task Add_AddsMultipleItemsAndReturnsNoContent() {
    ShoppingList list = PersistedList("Weekly");
    IShoppingListRepository repository = RepositoryWith(list);
    IProductRepository products = Substitute.For<IProductRepository>();
    products.GetProductsAsync(Arg.Any<IReadOnlyCollection<Guid>>(),
      TestContext.Current.CancellationToken).Returns(
        new Dictionary<Guid, MarketProduct> {
          [FormatId] = Product(FormatId),
          [OtherFormatId] = Product(OtherFormatId),
        });

    IResult result = await AddShoppingItemsEndpoint.AddItemsAsync(ListId,
      new AddShoppingItemsRequest([
        new ShoppingItemRequest(FormatId, 2, true),
        new ShoppingItemRequest(OtherFormatId, 3, false),
      ]), ShopperContext(),
      new AddItemsToList.Handler(repository, products, Substitute.For<IUnitOfWork>()),
      TestContext.Current.CancellationToken);

    ShoppingItem[] items = [.. list.Items];
    Assert.Equal((StatusCodes.Status204NoContent, 2,
      FormatId, 2, true, OtherFormatId, 3, false),
      (Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode,
        items.Length, items[0].ProductFormatId.Value, items[0].Amount.Value,
        items[0].IsChecked, items[1].ProductFormatId.Value,
        items[1].Amount.Value, items[1].IsChecked));
  }

  [Fact]
  public static async Task Add_RejectsNullItemsArray() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    IProductRepository products = Substitute.For<IProductRepository>();

    await Assert.ThrowsAsync<BadHttpRequestException>(() =>
      AddShoppingItemsEndpoint.AddItemsAsync(ListId,
        new AddShoppingItemsRequest(null), ShopperContext(),
        new AddItemsToList.Handler(repository, products,
          Substitute.For<IUnitOfWork>()),
        TestContext.Current.CancellationToken));
  }

  [Fact]
  public static async Task Add_RejectsNullItemInArray() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    IProductRepository products = Substitute.For<IProductRepository>();

    await Assert.ThrowsAsync<BadHttpRequestException>(() =>
      AddShoppingItemsEndpoint.AddItemsAsync(ListId,
        new AddShoppingItemsRequest([null!]), ShopperContext(),
        new AddItemsToList.Handler(repository, products,
          Substitute.For<IUnitOfWork>()),
        TestContext.Current.CancellationToken));
  }

  [Fact]
  public static async Task Add_RejectsEmptyItemsArray() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    IProductRepository products = Substitute.For<IProductRepository>();

    await Assert.ThrowsAsync<EmptyShoppingItemsException>(() =>
      AddShoppingItemsEndpoint.AddItemsAsync(ListId,
        new AddShoppingItemsRequest([]), ShopperContext(),
        new AddItemsToList.Handler(repository, products,
          Substitute.For<IUnitOfWork>()),
        TestContext.Current.CancellationToken));
  }
}