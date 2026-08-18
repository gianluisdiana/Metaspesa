using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class UpdateItem {
  public record Command(
    Guid UserUid,
    string? ShoppingListName,
    int ProductReferenceUid,
    int? Amount,
    bool? IsChecked
  );

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

      shoppingList.UpdateItem(
        new ProductFormatId(command.ProductReferenceUid),
        command.Amount.HasValue ? new PositiveAmount(command.Amount.Value) : null,
        command.IsChecked);
      await shoppingListRepository.UpdateAsync(shoppingList, cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);
    }
  }
}