using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Markets;

namespace Metaspesa.Application.UnitTests.Markets;

public class GetMarketProductsValidatorTest {
  private readonly GetMarketProducts.Validator _validator = new();

  private static GetMarketProducts.Query QueryWith(Pagination? pagination) =>
    new(new GetMarketProductsFilter(null, null, null, pagination));

  [Fact(DisplayName = "Returns error when page index is zero")]
  public async Task Validator_ReturnsError_WhenPageIndexIsZero() {
    ValidationResult result = await _validator.ValidateAsync(
      QueryWith(new Pagination(0, 20)), TestContext.Current.CancellationToken);

    Assert.False(result.IsValid);
  }

  [Fact(DisplayName = "Returns error when page index is negative")]
  public async Task Validator_ReturnsError_WhenPageIndexIsNegative() {
    ValidationResult result = await _validator.ValidateAsync(
      QueryWith(new Pagination(-1, 20)), TestContext.Current.CancellationToken);

    Assert.False(result.IsValid);
  }

  [Fact(DisplayName = "Returns error when page size is zero")]
  public async Task Validator_ReturnsError_WhenPageSizeIsZero() {
    ValidationResult result = await _validator.ValidateAsync(
      QueryWith(new Pagination(1, 0)), TestContext.Current.CancellationToken);

    Assert.False(result.IsValid);
  }

  [Fact(DisplayName = "Returns error when page size is negative")]
  public async Task Validator_ReturnsError_WhenPageSizeIsNegative() {
    ValidationResult result = await _validator.ValidateAsync(
      QueryWith(new Pagination(1, -5)), TestContext.Current.CancellationToken);

    Assert.False(result.IsValid);
  }

  [Fact(DisplayName = "Is valid when pagination is null")]
  public async Task Validator_IsValid_WhenPaginationIsNull() {
    ValidationResult result = await _validator.ValidateAsync(
      QueryWith(null), TestContext.Current.CancellationToken);

    Assert.True(result.IsValid);
  }

  [Fact(DisplayName = "Is valid when pagination is infinite")]
  public async Task Validator_IsValid_WhenPaginationIsInfinite() {
    ValidationResult result = await _validator.ValidateAsync(
      QueryWith(Pagination.Infinite), TestContext.Current.CancellationToken);

    Assert.True(result.IsValid);
  }

  [Fact(DisplayName = "Is valid when page index and size are positive")]
  public async Task Validator_IsValid_WhenPageIndexAndSizeArePositive() {
    ValidationResult result = await _validator.ValidateAsync(
      QueryWith(new Pagination(1, 20)), TestContext.Current.CancellationToken);

    Assert.True(result.IsValid);
  }
}
