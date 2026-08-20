using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Application.Shopping;

public static class UpdateShoppingList {
  public record Command(Guid UserUid, string? ShoppingListName, string? NewName);

  public class Handler(
    IShoppingListRepository shoppingListRepository,
    IUnitOfWork unitOfWork
  ) {
    public async Task Handle(
      Command command, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(command);

      UserId ownerId = ShoppingListRequest.Owner(command.UserUid);
      ShoppingListName? currentName = ShoppingListRequest.Name(command.ShoppingListName);
      var newName = new ShoppingListName(command.NewName ?? string.Empty);
      ShoppingList shoppingList = await shoppingListRepository.GetAsync(
        ownerId, currentName, cancellationToken) ??
        throw ShoppingListRequest.NotFound();

      if (newName != currentName &&
        await shoppingListRepository.ExistsAsync(ownerId, newName, cancellationToken)) {
        throw new ShoppingListAlreadyExistsException();
      }

      shoppingList.Rename(newName);
      await shoppingListRepository.UpdateAsync(shoppingList, cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);
    }
  }
}