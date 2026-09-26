using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class GetShoppingListSummaries {
  public record Query(Guid UserUid);
  public record Response(string? Name, Guid Id, bool IsTemporary);

  public class Handler(IShoppingListRepository shoppingListRepository) {
    public async Task<IReadOnlyCollection<Response>> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(query);

      var ownerId = new UserId(query.UserUid);
      IReadOnlyCollection<ShoppingList> lists =
        await shoppingListRepository.GetByOwnerAsync(ownerId, cancellationToken);

      return [.. lists.Select(list => new Response(
        list.Name?.Value, list.Id.Value, list.IsTemporary))];
    }
  }
}