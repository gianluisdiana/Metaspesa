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
    string OriginalItemName,
    string? NewName,
    string? Quantity,
    float? Price,
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

      ShoppingItem? currentItem = await shoppingRepository.GetItemAsync(
        command.UserUid,
        command.ShoppingListName,
        command.OriginalItemName,
        cancellationToken);

      if (currentItem is null) {
        return new DomainError(
          "ShoppingList.Item.NotFound",
          $"Item '{command.OriginalItemName}' not found.",
          ErrorKind.Missing);
      }

      ShoppingItem updated = new(
        Name: !string.IsNullOrWhiteSpace(command.NewName)
          ? command.NewName
          : currentItem.Name,
        Quantity: !string.IsNullOrWhiteSpace(command.Quantity)
          ? command.Quantity
          : currentItem.Quantity,
        Price: command.Price.HasValue
          ? new Price(command.Price.Value)
          : currentItem.Price,
        IsChecked: command.IsChecked ?? currentItem.IsChecked
      );

      shoppingRepository.UpdateItem(
        command.UserUid, command.ShoppingListName,
        command.OriginalItemName, updated);
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
          !await shoppingRepository.CheckItemExistsAsync(
            command.UserUid, command.ShoppingListName, command.NewName!, ct))
        .When(x => x.NewName != null &&
          !x.NewName.Equals(x.OriginalItemName, StringComparison.OrdinalIgnoreCase))
        .WithName(nameof(Command.NewName))
        .WithMessage(command =>
          $"Item '{command.NewName}' already exists in the shopping list.")
        .WithErrorCode("ShoppingList.Item.AlreadyExists")
        .WithState(_ => ErrorKind.Conflict);

      RuleFor(x => x.Price)
        .Must(price => PricePolicy.IsValidPrice(price!.Value))
        .When(x => x.Price.HasValue)
        .WithMessage("Item price must be greater than or equal to 0.")
        .WithErrorCode("ShoppingList.Items.Price.Negative");

      RuleFor(x => x.Quantity)
        .MaximumLength(50)
        .When(x => x.Quantity != null)
        .WithMessage("Item quantity must not exceed 50 characters.")
        .WithErrorCode("ShoppingList.Items.Quantity.TooLong");

      RuleFor(x => x)
        .Must(command =>
          !string.IsNullOrWhiteSpace(command.NewName) ||
          !string.IsNullOrWhiteSpace(command.Quantity) ||
          command.Price.HasValue ||
          command.IsChecked.HasValue)
        .WithMessage("At least one field must be provided to update the item.")
        .WithErrorCode("ShoppingList.Item.NoFieldsToUpdate");
    }
  }
}
