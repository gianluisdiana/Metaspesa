using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Extensions;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class RecordShoppingList {
  public record Command(Guid UserUid, ShoppingList ShoppingList) : ICommand;

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

      ShoppingList shoppingList = command.ShoppingList;
      Guid userUid = command.UserUid;

      List<RegisteredItem> registeredItems = await productRepository
        .GetRegisteredItemsAsync(userUid, cancellationToken);

      ShoppingList registeredShoppingList = shoppingList.Intersecting(registeredItems);

      if (registeredShoppingList.Any()) {
        productRepository.UpdateItems(userUid, registeredShoppingList.Items);
      }

      ShoppingList newShoppingList = shoppingList.Without(registeredItems);

      if (newShoppingList.Any()) {
        productRepository.RegisterItems(userUid, newShoppingList.Items);
      }

      if (shoppingList.IsNamed) {
        shoppingRepository.RecordShoppingList(userUid, shoppingList);
      }

      await unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success();
    }
  }

  internal class Validator : AbstractValidator<Command> {
    public Validator() {
      RuleFor(x => x.ShoppingList.Items)
        .NotEmpty()
        .WithMessage("Shopping list must contain at least one item.")
        .WithErrorCode("ShoppingList.Items.Empty");

      RuleForEach(x => x.ShoppingList.Items)
        .ChildRules(item => {
          item.RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Item name must not be empty.")
            .WithErrorCode("ShoppingList.Items.Name.Empty");

          item.RuleFor(x => x.Quantity)
            .MaximumLength(50)
            .WithMessage("Item quantity must not exceed 50 characters.")
            .WithErrorCode("ShoppingList.Items.Quantity.TooLong");

          item.RuleFor(x => x.Price)
            .Must(price => price is null || price >= 0)
            .WithMessage("Item price must be a non-negative number.")
            .WithErrorCode("ShoppingList.Items.Price.Negative");
        });

    }
  }
}
