using System.Diagnostics;
using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Extensions;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class RecordShoppingList {
  public record CommandItem(
    string? Name,
    string? Quantity,
    float Price,
    bool IsChecked
  );
  public record Command(
    Guid UserUid,
    string? ShoppingListName,
    IReadOnlyCollection<CommandItem> ShoppingListItems
  ) : ICommand {
    internal ShoppingList CreateShoppingList() {
      Debug.Assert(ShoppingListItems.All(i => i.Name is not null));
      return new ShoppingList(
        ShoppingListName,
        ShoppingListItems.Select(item => new ShoppingItem(
          item.Name!,
          item.Quantity,
          new Price(item.Price),
          item.IsChecked
        )).ToList()
      );
    }
  }

  internal class Handler(
    IValidator<Command> validator,
    IProductRepository productRepository,
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

      ShoppingList shoppingList = command.CreateShoppingList();
      Guid userUid = command.UserUid;

      List<Product> registeredItems = await productRepository
        .GetRegisteredItemsAsync(userUid, cancellationToken);

      ShoppingList priceChanged = shoppingList.OnlyWithPriceChangedItems(
        registeredItems);

      if (priceChanged.Count != 0) {
        productRepository.UpdateRegisteredItems(userUid, priceChanged.Items);
      }

      ShoppingList newItems = shoppingList.Without(registeredItems);
      if (newItems.Count != 0) {
        productRepository.RegisterItems(userUid, newItems.Items);
      }

      ShoppingList checkedItems = shoppingList.OnlyWithCheckedItems();
      shoppingRepository.RecordShoppingList(userUid, checkedItems);

      await unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success();
    }
  }

  internal class Validator : AbstractValidator<Command> {
    public Validator(IShoppingRepository shoppingRepository) {
      RuleFor(x => x.ShoppingListItems)
        .Must(x => x.Any(i => i.IsChecked))
        .WithMessage("Shopping list must contain at least one checked item.")
        .WithErrorCode("ShoppingList.MissingCheckedItems");

      RuleFor(x => x)
        .MustAsync(async (command, ct) =>
          await shoppingRepository.CheckShoppingListExistAsync(
            command.UserUid, command.ShoppingListName, ct))
        .WithName(nameof(Command.ShoppingListName))
        .WithMessage(command => string.IsNullOrWhiteSpace(command.ShoppingListName)
          ? $"User {command.UserUid} doesn't have a temporary shopping list."
          : $"User {command.UserUid} doesn't have a shopping list named '{command.ShoppingListName}'.")
        .WithErrorCode("ShoppingList.NotFound");

      RuleForEach(x => x.ShoppingListItems)
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