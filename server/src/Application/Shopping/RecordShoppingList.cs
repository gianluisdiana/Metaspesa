using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class RecordShoppingList {
  public record Command(
    Guid UserUid,
    string? ShoppingListName
  ) : ICommand;

  internal class Handler(
    IShoppingRepository shoppingRepository,
    IUnitOfWork unitOfWork
  ) : ICommandHandler<Command> {
    public async Task<Result> Handle(
      Command command, CancellationToken cancellationToken = default
    ) {
      Guid userUid = command.UserUid;
      AShoppingList? shoppingList = await shoppingRepository
        .GetShoppingListAsync(userUid, command.ShoppingListName, cancellationToken);

      if (shoppingList is null) {
        return new DomainError(
          Code: "ShoppingList.NotFound",
          Description: string.IsNullOrWhiteSpace(command.ShoppingListName)
            ? $"User {userUid} doesn't have a temporary shopping list."
            : $"User {userUid} doesn't have a shopping list named '{command.ShoppingListName}'.",
          Kind: ErrorKind.Missing);
      }

      if (!shoppingList.HasCheckedItems()) {
        return new DomainError(
          Code: "ShoppingList.MissingCheckedItems",
          Description: "Shopping list must contain at least one checked item.",
          Kind: ErrorKind.Validation);
      }

      AShoppingList checkedList = shoppingList.OnlyWithCheckedItems();
      shoppingRepository.RecordShoppingList(userUid, checkedList);
      shoppingRepository.ResetShoppingList(userUid, command.ShoppingListName);

      await unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success();
    }
  }
}