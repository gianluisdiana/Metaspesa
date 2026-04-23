using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Extensions;

namespace Metaspesa.Application.Shopping;

public static class RemoveItem {
  public record Command(
    Guid UserUid,
    string? ShoppingListName,
    string ItemName
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

      shoppingRepository.RemoveItem(
        command.UserUid, command.ShoppingListName, command.ItemName);
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
        .MustAsync(async (command, ct) =>
          await shoppingRepository.CheckItemExistsAsync(
            command.UserUid, command.ShoppingListName, command.ItemName, ct))
        .WithName(nameof(Command.ItemName))
        .WithMessage(command =>
          $"Item '{command.ItemName}' not found in the shopping list.")
        .WithErrorCode("ShoppingList.Item.NotFound")
        .WithState(_ => ErrorKind.Missing);
    }
  }
}
