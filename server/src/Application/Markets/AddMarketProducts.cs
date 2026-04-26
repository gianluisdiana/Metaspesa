using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Extensions;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Markets;

public static class AddMarketProducts {
  public record CommandProduct(
    string? Name,
    float Price,
    string? Quantity,
    string? MarketName,
    string? BrandName
  );

  public record Command(
    IReadOnlyCollection<CommandProduct> Products,
    DateOnly? RegisteredAt
  ) : ICommand {
    internal List<Market> ToMarkets() => [.. Products.GroupBy(p => p.MarketName!)
      .Select(g => new Market(g.Key, [..
        g.Select(p => new MarketProduct(
          p.Name!,
          p.Quantity!,
          new Price(p.Price),
          new ProductBrand(p.BrandName!)
        ))])
      )
    ];
  }

  internal class Handler(
    IValidator<Command> validator,
    IMarketRepository marketRepository
  ) : ICommandHandler<Command> {
    public async Task<Result> Handle(
      Command command, CancellationToken cancellationToken = default
    ) {
      ValidationResult validationResult = await validator.ValidateAsync(command, cancellationToken);
      if (!validationResult.IsValid) {
        return validationResult.ToDomainErrors();
      }

      List<Market> markets = command.ToMarkets();

      List<Market> existingMarkets = await marketRepository.GetMarketsAsync(
        cancellationToken);
      var newMarkets = markets.Where(m =>
        !existingMarkets.Any(em => em.Name.Equals(m.Name, StringComparison.OrdinalIgnoreCase)))
        .ToList();

      if (newMarkets.Count != 0) {
        await marketRepository.AddMarketsAsync(newMarkets, cancellationToken);
      }

      var brands = markets.SelectMany(m => m.Products)
        .Select(p => p.Brand)
        .DistinctBy(b => b.Name)
        .ToList();

      List<ProductBrand> existingBrands = await marketRepository.GetBrandsAsync(
        cancellationToken);
      var newBrands = brands.Where(b =>
        !existingBrands.Any(eb => eb.Name.Equals(b.Name, StringComparison.OrdinalIgnoreCase)))
        .ToList();

      if (newBrands.Count != 0) {
        await marketRepository.AddBrandsAsync(newBrands, cancellationToken);
      }

      DateOnly date = command.RegisteredAt ?? DateOnly.FromDateTime(DateTime.UtcNow);
      foreach (Market market in markets) {
        await marketRepository.AddMarketProductsAsync(market, date, cancellationToken);
      }

      return Result.Success();
    }
  }

  internal class Validator : AbstractValidator<Command> {
    public Validator(IMarketRepository marketRepository) {
      RuleFor(x => x.Products)
        .NotEmpty()
        .WithMessage("At least one product must be provided.")
        .WithErrorCode("Market.Products.Empty");

      RuleForEach(x => x.Products)
        .ChildRules(product => {
          product.RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name must not be empty.")
            .WithErrorCode("Market.Product.Name.Empty");

          product.RuleFor(x => x.MarketName)
            .NotEmpty()
            .WithMessage("Market name must not be empty.")
            .WithErrorCode("Market.Product.MarketName.Empty");

          product.RuleFor(x => x.BrandName)
            .NotEmpty()
            .WithMessage("Brand name must not be empty.")
            .WithErrorCode("Market.Product.BrandName.Empty");

          product.RuleFor(x => x.Quantity)
            .NotEmpty()
            .WithMessage("Product quantity must not be empty.")
            .WithErrorCode("Market.Product.Quantity.Empty");

          product.RuleFor(x => x.Price)
            .Must(PricePolicy.IsValidPrice)
            .WithMessage("Product price must be greater than or equal to 0.")
            .WithErrorCode("Market.Product.Price.Negative");
        });

      RuleForEach(x => x.Products)
        .Must((command, product) => !command.Products.Any(p =>
          p != product &&
          p.Name == product.Name &&
          p.MarketName == product.MarketName &&
          p.BrandName == product.BrandName
        ))
        .WithMessage("Each product must be unique in name, market and brand combination.")
        .WithErrorCode("Market.Product.Duplicate");
    }
  }
}
