using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.RestApi.Shopping.GetShoppingLists;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using static Metaspesa.RestApi.UnitTests.Shopping.ShoppingEndpointTestData;

namespace Metaspesa.RestApi.UnitTests.Shopping.GetShoppingLists;

public static class GetShoppingListsEndpointTests {
  [Fact]
  public static async Task List_ReturnsEmptyCollection_WhenShopperOwnsNoLists() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetByOwnerAsync(new UserId(OwnerUid),
      TestContext.Current.CancellationToken).Returns([]);

    IResult result = await GetShoppingListsEndpoint.ListAsync(
      ShopperContext(), new GetShoppingListSummaries.Handler(repository),
      TestContext.Current.CancellationToken);

    Assert.Empty(Assert.IsType<Ok<ShoppingListCollectionResponse>>(result)
      .Value!.Items);
  }

  [Fact]
  public static async Task List_ReturnsNamedAndTemporaryListIds() {
    var temporaryListId = Guid.CreateVersion7();
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetByOwnerAsync(new UserId(OwnerUid),
      TestContext.Current.CancellationToken).Returns([
        PersistedList("Weekly"),
        ShoppingList.Rehydrate(new ShoppingListId(temporaryListId),
          [new UserId(OwnerUid)], null, null, []),
      ]);

    IResult result = await GetShoppingListsEndpoint.ListAsync(
      ShopperContext(), new GetShoppingListSummaries.Handler(repository),
      TestContext.Current.CancellationToken);

    Assert.Equal([
      new ShoppingListSummaryResponse(ListId, "Weekly", false),
      new ShoppingListSummaryResponse(temporaryListId, null, true),
    ], Assert.IsType<Ok<ShoppingListCollectionResponse>>(result).Value!.Items);
  }

  [Fact]
  public static async Task List_RejectsRequestWithoutUserIdClaim() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
      GetShoppingListsEndpoint.ListAsync(new DefaultHttpContext(),
        new GetShoppingListSummaries.Handler(repository),
        TestContext.Current.CancellationToken));
  }
}