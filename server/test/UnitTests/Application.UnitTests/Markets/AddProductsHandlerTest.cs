using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Domain.Markets;
using NSubstitute;
using static Metaspesa.Application.Markets.AddMarketProducts;

namespace Metaspesa.Application.UnitTests.Markets;

public class AddProductsHandlerTest {
  private readonly IValidator<Command> _validator;
  private readonly IMarketRepository _marketRepository;
  private readonly Handler _handler;

  public AddProductsHandlerTest() {
    _validator = Substitute.For<IValidator<Command>>();
    _marketRepository = Substitute.For<IMarketRepository>();
    _handler = new Handler(_validator, _marketRepository);

    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>()).Returns([]);
    _marketRepository.GetBrandsAsync(Arg.Any<CancellationToken>()).Returns([]);
  }

  [Fact(DisplayName = "Returns errors when validation fails")]
  public async Task Handler_ReturnsErrors_WhenValidationFails() {
    // Arrange
    var command = new Command([], null);
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
    var command = new Command([], null);
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

  [Fact(DisplayName = "Calls AddMarketProductsAsync when validation succeeds")]
  public async Task Handler_CallsRepository_WhenValidationSucceeds() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99f, "1L", "Walmart", "Nike")],
      null);
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

  [Fact(DisplayName = "Returns success when handling succeeds")]
  public async Task Handler_ReturnsSuccess_WhenHandlingSucceeds() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99f, "1L", "Walmart", "Nike")],
      null);
    _validator.ValidateAsync(command, TestContext.Current.CancellationToken)
      .Returns(new ValidationResult());

    // Act
    Result result = await _handler.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
  }

  [Fact(DisplayName = "Passes registered_at to repository")]
  public async Task Handler_PassesRegisteredAt_ToRepository() {
    // Arrange
    var registeredAt = new DateOnly(2024, 1, 15);
    var command = new Command(
      [new CommandProduct("Milk", 1.99f, "1L", "Walmart", "Nike")],
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
      [new CommandProduct("Milk", 1.99f, "1L", "Walmart", "Nike")],
      null);
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
      [new CommandProduct("Milk", 1.99f, "1L", "Walmart", "Nike")],
      null);
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
}
