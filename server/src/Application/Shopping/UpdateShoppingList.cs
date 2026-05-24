using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Extensions;

namespace Metaspesa.Application.Shopping;

public static class UpdateShoppingList {
  public record Command(Guid UserUid, string? ShoppingListName, string? NewName) : ICommand;

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

      shoppingRepository.UpdateShoppingListName(
        command.UserUid,
        command.ShoppingListName,
        command.NewName);
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
        .Must(command => !string.IsNullOrWhiteSpace(command.NewName))
        .WithName(nameof(Command.NewName))
        .WithMessage("At least one field must be provided to update the shopping list.")
        .WithErrorCode("ShoppingList.NoFieldsToUpdate");

      RuleFor(x => x)
        .MustAsync(async (command, ct) =>
          !await shoppingRepository.CheckShoppingListExistAsync(
            command.UserUid, command.NewName, ct))
        .When(x => !string.IsNullOrWhiteSpace(x.NewName) &&
          !x.NewName.Equals(x.ShoppingListName, StringComparison.OrdinalIgnoreCase))
        .WithName(nameof(Command.NewName))
        .WithMessage(command =>
          $"User {command.UserUid} already has a shopping list named '{command.NewName}'.")
        .WithErrorCode("ShoppingList.AlreadyExists")
        .WithState(_ => ErrorKind.Conflict);
    }
  }
}