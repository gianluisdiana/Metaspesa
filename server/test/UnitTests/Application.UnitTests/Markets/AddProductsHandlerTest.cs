using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Domain.Markets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;
using static Metaspesa.Application.Markets.AddMarketProducts;

namespace Metaspesa.Application.UnitTests.Markets;

public class AddMarketProductsHandlerTest {
  private readonly IValidator<Command> _validator;
  private readonly IMarketRepository _marketRepository;
  private readonly ILogger<Handler> _logger;
  private readonly Handler _handler;

  public AddMarketProductsHandlerTest() {
    _validator = Substitute.For<IValidator<Command>>();
    _marketRepository = Substitute.For<IMarketRepository>();
    _logger = Substitute.For<ILogger<Handler>>();

    IServiceScopeFactory scopeFactory = new ServiceCollection()
      .AddSingleton(_marketRepository)
      .BuildServiceProvider()
      .GetRequiredService<IServiceScopeFactory>();

    _handler = new Handler(_validator, _marketRepository, scopeFactory, _logger);

    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>()).Returns([]);
    _marketRepository.GetBrandsAsync(Arg.Any<CancellationToken>()).Returns([]);
    _marketRepository.AddMarketProductsAsync(
      Arg.Any<Market>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
      .Returns([]);
  }

  private static TaskCompletionSource CreateRollbackSignal() =>
    new(TaskCreationOptions.RunContinuationsAsynchronously);

  private static async Task WaitForRollback(Task rollbackSignal) =>
    await rollbackSignal.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

