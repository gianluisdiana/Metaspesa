using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Application.Shopping;

public static class UpdateShoppingList {
  public record Command(Guid UserUid, int ShoppingListId, string? NewName);

  public class Handler(
    IShoppingListRepository shoppingListRepository,
    IUnitOfWork unitOfWork
  ) {
    public async Task Handle(
      Command command, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(command);

      var ownerId = new UserId(command.UserUid);
      var newName = new ShoppingListName(command.NewName ?? string.Empty);
      ShoppingList shoppingList = await shoppingListRepository.GetAsync(
        ownerId, new ShoppingListId(command.ShoppingListId),
        cancellationToken) ?? throw new ShoppingListNotFoundException();

      if (newName != shoppingList.Name &&
        await shoppingListRepository.ExistsAsync(ownerId, newName, cancellationToken)) {
        throw new ShoppingListAlreadyExistsException();
      }

      shoppingList.Rename(newName);
      await shoppingListRepository.UpdateAsync(shoppingList, cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);
    }
  }
}