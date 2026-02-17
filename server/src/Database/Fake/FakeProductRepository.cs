using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Database.Fake;

internal class FakeProductRepository : IProductRepository
{
  public async Task<List<RegisteredItem>> GetRegisteredItemsAsync(
    Guid userUid, CancellationToken cancellationToken
  )
  {
    return [];
  }

  public void RegisterItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems
  )
  {
  }

  public void UpdateItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems
  )
  {
  }
}
