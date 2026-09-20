using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using Metaspesa.RestApi.Shopping.GetShoppingList;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using static Metaspesa.RestApi.UnitTests.Shopping.ShoppingEndpointTestData;
using GetList = Metaspesa.Application.Shopping.GetShoppingList;

namespace Metaspesa.RestApi.UnitTests.Shopping.GetShoppingList;

public static class GetShoppingListEndpointTests {
  [Fact]
  public static async Task Get_ReturnsCurrentProductDetailsForOwnedList() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    IProductRepository products = Substitute.For<IProductRepository>();
    repository.GetAsync(new UserId(OwnerUid), new ShoppingListId(ListId),
      TestContext.Current.CancellationToken).Returns(PersistedList("Weekly",
        new ShoppingItem(new ProductFormatId(FormatId), new PositiveAmount(2), true)));
    products.GetProductsAsync(Arg.Any<IReadOnlyCollection<int>>(),
      TestContext.Current.CancellationToken).Returns(
        new Dictionary<int, MarketProduct> { [FormatId] = Product() });

    IResult result = await GetShoppingListEndpoint.GetAsync(ListId,
      ShopperContext(), new GetList.Handler(repository, products),
      TestContext.Current.CancellationToken);

    Assert.Equal(new ShoppingItemResponse(FormatId, "Milk", "Brand",
      new ShoppingMarketResponse(4, "Market"),
      new ShoppingQuantityResponse(1, "l"),
      new ShoppingMoneyResponse(1.25m, "EUR"), 2, true,
      "https://example.test/milk"),
      Assert.IsType<Ok<ShoppingListResponse>>(result).Value!.Items.Single());
  }

  [Fact]
  public static async Task Get_ReturnsTemporaryListWithoutName() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    IProductRepository products = Substitute.For<IProductRepository>();
    repository.GetAsync(new UserId(OwnerUid), new ShoppingListId(ListId),
      TestContext.Current.CancellationToken).Returns(PersistedList(null));
    products.GetProductsAsync(Arg.Any<IReadOnlyCollection<int>>(),
      TestContext.Current.CancellationToken).Returns(
        new Dictionary<int, MarketProduct>());

    IResult result = await GetShoppingListEndpoint.GetAsync(ListId,
      ShopperContext(), new GetList.Handler(repository, products),
      TestContext.Current.CancellationToken);

    ShoppingListResponse response =
      Assert.IsType<Ok<ShoppingListResponse>>(result).Value!;
    Assert.Equal((ListId, null as string, true, 0),
      (response.Id, response.Name, response.IsTemporary, response.Items.Count));
  }

  [Fact]
  public static async Task Get_RejectsListNotOwnedByShopper() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    IProductRepository products = Substitute.For<IProductRepository>();

    await Assert.ThrowsAsync<ShoppingListNotFoundException>(() =>
      GetShoppingListEndpoint.GetAsync(ListId, ShopperContext(),
        new GetList.Handler(repository, products),
        TestContext.Current.CancellationToken));
  }

  [Fact]
  public static async Task Get_RejectsZeroListId() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    IProductRepository products = Substitute.For<IProductRepository>();

    await Assert.ThrowsAsync<InvalidShoppingListIdException>(() =>
      GetShoppingListEndpoint.GetAsync(0, ShopperContext(),
        new GetList.Handler(repository, products),
        TestContext.Current.CancellationToken));
  }
}