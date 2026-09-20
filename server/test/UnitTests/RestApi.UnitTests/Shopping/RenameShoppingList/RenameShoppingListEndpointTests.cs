using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using Metaspesa.RestApi.Shopping.RenameShoppingList;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using static Metaspesa.RestApi.UnitTests.Shopping.ShoppingEndpointTestData;

namespace Metaspesa.RestApi.UnitTests.Shopping.RenameShoppingList;

public static class RenameShoppingListEndpointTests {
  [Fact]
  public static async Task Rename_SanitizesNameAndReturnsNoContent() {
    ShoppingList list = PersistedList("Old");
    IShoppingListRepository repository = RepositoryWith(list);

    IResult result = await RenameShoppingListEndpoint.RenameAsync(ListId,
      new RenameShoppingListRequest(" Weekly \u2713 "), ShopperContext(),
      new UpdateShoppingList.Handler(repository, Substitute.For<IUnitOfWork>()),
      TestContext.Current.CancellationToken);

    Assert.Equal((new ShoppingListName("Weekly"), StatusCodes.Status204NoContent),
      (list.Name, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode));
  }

  [Fact]
  public static async Task Rename_RejectsMissingList() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();

    await Assert.ThrowsAsync<ShoppingListNotFoundException>(() =>
      RenameShoppingListEndpoint.RenameAsync(ListId,
        new RenameShoppingListRequest("Weekly"), ShopperContext(),
        new UpdateShoppingList.Handler(repository, Substitute.For<IUnitOfWork>()),
        TestContext.Current.CancellationToken));
  }
}