using FluentValidation.TestHelper;
using Metaspesa.Application.Abstractions.Markets;
using NSubstitute;
using static Metaspesa.Application.Markets.AddMarketProducts;

namespace Metaspesa.Application.UnitTests.Markets;

public class AddProductsValidatorTest {
  private readonly IMarketRepository _marketRepository;
  private readonly Validator _validator;

  public AddProductsValidatorTest() {
    _marketRepository = Substitute.For<IMarketRepository>();
    _validator = new Validator(_marketRepository);
  }

  [Fact(DisplayName = "Fails when products collection is empty")]
  public async Task Validator_Fails_WhenProductsCollectionIsEmpty() {
    // Arrange
    var command = new Command([], DateOnly.MinValue);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Products)
      .WithErrorCode("Market.Products.Empty");
  }

  [Theory(DisplayName = "Fails when any product name is empty")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public async Task Validator_Fails_WhenAnyProductNameIsEmpty(string? name) {
    // Arrange
    var command = new Command([new CommandProduct(name, 1.99m, "1L", "Walmart", "Nike", null)], DateOnly.MinValue);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Products[0].Name")
      .WithErrorCode("Market.Product.Name.Empty");
  }

  [Theory(DisplayName = "Fails when any market name is empty")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public async Task Validator_Fails_WhenAnyMarketNameIsEmpty(string? marketName) {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, "1L", marketName, "Nike", null)], DateOnly.MinValue);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Products[0].MarketName")
      .WithErrorCode("Market.Product.MarketName.Empty");
  }

  [Theory(DisplayName = "Fails when any brand name is empty")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public async Task Validator_Fails_WhenAnyBrandNameIsEmpty(string? brandName) {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, "1L", "Walmart", brandName, null)], DateOnly.MinValue);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Products[0].BrandName")
      .WithErrorCode("Market.Product.BrandName.Empty");
  }

  [Theory(DisplayName = "Fails when quantity is empty")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public async Task Validator_Fails_WhenQuantityIsEmpty(string? quantity) {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 1.99m, quantity, "Walmart", "Nike", null)], DateOnly.MinValue);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Products[0].Quantity")
      .WithErrorCode("Market.Product.Quantity.Empty");
  }

  [Fact(DisplayName = "Fails when price is negative")]
  public async Task Validator_Fails_WhenPriceIsNegative() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", -1m, "1L", "Walmart", "Nike", null)], DateOnly.MinValue);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Products[0].Price")
      .WithErrorMessage("Product price '-1' must be greater than or equal to 0.")
      .WithErrorCode("Market.Product.Price.Negative");
  }

  [Fact(DisplayName = "Fails when duplicate product exists in same market, brand and quantity")]
  public async Task Validator_Fails_WhenDuplicateProductExists() {
    // Arrange
    var command = new Command([
      new CommandProduct("Milk", 1.99m, "1L", "Walmart", "Nike", null),
      new CommandProduct("Milk", 2.50m, "1L", "Walmart", "Nike", null),
    ], DateOnly.MinValue);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Products[0]")
      .WithErrorMessage(
        "Each product must be unique in name, market, brand and quantity combination. " +
        "Repeated product: name 'Milk', market 'Walmart', brand 'Nike', quantity '1L'.")
      .WithErrorCode("Market.Product.Duplicate");
  }

  [Theory(DisplayName = "Fails when RegisteredAt is before January 1, 2023")]
  [InlineData(2022, 12, 31)]
  [InlineData(2020, 1, 1)]
  public async Task Validator_Fails_WhenRegisteredAtIsTooOld(
    int year, int month, int day
  ) {
    // Arrange
    var registeredAt = new DateOnly(year, month, day);
    var command = new Command([
      new CommandProduct("Milk", 1.99m, "1L", "Walmart", "Nike", null),
    ], registeredAt);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.RegisteredAt)
      .WithErrorMessage(
        $"RegisteredAt '{registeredAt:yyyy-MM-dd}' must be on or after January 1, 2023.")
      .WithErrorCode("Market.RegisteredAt.TooOld");
  }

  [Fact(DisplayName = "Passes when all products are valid")]
  public async Task Validator_Passes_WhenAllProductsAreValid() {
    // Arrange
    var command = new Command([
      new CommandProduct("Milk", 1.99m, "1L", "Walmart", "Nike", null),
      new CommandProduct("Bread", 0.99m, "500g", "Carrefour", "Adidas", null),
    ], DateOnly.FromDateTime(new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc)));

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact(DisplayName = "Passes when price is zero")]
  public async Task Validator_Passes_WhenPriceIsZero() {
    // Arrange
    var command = new Command(
      [new CommandProduct("Milk", 0m, "1L", "Walmart", "Nike", null)],
      DateOnly.FromDateTime(new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc)));

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }
}