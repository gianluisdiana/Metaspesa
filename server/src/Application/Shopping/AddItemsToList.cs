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
  public record CommandItem(Guid ProductFormatUid, int Amount, bool IsChecked);
  public record Command(
    Guid UserUid,
    Guid ShoppingListId,
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

      ShoppingList shoppingList = await shoppingListRepository.GetAsync(
        new UserId(command.UserUid), new ShoppingListId(command.ShoppingListId),
        cancellationToken) ?? throw new ShoppingListNotFoundException();

      var items = command.Items.Select(item => new ShoppingItem(
        new ProductFormatId(item.ProductFormatUid),
        new PositiveAmount(item.Amount),
        item.IsChecked)).ToList();
      IReadOnlyCollection<Guid> formatIds = [
        .. items.Select(item => item.ProductFormatId.Value).Distinct()
      ];
      IReadOnlyDictionary<Guid, MarketProduct> products =
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