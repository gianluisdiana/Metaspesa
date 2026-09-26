using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using NSubstitute;
using static Metaspesa.Application.Shopping.GetShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class GetShoppingListHandlerTest {
  private readonly IShoppingListRepository _repository;

  private readonly Handler _handler;

  public GetShoppingListHandlerTest() {
    _repository = Substitute.For<IShoppingListRepository>();

    _handler = new Handler(_repository);
  }

  [Fact(DisplayName = "Returns read model from repository")]
  public async Task Handle_ReturnsReadModelFromRepository() {
    var expectedResponse = new Response(
      Guid.CreateVersion7(), "My Shopping List", []);

    var query = new Query(Guid.CreateVersion7(), Guid.CreateVersion7());
    _repository.GetWithPricesAsync(
      query.UserUid, query.ShoppingListId, Arg.Any<CancellationToken>()
    ).Returns(expectedResponse);

    Response actualResponse = await _handler.Handle(
      query, TestContext.Current.CancellationToken);

    Assert.Equal(expectedResponse, actualResponse);
  }

  [Fact(DisplayName = "Rejects missing shopping list")]
  public async Task Handle_RejectsMissingShoppingList() {
    var query = new Query(Guid.CreateVersion7(), Guid.CreateVersion7());
    _repository.GetWithPricesAsync(
      query.UserUid, query.ShoppingListId, Arg.Any<CancellationToken>()
    ).Returns((Response?)null);

    async Task action() => await _handler.Handle(
      query, TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<ShoppingListNotFoundException>(action);
  }
}