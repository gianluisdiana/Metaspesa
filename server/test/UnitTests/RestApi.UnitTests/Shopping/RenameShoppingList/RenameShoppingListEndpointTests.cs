using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using Metaspesa.RestApi.Shopping.RenameShoppingList;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using static Metaspesa.RestApi.UnitTests.Shopping.ShoppingEndpointTestData;

namespace Metaspesa.RestApi.UnitTests.Shopping.RenameShoppingList;

public class RenameShoppingListEndpointTests {
  private sealed class FakeHandler : UpdateShoppingList.Handler {
    private readonly IShoppingListRepository _repository;
    public FakeHandler(IShoppingListRepository repository) : base(repository) {
      _repository = repository;
    }

    public void WithHappyPath() {
      var list = ShoppingList.Create(Guid.CreateVersion7(), "Weekly");
      _repository.GetAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), TestContext.Current.CancellationToken)
        .Returns(list);
    }
  }

  private readonly FakeHandler _handler;

  public RenameShoppingListEndpointTests() {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    _handler = new FakeHandler(repository);
  }

  [Fact]
  public async Task Rename_ReturnsNoContent() {
    _handler.WithHappyPath();

    IResult result = await RenameShoppingListEndpoint.RenameAsync(
      Guid.CreateVersion7(),
      new RenameShoppingListRequest("Weekly"),
      ShopperContext(),
      _handler,
      TestContext.Current.CancellationToken);

    int? statusCode = Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false).StatusCode;
    Assert.Equal(StatusCodes.Status204NoContent, statusCode);
  }

  [Fact]
  public async Task Rename_RejectsMissingList() {
    Task action() => RenameShoppingListEndpoint.RenameAsync(
      Guid.CreateVersion7(),
      new RenameShoppingListRequest("Weekly"),
      ShopperContext(),
      _handler,
      TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<ShoppingListNotFoundException>(action);
  }
}