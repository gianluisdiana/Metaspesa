using Metaspesa.Application.Abstractions.Shopping;
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

      Guid ownerId = command.UserUid;
      string? name = command.ShoppingListName;

      if (await shoppingListRepository.ExistsAsync(ownerId, name, cancellationToken)) {
        throw new ShoppingListAlreadyExistsException(ownerId, name);
      }

      var shoppingList = ShoppingList.Create(ownerId, name);
      return await shoppingListRepository.AddAsync(shoppingList, cancellationToken);
    }
  }
}