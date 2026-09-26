using Metaspesa.Application.Abstractions.Shopping;
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
  public async Task Create_ReturnsNewLocation() {
    var request = new CreateShoppingListRequest("Weekly");

    IResult result = await CreateShoppingListEndpoint.CreateAsync(
      request, ShopperContext(), _handler, TestContext.Current.CancellationToken);

    Created<IdResponse> response = Assert.IsType<Created<IdResponse>>(result);
    Assert.Equal($"/api/v1/shopping-lists/{response.Value!.Id}", response.Location);
  }
}