using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.GetCurrentShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class GetCurrentShoppingListHandlerTest {
  private readonly IShoppingRepository _shoppingRepository;

  private readonly Handler handler;

  public GetCurrentShoppingListHandlerTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    handler = new Handler(_shoppingRepository);
  }

  [Fact(DisplayName = "Returns shopping list from repository if it exists")]
  public async Task Handler_ReturnsShoppingListFromRepository_IfItExists() {
    // Arrange
    var userUid = Guid.NewGuid();
    var expectedShoppingList = new ShoppingList("Test List", []);
    _shoppingRepository
      .GetCurrentShoppingListAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(expectedShoppingList);

    // Act
    Result<ShoppingList> result = await handler.Handle(
      new Query(userUid), TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(expectedShoppingList, result.Value);
  }

  [Fact(DisplayName = "Returns empty shopping list if repository returns null")]
  public async Task Handler_ReturnsEmptyShoppingList_IfRepositoryReturnsNull() {
    // Arrange
    var userUid = Guid.NewGuid();
    _shoppingRepository
      .GetCurrentShoppingListAsync(userUid, TestContext.Current.CancellationToken)
      .Returns((ShoppingList?)null);

    // Act
    Result<ShoppingList> result = await handler.Handle(
      new Query(userUid), TestContext.Current.CancellationToken);

    // Assert
    Assert.Empty(result.Value.Items);
  }
}