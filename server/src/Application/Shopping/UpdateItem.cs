using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Application.Shopping;

public static class UpdateItem {
  public record Command(
    Guid UserUid,
    Guid ShoppingListId,
    Guid ProductFormatUid,
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

      ShoppingList shoppingList = await shoppingListRepository.GetAsync(
        new UserId(command.UserUid), new ShoppingListId(command.ShoppingListId),
        cancellationToken) ?? throw new ShoppingListNotFoundException();

      shoppingList.UpdateItem(
        new ProductFormatId(command.ProductFormatUid),
        command.Amount.HasValue ? new PositiveAmount(command.Amount.Value) : null,
        command.IsChecked);
      await shoppingListRepository.UpdateAsync(shoppingList, cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);
    }
  }
}