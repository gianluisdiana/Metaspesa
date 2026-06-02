using System.Globalization;
using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Extensions;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class AddItemsToList {
  public record CommandItem(
    int ReferenceUid,
    int Amount,
    bool IsChecked
  );

  public record Command(
    Guid UserUid,
    string? ShoppingListName,
    IReadOnlyCollection<CommandItem> Items
  ) : ICommand {
    internal IReadOnlyCollection<AShoppingItem> ToShoppingItems() => [..
      Items.Select(i => new AShoppingItem(
        ReferenceUid: i.ReferenceUid,
        Amount: i.Amount,
        IsChecked: i.IsChecked
      ))
    ];
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
    public Validator(
      IShoppingRepository shoppingRepository,
      IProductRepository productRepository
    ) {
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

      RuleFor(x => x.Items)
        .NotEmpty()
        .WithMessage("At least one item must be provided.")
        .WithErrorCode("ShoppingList.Items.Empty");

      RuleFor(x => x.Items)
        .Must(items => items.Select(i => i.ReferenceUid).Distinct().Count() == items.Count)
        .WithName("ShoppingList.Items[].ReferenceUid")
        .WithMessage("Duplicate product reference UIDs are not allowed.")
        .WithErrorCode("ShoppingList.Items.DuplicateReferenceUid")
        .WithState(_ => ErrorKind.Validation);

      RuleForEach(x => x.Items)
        .ChildRules(item => {
          item.RuleFor(i => i.Amount)
          .GreaterThan(0)
          .WithMessage(i =>
            $"Item with reference UID {i.ReferenceUid} must have an amount greater than zero.")
          .WithErrorCode("ShoppingList.Item.Amount.Invalid")
          .WithState(_ => ErrorKind.Validation);

          item.RuleFor(i => i)
            .MustAsync(async (item, ct) =>
              await productRepository.CheckProductExistsAsync(item.ReferenceUid, ct))
            .WithMessage(i =>
              $"Product reference with UID {i.ReferenceUid} does not exist.")
            .WithErrorCode("ShoppingList.Item.ReferenceUid.NotFound")
            .WithState(_ => ErrorKind.Missing);
        });
    }
  }
}