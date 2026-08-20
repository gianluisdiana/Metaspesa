using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class RemoveItem {
  public record Command(Guid UserUid, string? ShoppingListName, int ProductFormatUid);

  public class Handler(
    IShoppingListRepository shoppingListRepository,
    IUnitOfWork unitOfWork
  ) {
    public async Task Handle(
      Command command, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(command);

      UserId ownerId = ShoppingListRequest.Owner(command.UserUid);
      ShoppingListName? name = ShoppingListRequest.Name(command.ShoppingListName);
      ShoppingList shoppingList = await shoppingListRepository.GetAsync(
        ownerId, name, cancellationToken) ??
        throw ShoppingListRequest.NotFound();

      shoppingList.RemoveItem(new ProductFormatId(command.ProductFormatUid));
      await shoppingListRepository.UpdateAsync(shoppingList, cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);
    }
  }
}