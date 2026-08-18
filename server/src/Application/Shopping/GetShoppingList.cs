using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using MarketProductRepository = Metaspesa.Application.Abstractions.Markets.IProductRepository;

namespace Metaspesa.Application.Shopping;

public static class GetShoppingList {
  public record ResponseItem(
    string ProductName, int Amount, MarketProductFormat Format, bool IsChecked);
  public record Response(
    string? ShoppingListName,
    IReadOnlyCollection<ResponseItem> Items
  );
  public record Query(Guid UserUid, string? ShoppingListName) : IQuery<Response>;

  internal class Handler(
    IShoppingRepository shoppingRepository,
    MarketProductRepository productRepository
  ) : IQueryHandler<Query, Response> {
    public async Task<Result<Response>> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      AShoppingList? shoppingList = await shoppingRepository.GetShoppingListAsync(
        query.UserUid, query.ShoppingListName, cancellationToken);

      if (shoppingList is null) {
        return new DomainError(
          "ShoppingList.NotFound",
          string.IsNullOrWhiteSpace(query.ShoppingListName)
            ? $"User {query.UserUid} doesn't have a temporary shopping list."
            : $"User {query.UserUid} doesn't have a shopping list named '{query.ShoppingListName}'.",
          ErrorKind.Missing);
      }

      IReadOnlyCollection<int> referencesId = [
        ..shoppingList.Items.Select(i => i.ReferenceUid)
      ];

      IReadOnlyDictionary<int, MarketProduct> marketProducts = await productRepository
        .GetProductsAsync(referencesId, cancellationToken);

      var items = shoppingList.Items.Select(i => new ResponseItem(
         ProductName: marketProducts[i.ReferenceUid].Name,
         Amount: i.Amount,
         Format: marketProducts[i.ReferenceUid].Formats.First(),
         IsChecked: i.IsChecked
       ))
       .ToList();

      return new Response(shoppingList.Name, items);
    }
  }
}
