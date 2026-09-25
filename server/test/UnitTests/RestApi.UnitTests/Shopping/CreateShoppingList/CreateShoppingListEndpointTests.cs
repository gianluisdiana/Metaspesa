using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using Metaspesa.RestApi.Shopping.CreateShoppingList;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using static Metaspesa.RestApi.UnitTests.Shopping.ShoppingEndpointTestData;
using CreateListUseCase = Metaspesa.Application.Shopping.CreateShoppingList;

namespace Metaspesa.RestApi.UnitTests.Shopping.CreateShoppingList;

public class CreateShoppingListEndpointTests {
  private readonly IShoppingListRepository _repository;
  private readonly CreateListUseCase.Handler _handler;

  public CreateShoppingListEndpointTests() {
    _repository = Substitute.For<IShoppingListRepository>();

    _handler = new CreateListUseCase.Handler(_repository);
  }

  [Fact]
  public async Task Create_ReturnsNewListId() {
    var request = new CreateShoppingListRequest("Weekly");

    _repository.AddAsync(Arg.Any<ShoppingList>(),
      TestContext.Current.CancellationToken).Returns(ListId);

    IResult result = await CreateShoppingListEndpoint.CreateAsync(
      request, ShopperContext(), _handler, TestContext.Current.CancellationToken);

    Created<IdResponse> response = Assert.IsType<Created<IdResponse>>(result);
    Assert.Equal(ListId, response.Value!.Id);
  }

  [Fact]
  public async Task Create_ReturnsNewLocation() {
    var request = new CreateShoppingListRequest("Weekly");

    _repository.AddAsync(Arg.Any<ShoppingList>(),
      TestContext.Current.CancellationToken).Returns(ListId);

    IResult result = await CreateShoppingListEndpoint.CreateAsync(
      request, ShopperContext(), _handler, TestContext.Current.CancellationToken);

    Created<IdResponse> response = Assert.IsType<Created<IdResponse>>(result);
    Assert.Equal($"/api/v1/shopping-lists/{ListId}", response.Location);
  }

  [Fact]
  public async Task Create_RejectsDuplicateName() {
    var request = new CreateShoppingListRequest("Weekly");
    _repository.ExistsAsync(OwnerUid, request.Name, TestContext.Current.CancellationToken)
      .Returns(true);

    async Task action() => await CreateShoppingListEndpoint.CreateAsync(
      request, ShopperContext(), _handler, TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<ShoppingListAlreadyExistsException>(action);
  }
}