  [Fact(DisplayName = "Returns errors when validation fails")]
  public async Task Handler_ReturnsErrors_WhenValidationFails() {
    // Arrange
    var command = new Command([], DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
  }

  [Fact(DisplayName = "Does not call repository when validation fails")]
  public async Task Handler_DoesNotCallRepository_WhenValidationFails() {
    // Arrange
    var command = new Command([], DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.DidNotReceive().AddMarketProductsAsync(
      Arg.Any<Market>(),
      Arg.Any<DateOnly>(),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Does not delete markets when validation fails before changes")]
  public async Task Handler_DoesNotDeleteMarkets_WhenValidationFailsBeforeChanges() {
    // Arrange
    var command = new Command([], DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.DidNotReceive().DeleteMarketsAsync(
      Arg.Any<IReadOnlyCollection<string>>(),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Does not delete brands when validation fails before changes")]
  public async Task Handler_DoesNotDeleteBrands_WhenValidationFailsBeforeChanges() {
    // Arrange
    var command = new Command([], DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.DidNotReceive().DeleteBrandsAsync(
      Arg.Any<IReadOnlyCollection<string>>(),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Does not delete products when validation fails before changes")]
  public async Task Handler_DoesNotDeleteProducts_WhenValidationFailsBeforeChanges() {
    // Arrange
    var command = new Command([], DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult([new ValidationFailure()]));

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.DidNotReceive().DeleteProductsAsync(
      Arg.Any<IReadOnlyCollection<int>>(),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Calls AddMarketProductsAsync when validation succeeds")]
  public async Task Handler_CallsRepository_WhenValidationSucceeds() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.Received(1).AddMarketProductsAsync(
      Arg.Is<Market>(m => m.Products.Count == 1),
      Arg.Any<DateOnly>(),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Combines quantity and unit of measure into product format")]
  public async Task Handler_CombinesQuantityAndUnitOfMeasure_IntoProductFormat() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.Received(1).AddMarketProductsAsync(
      Arg.Is<Market>(m =>
        m.Products.Single().Formats.Single().Quantity == new AQuantity(1, "L")),
      Arg.Any<DateOnly>(),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Returns success when handling succeeds")]
  public async Task Handler_ReturnsSuccess_WhenHandlingSucceeds() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    Result result = await _handler.Handle(
      command, TestContext.Current.CancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
  }

  [Fact(DisplayName = "Passes registered_at to repository")]
  public async Task Handler_PassesRegisteredAt_ToRepository() {
    // Arrange
    var registeredAt = new DateOnly(2024, 1, 15);
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      registeredAt);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.Received(1).AddMarketProductsAsync(
      Arg.Any<Market>(),
      Arg.Is<DateOnly>(d => d == registeredAt),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Skips AddMarketsAsync when all markets already exist")]
  public async Task Handler_SkipsAddMarketsAsync_WhenMarketAlreadyExists() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([new Market("Walmart", [])]);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.DidNotReceive().AddMarketsAsync(
      Arg.Any<IReadOnlyCollection<Market>>(),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Skips AddBrandsAsync when all brands already exist")]
  public async Task Handler_SkipsAddBrandsAsync_WhenBrandAlreadyExists() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());
    _marketRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .Returns([new ProductBrand("Nike")]);

    // Act
    await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.DidNotReceive().AddBrandsAsync(
      Arg.Any<IReadOnlyCollection<ProductBrand>>(),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Deletes added markets if command is cancelled getting brands")]
  public async Task Handler_DeletesAddedMarkets_IfCancelledGettingBrands() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _marketRepository.ClearSubstitute();

    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .ThrowsAsync<OperationCanceledException>();
    TaskCompletionSource rollbackSignal = CreateRollbackSignal();
    _marketRepository.DeleteMarketsAsync(
        Arg.Any<IReadOnlyCollection<string>>(),
        Arg.Any<CancellationToken>())
      .Returns(_ => {
        rollbackSignal.SetResult();
        return Task.CompletedTask;
      });

    // Act
    try {
      await _handler.Handle(command, TestContext.Current.CancellationToken);
    } catch (OperationCanceledException) {
      // Expected; this test verifies only the rollback side effect.
    }

    // Assert
    await WaitForRollback(rollbackSignal.Task);
    await _marketRepository.Received(1).DeleteMarketsAsync(
      Arg.Is<IReadOnlyCollection<string>>(m => m.Single() == "Walmart"),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Throws if command is cancelled getting brands")]
  public async Task Handler_ThrowsOperationCanceled_WhenCancelledGettingBrands() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _marketRepository.ClearSubstitute();

    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .ThrowsAsync<OperationCanceledException>();

    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(() =>
      _handler.Handle(command, TestContext.Current.CancellationToken));
  }

  [Fact(DisplayName = "Deletes added markets if command is cancelled adding brands")]
  public async Task Handler_DeletesAddedMarkets_IfCancelledAddingBrands() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _marketRepository.ClearSubstitute();

    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.AddBrandsAsync(
        Arg.Any<IReadOnlyCollection<ProductBrand>>(),
        Arg.Any<CancellationToken>())
      .ThrowsAsync<OperationCanceledException>();
    TaskCompletionSource rollbackSignal = CreateRollbackSignal();
    _marketRepository.DeleteMarketsAsync(
        Arg.Any<IReadOnlyCollection<string>>(),
        Arg.Any<CancellationToken>())
      .Returns(_ => {
        rollbackSignal.SetResult();
        return Task.CompletedTask;
      });

    // Act
    try {
      await _handler.Handle(command, TestContext.Current.CancellationToken);
    } catch (OperationCanceledException) {
      // Expected; this test verifies only the rollback side effect.
    }

    // Assert
    await WaitForRollback(rollbackSignal.Task);
    await _marketRepository.Received(1).DeleteMarketsAsync(
      Arg.Is<IReadOnlyCollection<string>>(m => m.Single() == "Walmart"),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Throws if command is cancelled adding brands")]
  public async Task Handler_ThrowsOperationCanceled_WhenCancelledAddingBrands() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _marketRepository.ClearSubstitute();

    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.AddBrandsAsync(
        Arg.Any<IReadOnlyCollection<ProductBrand>>(),
        Arg.Any<CancellationToken>())
      .ThrowsAsync<OperationCanceledException>();

    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(() =>
      _handler.Handle(command, TestContext.Current.CancellationToken));
  }

  [Fact(DisplayName = "Deletes added markets if command is cancelled adding products")]
  public async Task Handler_DeletesAddedMarkets_IfCancelledAddingProducts() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _marketRepository.ClearSubstitute();

    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.AddMarketProductsAsync(
        Arg.Any<Market>(),
        Arg.Any<DateOnly>(),
        Arg.Any<CancellationToken>())
      .ThrowsAsync<OperationCanceledException>();
    TaskCompletionSource rollbackSignal = CreateRollbackSignal();
    _marketRepository.DeleteMarketsAsync(
        Arg.Any<IReadOnlyCollection<string>>(),
        Arg.Any<CancellationToken>())
      .Returns(_ => {
        rollbackSignal.SetResult();
        return Task.CompletedTask;
      });

    // Act
    try {
      await _handler.Handle(command, TestContext.Current.CancellationToken);
    } catch (OperationCanceledException) {
      // Expected; this test verifies only the rollback side effect.
    }

    // Assert
    await WaitForRollback(rollbackSignal.Task);
    await _marketRepository.Received(1).DeleteMarketsAsync(
      Arg.Is<IReadOnlyCollection<string>>(m => m.Single() == "Walmart"),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Throws if command is cancelled adding products")]
  public async Task Handler_ThrowsOperationCanceled_WhenCancelledAddingProducts() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _marketRepository.ClearSubstitute();

    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.AddMarketProductsAsync(
        Arg.Any<Market>(),
        Arg.Any<DateOnly>(),
        Arg.Any<CancellationToken>())
      .ThrowsAsync<OperationCanceledException>();

    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(() =>
      _handler.Handle(command, TestContext.Current.CancellationToken));
  }

  [Fact(DisplayName = "Deletes added brands if command is cancelled adding products")]
  public async Task Handler_DeletesAddedBrands_IfCancelledAddingProducts() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null)],
      DateOnly.MinValue);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _marketRepository.ClearSubstitute();

    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.AddMarketProductsAsync(
        Arg.Any<Market>(),
        Arg.Any<DateOnly>(),
        Arg.Any<CancellationToken>())
      .ThrowsAsync<OperationCanceledException>();
    TaskCompletionSource rollbackSignal = CreateRollbackSignal();
    _marketRepository.DeleteBrandsAsync(
        Arg.Any<IReadOnlyCollection<string>>(),
        Arg.Any<CancellationToken>())
      .Returns(_ => {
        rollbackSignal.SetResult();
        return Task.CompletedTask;
      });

    // Act
    try {
      await _handler.Handle(command, TestContext.Current.CancellationToken);
    } catch (OperationCanceledException) {
      // Expected; this test verifies only the rollback side effect.
    }

    // Assert
    await WaitForRollback(rollbackSignal.Task);
    await _marketRepository.Received(1).DeleteBrandsAsync(
      Arg.Is<IReadOnlyCollection<string>>(b => b.Single() == "Nike"),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Deletes product history for completed markets if command is cancelled adding later products")]
  public async Task Handler_DeletesProductHistoryForCompletedMarkets_IfCancelledAddingLaterProducts() {
    // Arrange
    var registeredAt = new DateOnly(2024, 1, 15);
    var command = new Command(
      [
        new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null),
        new CommandProduct("Bread", 2.49m, 500, "g", "Target", "Adidas", null),
      ],
      registeredAt);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _marketRepository.ClearSubstitute();

    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.AddMarketProductsAsync(
        Arg.Is<Market>(m => m.Name == "Walmart"),
        Arg.Any<DateOnly>(),
        Arg.Any<CancellationToken>())
      .Returns([123]);

    _marketRepository.AddMarketProductsAsync(
        Arg.Is<Market>(m => m.Name == "Target"),
        Arg.Any<DateOnly>(),
        Arg.Any<CancellationToken>())
      .ThrowsAsync<OperationCanceledException>();

    TaskCompletionSource rollbackSignal = CreateRollbackSignal();
    _marketRepository.DeleteProductsHistoryForMarketsAsync(
        Arg.Any<IReadOnlyCollection<string>>(),
        Arg.Any<DateOnly>(),
        Arg.Any<CancellationToken>())
      .Returns(_ => {
        rollbackSignal.SetResult();
        return Task.CompletedTask;
      });

    // Act
    try {
      await _handler.Handle(command, TestContext.Current.CancellationToken);
    } catch (OperationCanceledException) {
      // Expected; this test verifies only the rollback side effect.
    }

    // Assert
    await WaitForRollback(rollbackSignal.Task);
    await _marketRepository.Received(1).DeleteProductsHistoryForMarketsAsync(
      Arg.Is<IReadOnlyCollection<string>>(m => m.Single() == "Walmart"),
      Arg.Is<DateOnly>(d => d == registeredAt),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Deletes added product ids if command is cancelled adding later products")]
  public async Task Handler_DeletesAddedProductIds_IfCancelledAddingLaterProducts() {
    // Arrange
    var registeredAt = new DateOnly(2024, 1, 15);
    var command = new Command(
      [
        new CommandProduct("Milk", 1.99m, 1, "L", "Walmart", "Nike", null),
        new CommandProduct("Bread", 2.49m, 500, "g", "Target", "Adidas", null),
      ],
      registeredAt);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    _marketRepository.ClearSubstitute();

    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .Returns([]);

    _marketRepository.AddMarketProductsAsync(
        Arg.Is<Market>(m => m.Name == "Walmart"),
        Arg.Any<DateOnly>(),
        Arg.Any<CancellationToken>())
      .Returns([123]);

    _marketRepository.AddMarketProductsAsync(
        Arg.Is<Market>(m => m.Name == "Target"),
        Arg.Any<DateOnly>(),
        Arg.Any<CancellationToken>())
      .ThrowsAsync<OperationCanceledException>();

    TaskCompletionSource rollbackSignal = CreateRollbackSignal();
    _marketRepository.DeleteProductsAsync(
        Arg.Any<IReadOnlyCollection<int>>(),
        Arg.Any<CancellationToken>())
      .Returns(_ => {
        rollbackSignal.SetResult();
        return Task.CompletedTask;
      });

    // Act
    try {
      await _handler.Handle(command, TestContext.Current.CancellationToken);
    } catch (OperationCanceledException) {
      // Expected; this test verifies only the rollback side effect.
    }

    // Assert
    await WaitForRollback(rollbackSignal.Task);
    await _marketRepository.Received(1).DeleteProductsAsync(
      Arg.Is<IReadOnlyCollection<int>>(ids => ids.Single() == 123),
      Arg.Any<CancellationToken>());
  }
}
