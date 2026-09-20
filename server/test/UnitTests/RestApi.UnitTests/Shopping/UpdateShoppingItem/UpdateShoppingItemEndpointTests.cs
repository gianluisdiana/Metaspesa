using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using Metaspesa.RestApi.Shopping.UpdateShoppingItem;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using static Metaspesa.RestApi.UnitTests.Shopping.ShoppingEndpointTestData;

namespace Metaspesa.RestApi.UnitTests.Shopping.UpdateShoppingItem;

public static class UpdateShoppingItemEndpointTests {
  [Fact]
  public static async Task Update_ChangesItemAndReturnsNoContent() {
    ShoppingList list = PersistedList("Weekly",
      new ShoppingItem(new ProductFormatId(FormatId),
        new PositiveAmount(1), false));
    IShoppingListRepository repository = RepositoryWith(list);

    IResult result = await UpdateShoppingItemEndpoint.UpdateItemAsync(ListId, FormatId,
      new UpdateShoppingItemRequest(3, true), ShopperContext(),
      new UpdateItem.Handler(repository, Substitute.For<IUnitOfWork>()),
      TestContext.Current.CancellationToken);

    ShoppingItem item = list.Items.Single();
    Assert.Equal((StatusCodes.Status204NoContent, 3, true),
      (Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode,
        item.Amount.Value, item.IsChecked));
  }

  [Fact]
  public static async Task Update_RejectsRequestWithoutChanges() {
    ShoppingList list = PersistedList("Weekly",
      new ShoppingItem(new ProductFormatId(FormatId),
        new PositiveAmount(1), false));
    IShoppingListRepository repository = RepositoryWith(list);

    await Assert.ThrowsAsync<EmptyShoppingItemUpdateException>(() =>
      UpdateShoppingItemEndpoint.UpdateItemAsync(ListId, FormatId,
        new UpdateShoppingItemRequest(null, null), ShopperContext(),
        new UpdateItem.Handler(repository, Substitute.For<IUnitOfWork>()),
        TestContext.Current.CancellationToken));
  }

  [Fact]
  public static async Task Update_PreservesAmountWhenOnlyCheckedStateChanges() {
    ShoppingList list = PersistedList("Weekly",
      new ShoppingItem(new ProductFormatId(FormatId),
        new PositiveAmount(2), false));
    IShoppingListRepository repository = RepositoryWith(list);

    await UpdateShoppingItemEndpoint.UpdateItemAsync(ListId, FormatId,
      new UpdateShoppingItemRequest(null, true), ShopperContext(),
      new UpdateItem.Handler(repository, Substitute.For<IUnitOfWork>()),
      TestContext.Current.CancellationToken);

    ShoppingItem item = list.Items.Single();
    Assert.Equal((2, true), (item.Amount.Value, item.IsChecked));
  }
}