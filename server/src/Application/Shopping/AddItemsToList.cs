using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using MarketProductRepository = Metaspesa.Application.Abstractions.Markets.IProductRepository;

namespace Metaspesa.Application.Shopping;

public static class AddItemsToList {
  public record CommandItem(int ProductFormatUid, int Amount, bool IsChecked);
  public record Command(
    Guid UserUid,
    string? ShoppingListName,
    IReadOnlyCollection<CommandItem> Items
  );

  public class Handler(
    IShoppingListRepository shoppingListRepository,
    MarketProductRepository productRepository,
    IUnitOfWork unitOfWork
  ) {
    public async Task Handle(
      Command command, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(command);

      if (command.Items.Count == 0) {
        throw new EmptyShoppingItemsException();
      }

      UserId ownerId = ShoppingListRequest.Owner(command.UserUid);
      ShoppingListName? name = ShoppingListRequest.Name(command.ShoppingListName);
      ShoppingList shoppingList = await shoppingListRepository.GetAsync(
        ownerId, name, cancellationToken) ??
        throw ShoppingListRequest.NotFound();

      var items = command.Items.Select(item => new ShoppingItem(
        new ProductFormatId(item.ProductFormatUid),
        new PositiveAmount(item.Amount),
        item.IsChecked)).ToList();
      IReadOnlyCollection<int> formatIds = [
        .. items.Select(item => item.ProductFormatId.Value).Distinct()
      ];
      IReadOnlyDictionary<int, MarketProduct> products =
        await productRepository.GetProductsAsync(formatIds, cancellationToken);

      ProductFormatId? missingFormat = items
        .Where(item => !products.ContainsKey(item.ProductFormatId.Value))
        .Select(item => (ProductFormatId?)item.ProductFormatId)
        .FirstOrDefault();
      if (missingFormat.HasValue) {
        throw new ShoppingProductFormatNotFoundException(missingFormat.Value);
      }

      shoppingList.AddItems(items);
      await shoppingListRepository.UpdateAsync(shoppingList, cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);
    }
  }
}