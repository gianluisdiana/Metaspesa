using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Application.Shopping;

public static class CreateShoppingList {
  public record Command(Guid UserUid, string? ShoppingListName);

  public class Handler(
    IShoppingListRepository shoppingListRepository
  ) {
    public async Task<int> Handle(
      Command command, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(command);

      var ownerId = new UserId(command.UserUid);
      ShoppingListName? name = string.IsNullOrWhiteSpace(command.ShoppingListName)
        ? null : new ShoppingListName(command.ShoppingListName);

      if (await shoppingListRepository.ExistsAsync(ownerId, name, cancellationToken)) {
        if (name is null) {
          throw new TemporaryShoppingListAlreadyExistsException();
        }
        throw new ShoppingListAlreadyExistsException();
      }

      return await shoppingListRepository.AddAsync(
        ShoppingList.Create(ownerId, name), cancellationToken);
    }
  }
}