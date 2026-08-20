using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Application.Shopping;

public static class CreateShoppingList {
  public record Command(Guid UserUid, string? ShoppingListName);

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

      if (await shoppingListRepository.ExistsAsync(ownerId, name, cancellationToken)) {
        throw new ShoppingListAlreadyExistsException();
      }

      shoppingListRepository.Add(ShoppingList.Create(ownerId, name));
      await unitOfWork.SaveChangesAsync(cancellationToken);
    }
  }
}