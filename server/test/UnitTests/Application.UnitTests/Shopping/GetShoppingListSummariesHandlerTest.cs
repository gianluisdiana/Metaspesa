using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using NSubstitute;
using static Metaspesa.Application.Shopping.GetShoppingListSummaries;

namespace Metaspesa.Application.UnitTests.Shopping;

public class GetShoppingListSummariesHandlerTest {
  private readonly IShoppingRepository _shoppingRepository;

  private readonly Handler handler;

  public GetShoppingListSummariesHandlerTest() {
    _shoppingRepository = Substitute.For<IShoppingRepository>();
    handler = new Handler(_shoppingRepository);
  }

  [Fact(DisplayName = "Returns shopping list summaries from repository")]
  public async Task Handler_ReturnsShoppingListSummaries_FromRepository() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<ShoppingList> expectedSummaries = [
      new("Groceries", []),
      new(null, []),
    ];
    _shoppingRepository
      .GetShoppingListSummariesAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(expectedSummaries);

    // Act
    Result<List<ShoppingList>> result = await handler.Handle(
      new Query(userUid), TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(expectedSummaries, result.Value);
  }

  [Fact(DisplayName = "Returns summaries without shopping list items")]
  public async Task Handler_ReturnsSummaries_WithoutShoppingListItems() {
    // Arrange
    var userUid = Guid.NewGuid();
    List<ShoppingList> expectedSummaries = [
      new("Groceries", []),
      new(null, []),
    ];
    _shoppingRepository
      .GetShoppingListSummariesAsync(userUid, TestContext.Current.CancellationToken)
      .Returns(expectedSummaries);

    // Act
    Result<List<ShoppingList>> result = await handler.Handle(
      new Query(userUid), TestContext.Current.CancellationToken);

    // Assert
    Assert.True(result.Value.All(shoppingList => shoppingList.Items.Count == 0));
  }

  [Fact(DisplayName = "Returns empty summaries when repository returns none")]
  public async Task Handler_ReturnsEmptySummaries_WhenRepositoryReturnsNone() {
    // Arrange
    var userUid = Guid.NewGuid();
    _shoppingRepository
      .GetShoppingListSummariesAsync(userUid, TestContext.Current.CancellationToken)
      .Returns([]);

    // Act
    Result<List<ShoppingList>> result = await handler.Handle(
      new Query(userUid), TestContext.Current.CancellationToken);

    // Assert
    Assert.Empty(result.Value);
  }
}