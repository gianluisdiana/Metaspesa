using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.RestApi.Shopping.AddShoppingItems;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using static Metaspesa.RestApi.UnitTests.Shopping.ShoppingEndpointTestData;

namespace Metaspesa.RestApi.UnitTests.Shopping.AddShoppingItems;

public class AddShoppingItemsEndpointTests {
  private sealed class FakeHandler : AddItemsToList.Handler {
    private readonly IShoppingListRepository _shoppingListRepository;
    private readonly IProductRepository _productRepository;

    public FakeHandler(
      IShoppingListRepository shoppingListRepository,
      IProductRepository productRepository
    ) : base(shoppingListRepository, productRepository) {
      _shoppingListRepository = shoppingListRepository;
      _productRepository = productRepository;
    }

    public void WithHappyPath() {
      _shoppingListRepository.GetAsync(
          Arg.Any<Guid>(), Arg.Any<Guid>(), TestContext.Current.CancellationToken)
        .Returns(ShoppingList.Create(Guid.CreateVersion7(), "Weekly"));
      _productRepository.CheckFormatsExistAsync(
          Arg.Any<IEnumerable<Guid>>(), TestContext.Current.CancellationToken)
        .Returns(true);
    }
  }

  private readonly FakeHandler _handler;

  public AddShoppingItemsEndpointTests() {
    _handler = new FakeHandler(
      Substitute.For<IShoppingListRepository>(),
      Substitute.For<IProductRepository>());
  }

  [Fact(DisplayName = "Returns no content for accepted request")]
  public async Task Add_ReturnsNoContent_WhenRequestIsAccepted() {
    _handler.WithHappyPath();

    var request = new AddShoppingItemsRequest([
      new ShoppingItemRequest(Guid.CreateVersion7(), 2, true),
      new ShoppingItemRequest(Guid.CreateVersion7(), 3, false),
    ]);

    IResult result = await AddShoppingItemsEndpoint.AddItemsAsync(
      Guid.CreateVersion7(),
      request,
      ShopperContext(),
      _handler,
      TestContext.Current.CancellationToken);

    IStatusCodeHttpResult statusCodeHttpResult = Assert.IsType<IStatusCodeHttpResult>(
      result, exactMatch: false);
    Assert.Equal(StatusCodes.Status204NoContent, statusCodeHttpResult.StatusCode);
  }

  [Fact(DisplayName = "Rejects null items array")]
  public async Task Add_RejectsNullItemsArray() {
    var request = new AddShoppingItemsRequest(null);

    async Task action() => await AddShoppingItemsEndpoint.AddItemsAsync(
      Guid.CreateVersion7(),
      request,
      ShopperContext(),
      _handler,
      TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<BadHttpRequestException>(action);
  }

  [Fact(DisplayName = "Rejects null item in array")]
  public async Task Add_RejectsNullItemInArray() {
    var request = new AddShoppingItemsRequest([null!]);

    async Task action() => await AddShoppingItemsEndpoint.AddItemsAsync(
      Guid.CreateVersion7(),
      request,
      ShopperContext(),
      _handler,
      TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<BadHttpRequestException>(action);
  }

  [Fact(DisplayName = "Rejects request without authenticated user ID")]
  public async Task Add_RejectsRequestWithoutUserIdClaim() {
    var request = new AddShoppingItemsRequest([
      new ShoppingItemRequest(Guid.CreateVersion7(), 2, true),
    ]);

    async Task action() => await AddShoppingItemsEndpoint.AddItemsAsync(
      Guid.CreateVersion7(),
      request,
      new DefaultHttpContext(),
      _handler,
      TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<UnauthorizedAccessException>(action);
  }
}