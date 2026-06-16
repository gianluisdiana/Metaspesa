using System.Globalization;
using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Extensions;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class UpdateItem {
  public record Command(
    Guid UserUid,
    string? ShoppingListName,
    int ProductReferenceUid,
    int? Amount,
    bool? IsChecked
  ) : ICommand;

  internal class Handler(
    IValidator<Command> validator,
    IShoppingRepository shoppingRepository,
    IUnitOfWork unitOfWork
  ) : ICommandHandler<Command> {
    public async Task<Result> Handle(
      Command command, CancellationToken cancellationToken = default
    ) {
      ValidationResult validationResult = await validator.ValidateAsync(
        command, cancellationToken);
      if (!validationResult.IsValid) {
        return validationResult.ToDomainErrors();
      }

      AShoppingItem? currentItem = await shoppingRepository.GetItemAsync(
        command.UserUid,
        command.ShoppingListName,
        command.ProductReferenceUid,
        cancellationToken);

      if (currentItem is null) {
        return new DomainError(
          "ShoppingList.Item.NotFound",
          $"Item with reference UID '{command.ProductReferenceUid}' not found.",
          ErrorKind.Missing);
      }

      AShoppingItem updated = new(
        ReferenceUid: currentItem.ReferenceUid,
        Amount: command.Amount ?? currentItem.Amount,
        IsChecked: command.IsChecked ?? currentItem.IsChecked
      );

      shoppingRepository.UpdateItem(
        command.UserUid, command.ShoppingListName, updated);
      await unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success();
    }
  }

  internal class Validator : AbstractValidator<Command> {
    public Validator(IShoppingRepository shoppingRepository) {
      RuleFor(x => x)
        .MustAsync(async (command, ct) =>
          await shoppingRepository.CheckShoppingListExistAsync(
            command.UserUid, command.ShoppingListName, ct))
        .WithName(nameof(Command.ShoppingListName))
        .WithMessage(command => string.IsNullOrWhiteSpace(command.ShoppingListName)
          ? $"User {command.UserUid} doesn't have a temporary shopping list."
          : $"User {command.UserUid} doesn't have a shopping list named '{command.ShoppingListName}'.")
        .WithErrorCode("ShoppingList.NotFound")
        .WithState(_ => ErrorKind.Missing);

      RuleFor(x => x)
        .Must(command =>
          command.Amount.HasValue ||
          command.IsChecked.HasValue)
        .WithMessage("At least one field must be provided to update the item.")
        .WithErrorCode("ShoppingList.Item.NoFieldsToUpdate");

      RuleFor(x => x.Amount)
        .GreaterThan(0)
        .When(x => x.Amount.HasValue)
        .WithMessage("Amount must be greater than zero.")
        .WithErrorCode("ShoppingList.Item.InvalidAmount");
    }
  }
}