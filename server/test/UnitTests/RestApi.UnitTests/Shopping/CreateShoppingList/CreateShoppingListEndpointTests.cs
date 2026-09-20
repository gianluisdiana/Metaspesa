using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using Metaspesa.RestApi.Shopping.CreateShoppingList;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using static Metaspesa.RestApi.UnitTests.Shopping.ShoppingEndpointTestData;
using CreateListUseCase = Metaspesa.Application.Shopping.CreateShoppingList;

namespace Metaspesa.RestApi.UnitTests.Shopping.CreateShoppingList;

public static class CreateShoppingListEndpointTests {
  [Fact]
  public static async Task Create_ReturnsNewListIdAndLocation() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.AddAsync(Arg.Any<ShoppingList>(),
      TestContext.Current.CancellationToken).Returns(ListId);

    IResult result = await CreateShoppingListEndpoint.CreateAsync(
      new CreateShoppingListRequest("Weekly"), ShopperContext(),
      new CreateListUseCase.Handler(repository),
      TestContext.Current.CancellationToken);

    Created<IdResponse> response = Assert.IsType<Created<IdResponse>>(result);
    Assert.Equal((ListId, $"/api/v1/shopping-lists/{ListId}"),
      (response.Value!.Id, response.Location));
  }

  [Fact]
  public static async Task Create_MapsMissingNameToTemporaryList() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();

    await CreateShoppingListEndpoint.CreateAsync(
      new CreateShoppingListRequest(null), ShopperContext(),
      new CreateListUseCase.Handler(repository),
      TestContext.Current.CancellationToken);

    await repository.Received(1).AddAsync(Arg.Is<ShoppingList>(list =>
      list.IsTemporary && list.OwnerIds.Single() == new UserId(OwnerUid)),
      TestContext.Current.CancellationToken);
  }

  [Fact]
  public static async Task Create_RejectsDuplicateName() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.ExistsAsync(new UserId(OwnerUid),
      new ShoppingListName("Weekly"), TestContext.Current.CancellationToken)
      .Returns(true);

    await Assert.ThrowsAsync<ShoppingListAlreadyExistsException>(() =>
      CreateShoppingListEndpoint.CreateAsync(
        new CreateShoppingListRequest("Weekly"), ShopperContext(),
        new CreateListUseCase.Handler(repository),
        TestContext.Current.CancellationToken));
  }
}