using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.GetShoppingList;

namespace Metaspesa.Application.UnitTests.Shopping;

public class GetShoppingListHandlerTest {
  private readonly IShoppingRepository _shoppingRepository;

  private readonly Handler handler;

  public GetShoppingListHandlerTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    handler = new Handler(_shoppingRepository);
  }

  [Fact(DisplayName = "Returns shopping list from repository if it exists")]
  public async Task Handler_ReturnsShoppingListFromRepository_IfItExists() {
    // Arrange
    var userUid = Guid.NewGuid();
    var expectedShoppingList = new ShoppingList("Test List", []);
    _shoppingRepository
      .GetShoppingListAsync(userUid, "Test List", TestContext.Current.CancellationToken)
      .Returns(expectedShoppingList);

    // Act
    Result<ShoppingList> result = await handler.Handle(
      new Query(userUid, "Test List"), TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(expectedShoppingList, result.Value);
    await _shoppingRepository.Received(1).GetShoppingListAsync(
      userUid, "Test List", TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Returns empty shopping list if repository returns null")]
  public async Task Handler_ReturnsEmptyShoppingList_IfRepositoryReturnsNull() {
    // Arrange
    var userUid = Guid.NewGuid();
    _shoppingRepository
      .GetShoppingListAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns((ShoppingList?)null);

    // Act
    Result<ShoppingList> result = await handler.Handle(
      new Query(userUid, null), TestContext.Current.CancellationToken);

    // Assert
    Assert.Empty(result.Value.Items);
  }

  [Fact(DisplayName = "Returns temporary shopping list if repository returns null")]
  public async Task Handler_ReturnsTemporaryShoppingList_IfRepositoryReturnsNull() {
    // Arrange
    var userUid = Guid.NewGuid();
    _shoppingRepository
      .GetShoppingListAsync(userUid, null, TestContext.Current.CancellationToken)
      .Returns((ShoppingList?)null);

    // Act
    Result<ShoppingList> result = await handler.Handle(
      new Query(userUid, null), TestContext.Current.CancellationToken);

    // Assert
    Assert.True(result.Value.IsTemporary());
  }
}