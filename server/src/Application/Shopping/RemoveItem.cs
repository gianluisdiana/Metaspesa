using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Application.Shopping;

public static class RemoveItem {
  public record Command(Guid UserUid, int ShoppingListId, int ProductFormatUid);

  public class Handler(
    IShoppingListRepository shoppingListRepository,
    IUnitOfWork unitOfWork
  ) {
    public async Task Handle(
      Command command, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(command);

      ShoppingList shoppingList = await shoppingListRepository.GetAsync(
        new UserId(command.UserUid), new ShoppingListId(command.ShoppingListId),
        cancellationToken) ?? throw new ShoppingListNotFoundException();

      shoppingList.RemoveItem(new ProductFormatId(command.ProductFormatUid));
      await shoppingListRepository.UpdateAsync(shoppingList, cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);
    }
  }
}