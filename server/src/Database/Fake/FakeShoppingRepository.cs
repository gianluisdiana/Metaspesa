using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Database.Fake;

internal class FakeShoppingRepository : IShoppingRepository
{
  public async Task<ShoppingList?> GetCurrentShoppingListAsync(
    Guid userUid, CancellationToken cancellationToken
  )
  {
    return null;
  }

  public void RecordShoppingList(
    Guid userUid, ShoppingList shoppingList
  )
  {
  }
}