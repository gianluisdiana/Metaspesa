using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Extensions;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;
using Microsoft.Extensions.DependencyInjection;

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
    IMarketRepository marketRepository,
    IServiceScopeFactory scopeFactory
  ) : CancellableCommandHandler<Command>(scopeFactory) {
    private List<Market> _addedMarkets = [];
    private List<ProductBrand> _addedBrands = [];
    private readonly List<int> _addedProductIds = [];
    private readonly List<string> _completedMarketNames = [];
    private DateOnly _registeredAt;

    protected override async Task<Result> ExecuteAsync(
      Command command, CancellationToken cancellationToken
    ) {
      ValidationResult validationResult = await validator.ValidateAsync(command, cancellationToken);
      if (!validationResult.IsValid) {
        return validationResult.ToDomainErrors();
      }

      List<Market> markets = command.ToMarkets();

      await AddMarketsAsync(markets, cancellationToken);
      await AddBrandsAsync(markets, cancellationToken);
      await AddProductsAsync(command, markets, cancellationToken);

      return Result.Success();
    }

    private async Task AddMarketsAsync(
      List<Market> markets, CancellationToken cancellationToken
    ) {
      List<Market> existingMarkets = await marketRepository.GetMarketsAsync(
        cancellationToken);
      var newMarkets = markets.Where(m =>
        !existingMarkets.Any(em => em.Name.Equals(m.Name, StringComparison.OrdinalIgnoreCase)))
        .ToList();

      if (newMarkets.Count != 0) {
        await marketRepository.AddMarketsAsync(newMarkets, cancellationToken);
        _addedMarkets = newMarkets;
      }
    }

    private async Task AddBrandsAsync(
      List<Market> markets, CancellationToken cancellationToken
    ) {
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
        _addedBrands = newBrands;
      }
    }

    private async Task AddProductsAsync(
      Command command, List<Market> markets, CancellationToken cancellationToken
    ) {
      _registeredAt = command.RegisteredAt ?? DateOnly.FromDateTime(DateTime.UtcNow);
      foreach (Market market in markets) {
        IReadOnlyCollection<int> addedIds = await marketRepository
          .AddMarketProductsAsync(market, _registeredAt, cancellationToken);
        _addedProductIds.AddRange(addedIds);
        _completedMarketNames.Add(market.Name);
      }
    }

    protected override async Task RollbackAsync(
      Command command, IServiceProvider services, CancellationToken cancellationToken
    ) {
      IMarketRepository repo = services.GetRequiredService<IMarketRepository>();

      if (_completedMarketNames.Count > 0) {
        await repo.DeleteProductsHistoryForMarketsAsync(
          _completedMarketNames, _registeredAt, cancellationToken);
      }
      if (_addedProductIds.Count > 0) {
        await repo.DeleteProductsAsync(_addedProductIds, cancellationToken);
      }
      if (_addedBrands.Count > 0) {
        await repo.DeleteBrandsAsync(
          [.. _addedBrands.Select(b => b.Name)], cancellationToken);
      }
      if (_addedMarkets.Count > 0) {
        await repo.DeleteMarketsAsync(
          [.. _addedMarkets.Select(m => m.Name)], cancellationToken);
      }
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
