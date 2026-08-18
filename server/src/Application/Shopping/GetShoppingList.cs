using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using MarketProductRepository = Metaspesa.Application.Abstractions.Markets.IProductRepository;

namespace Metaspesa.Application.Shopping;

public static class GetShoppingList {
  public record ResponseItem(
    string ProductName, int Amount, MarketProductFormat Format, bool IsChecked);
  public record Response(
    string? ShoppingListName,
    IReadOnlyCollection<ResponseItem> Items
  );
  public record Query(Guid UserUid, string? ShoppingListName);

  public class Handler(
    IShoppingListRepository shoppingListRepository,
    MarketProductRepository productRepository
  ) {
    public async Task<Response> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(query);

      UserId ownerId = ShoppingListRequest.Owner(query.UserUid);
      ShoppingListName? name = ShoppingListRequest.Name(query.ShoppingListName);
      ShoppingList shoppingList = await shoppingListRepository.GetAsync(
        ownerId, name, cancellationToken) ??
        throw ShoppingListRequest.NotFound();

      IReadOnlyCollection<int> formatIds = [
        .. shoppingList.Items.Select(item => item.ProductFormatId.Value)
      ];
      IReadOnlyDictionary<int, MarketProduct> products =
        await productRepository.GetProductsAsync(formatIds, cancellationToken);

      List<ResponseItem> items = [];
      foreach (ShoppingItem item in shoppingList.Items) {
        if (!products.TryGetValue(item.ProductFormatId.Value, out MarketProduct? product)) {
          throw new ShoppingProductFormatNotFoundException(item.ProductFormatId);
        }

        items.Add(new ResponseItem(
          product.Name,
          item.Amount.Value,
          product.Formats.First(),
          item.IsChecked));
      }

      return new Response(shoppingList.Name?.Value, items);
    }
  }
}
