using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class GetShoppingListSummaries {
  public record Query(Guid UserUid);
  public record Response(string? Name);

  public class Handler(IShoppingListRepository shoppingListRepository) {
    public async Task<IReadOnlyCollection<Response>> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(query);

      UserId ownerId = ShoppingListRequest.Owner(query.UserUid);
      IReadOnlyCollection<ShoppingList> lists =
        await shoppingListRepository.GetByOwnerAsync(ownerId, cancellationToken);

      return [.. lists.Select(list => new Response(list.Name?.Value))];
    }
  }
}