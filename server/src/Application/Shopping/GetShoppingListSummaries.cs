using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class GetShoppingListSummaries {
  public record Query(Guid UserUid) : IQuery<List<ShoppingList>>;

  internal class Handler(
    IShoppingRepository shoppingRepository
  ) : IQueryHandler<Query, List<ShoppingList>> {
    public async Task<Result<List<ShoppingList>>> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      List<ShoppingList> summaries =
        await shoppingRepository.GetShoppingListSummariesAsync(
          query.UserUid, cancellationToken);

      return summaries;
    }
  }
}
