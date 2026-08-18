using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class RecordShoppingList {
  public record Command(Guid UserUid, string? ShoppingListName) : ICommand;

  public class Handler(
    IShoppingPurchaseRepository shoppingPurchaseRepository,
    IUnitOfWork unitOfWork
  ) : ICommandHandler<Command> {
    public async Task<Result> Handle(
      Command command, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(command);

      UserId ownerId = ShoppingListRequest.Owner(command.UserUid);
      ShoppingListName? name = ShoppingListRequest.Name(command.ShoppingListName);
      ShoppingList? shoppingList = await shoppingPurchaseRepository.GetAsync(
        ownerId, name, cancellationToken);

      if (shoppingList is null) {
        return new DomainError(
          "ShoppingList.NotFound",
          "Shopping list was not found.",
          ErrorKind.Missing);
      }

      if (shoppingList.CheckedItems().Count == 0) {
        return new DomainError(
          "ShoppingList.MissingCheckedItems",
          "Shopping list must contain at least one checked item.",
          ErrorKind.Validation);
      }

      shoppingPurchaseRepository.Record(ownerId, shoppingList);
      shoppingPurchaseRepository.Reset(ownerId, name);
      await unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success();
    }
  }
}
