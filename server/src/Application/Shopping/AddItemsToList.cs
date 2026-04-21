using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Extensions;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class AddItemsToList {
  public record CommandItem(
    string? Name,
    string? Quantity,
    float Price,
    bool IsChecked
  );

  public record Command(
    Guid UserUid,
    string? ShoppingListName,
    IReadOnlyCollection<CommandItem> Items
  ) : ICommand {
    internal IReadOnlyCollection<ShoppingItem> ToShoppingItems() =>
      Items.Select(i => new ShoppingItem(
        i.Name!,
        i.Quantity,
        new Price(i.Price),
        i.IsChecked
      )).ToList();
  }

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

      shoppingRepository.AddItemsToList(
        command.UserUid, command.ShoppingListName, command.ToShoppingItems());
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
        .WithErrorCode("ShoppingList.NotFound");

      RuleFor(x => x.Items)
        .NotEmpty()
        .WithMessage("At least one item must be provided.")
        .WithErrorCode("ShoppingList.Items.Empty");

      RuleForEach(x => x.Items)
        .ChildRules(item => {
          item.RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Item name must not be empty.")
            .WithErrorCode("ShoppingList.Items.Name.Empty");

          item.RuleFor(x => x.Price)
            .Must(PricePolicy.IsValidPrice)
            .WithMessage("Item price must be greater than or equal to 0.")
            .WithErrorCode("ShoppingList.Items.Price.Negative");

          item.RuleFor(x => x.Quantity)
            .MaximumLength(50)
            .WithMessage("Item quantity must not exceed 50 characters.")
            .WithErrorCode("ShoppingList.Items.Quantity.TooLong");
        });
    }
  }
}
