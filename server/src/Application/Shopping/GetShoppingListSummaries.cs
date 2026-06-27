using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class GetShoppingListSummaries {
  public record Query(Guid UserUid) : IQuery<List<AShoppingList>>;

  internal class Handler(
    IShoppingRepository shoppingRepository
  ) : IQueryHandler<Query, List<AShoppingList>> {
    public async Task<Result<List<AShoppingList>>> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      List<AShoppingList> summaries =
        await shoppingRepository.GetShoppingListSummariesAsync(
          query.UserUid, cancellationToken);

      return summaries;
    }
  }
}