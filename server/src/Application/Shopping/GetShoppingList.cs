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
    string ProductName, string BrandName, MarketSummary? Market,
    int Amount, MarketProductFormat Format, bool IsChecked);
  public record Response(
    string? ShoppingListName,
    IReadOnlyCollection<ResponseItem> Items,
    int Id,
    bool IsTemporary
  );
  public record Query(Guid UserUid, int ShoppingListId);

  public class Handler(
    IShoppingListRepository shoppingListRepository,
    MarketProductRepository productRepository
  ) {
    public async Task<Response> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(query);

      ShoppingList shoppingList = await shoppingListRepository.GetAsync(
        new UserId(query.UserUid), new ShoppingListId(query.ShoppingListId),
        cancellationToken) ?? throw new ShoppingListNotFoundException();

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
          product.BrandName,
          product.Market,
          item.Amount.Value,
          product.Formats.Single(),
          item.IsChecked));
      }

      return new Response(shoppingList.Name?.Value, items,
        shoppingList.Id!.Value.Value, shoppingList.IsTemporary);
    }
  }
}