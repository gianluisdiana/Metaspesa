using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class GetShoppingList {
  public record Query(Guid UserUid, string? ShoppingListName) : IQuery<ShoppingList>;

  internal class Handler(
    IShoppingRepository shoppingRepository
  ) : IQueryHandler<Query, ShoppingList> {
    public async Task<Result<ShoppingList>> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      ShoppingList? shoppingList = await shoppingRepository.GetShoppingListAsync(
        query.UserUid, query.ShoppingListName, cancellationToken);

      return shoppingList ?? new ShoppingList(null, []);
    }
  }
}
