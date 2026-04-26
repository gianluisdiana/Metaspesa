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
    var command = new Command([], null);

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
    var command = new Command([new CommandProduct(name, 1.99f, "1L", "Walmart", "Nike")], null);

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
      [new CommandProduct("Milk", 1.99f, "1L", marketName, "Nike")], null);

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
      [new CommandProduct("Milk", 1.99f, "1L", "Walmart", brandName)], null);

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
      [new CommandProduct("Milk", 1.99f, quantity, "Walmart", "Nike")], null);

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
      [new CommandProduct("Milk", -1f, "1L", "Walmart", "Nike")], null);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Products[0].Price")
      .WithErrorCode("Market.Product.Price.Negative");
  }

  [Fact(DisplayName = "Fails when duplicate product exists in same market and brand")]
  public async Task Validator_Fails_WhenDuplicateProductExists() {
    // Arrange
    var command = new Command([
      new CommandProduct("Milk", 1.99f, "1L", "Walmart", "Nike"),
      new CommandProduct("Milk", 2.50f, "2L", "Walmart", "Nike"),
    ], null);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldHaveValidationErrorFor("Products[0]")
      .WithErrorCode("Market.Product.Duplicate");
  }

  [Fact(DisplayName = "Passes when all products are valid")]
  public async Task Validator_Passes_WhenAllProductsAreValid() {
    // Arrange
    var command = new Command([
      new CommandProduct("Milk", 1.99f, "1L", "Walmart", "Nike"),
      new CommandProduct("Bread", 0.99f, "500g", "Carrefour", "Adidas"),
    ], null);

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
      [new CommandProduct("Milk", 0f, "1L", "Walmart", "Nike")], null);

    // Act
    TestValidationResult<Command> result = await _validator.TestValidateAsync(
      command, cancellationToken: TestContext.Current.CancellationToken);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }
}
