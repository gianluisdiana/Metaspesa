using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using Metaspesa.RestApi.Shopping.RemoveShoppingItem;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using static Metaspesa.RestApi.UnitTests.Shopping.ShoppingEndpointTestData;

namespace Metaspesa.RestApi.UnitTests.Shopping.RemoveShoppingItem;

public static class RemoveShoppingItemEndpointTests {
  [Fact]
  public static async Task Remove_DeletesSelectedItemAndReturnsNoContent() {
    ShoppingList list = PersistedList("Weekly",
      new ShoppingItem(new ProductFormatId(FormatId),
        new PositiveAmount(1), false));
    IShoppingListRepository repository = RepositoryWith(list);

    IResult result = await RemoveShoppingItemEndpoint.RemoveItemAsync(ListId, FormatId,
      ShopperContext(),
      new RemoveItem.Handler(repository, Substitute.For<IUnitOfWork>()),
      TestContext.Current.CancellationToken);

    Assert.Equal((StatusCodes.Status204NoContent, 0),
      (Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode,
        list.Items.Count));
  }

  [Fact]
  public static async Task Remove_RejectsMissingItem() {
    ShoppingList list = PersistedList("Weekly");
    IShoppingListRepository repository = RepositoryWith(list);

    await Assert.ThrowsAsync<ShoppingItemNotFoundException>(() =>
      RemoveShoppingItemEndpoint.RemoveItemAsync(ListId, FormatId,
        ShopperContext(),
        new RemoveItem.Handler(repository, Substitute.For<IUnitOfWork>()),
        TestContext.Current.CancellationToken));
  }
}