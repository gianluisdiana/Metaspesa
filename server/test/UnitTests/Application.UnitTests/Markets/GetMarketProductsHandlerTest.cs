using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Markets;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;
using NSubstitute;

namespace Metaspesa.Application.UnitTests.Markets;

public class GetMarketProductsHandlerTest {
  private readonly IMarketRepository _marketRepository;
  private readonly IValidator<GetMarketProducts.Query> _validator;
  private readonly GetMarketProducts.Handler _handler;

  public GetMarketProductsHandlerTest() {
    _marketRepository = Substitute.For<IMarketRepository>();
    _validator = Substitute.For<IValidator<GetMarketProducts.Query>>();
    _validator
      .ValidateAsync(Arg.Any<GetMarketProducts.Query>(), Arg.Any<CancellationToken>())
      .Returns(new ValidationResult());
    _handler = new GetMarketProducts.Handler(_validator, _marketRepository);
  }

  private static GetMarketProductsFilter DefaultFilter(
    string? market = null, string? brandSegment = null, string? segment = null,
    Pagination? pagination = null
  ) => new(market, brandSegment, segment, pagination);

  [Fact(DisplayName = "Passes filter to the repository")]
  public async Task Handler_PassesFilter_ToRepository() {
    // Arrange
    GetMarketProductsFilter filter = DefaultFilter("Mercadona", "Hacendado", "leche");
    _marketRepository
      .GetProductsAsync(Arg.Any<GetMarketProductsFilter>(), Arg.Any<CancellationToken>())
      .Returns(new PagedResult<Market>([], 0));

    // Act
    await _handler.Handle(new GetMarketProducts.Query(filter), TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.Received(1).GetProductsAsync(
      Arg.Is<GetMarketProductsFilter>(f =>
        f.MarketName == "Mercadona" && f.BrandNameSegment == "Hacendado" && f.NameSegment == "leche"),
      TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Passes pagination index to the repository")]
  public async Task Handler_PassesPaginationIndex_ToRepository() {
    // Arrange
    GetMarketProductsFilter filter = DefaultFilter(pagination: new Pagination(3, 10));
    _marketRepository
      .GetProductsAsync(Arg.Any<GetMarketProductsFilter>(), Arg.Any<CancellationToken>())
      .Returns(new PagedResult<Market>([], 0));

    // Act
    await _handler.Handle(new GetMarketProducts.Query(filter), TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.Received(1).GetProductsAsync(
      Arg.Is<GetMarketProductsFilter>(f => f.Pagination!.Index == 3),
      TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Passes pagination size to the repository")]
  public async Task Handler_PassesPaginationSize_ToRepository() {
    // Arrange
    GetMarketProductsFilter filter = DefaultFilter(pagination: new Pagination(3, 10));
    _marketRepository
      .GetProductsAsync(Arg.Any<GetMarketProductsFilter>(), Arg.Any<CancellationToken>())
      .Returns(new PagedResult<Market>([], 0));

    // Act
    await _handler.Handle(new GetMarketProducts.Query(filter), TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.Received(1).GetProductsAsync(
      Arg.Is<GetMarketProductsFilter>(f => f.Pagination!.Size == 10),
      TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Assigns infinite pagination when filter pagination is null")]
  public async Task Handler_AssignsInfinitePagination_WhenFilterPaginationIsNull() {
    // Arrange
    _marketRepository
      .GetProductsAsync(Arg.Any<GetMarketProductsFilter>(), Arg.Any<CancellationToken>())
      .Returns(new PagedResult<Market>([], 0));

    // Act
    await _handler.Handle(
      new GetMarketProducts.Query(DefaultFilter()),
      TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.Received(1).GetProductsAsync(
      Arg.Is<GetMarketProductsFilter>(f => f.Pagination != null && f.Pagination.IsInfinite),
      TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Returns error when validator fails")]
  public async Task Handler_ReturnsError_WhenValidatorFails() {
    // Arrange
    _validator
      .ValidateAsync(Arg.Any<GetMarketProducts.Query>(), Arg.Any<CancellationToken>())
      .Returns(new ValidationResult([new ValidationFailure("field", "error")]));

    // Act
    Result<PagedResult<Market>> result = await _handler.Handle(
      new GetMarketProducts.Query(DefaultFilter()),
      TestContext.Current.CancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
  }

  [Fact(DisplayName = "Does not call repository when validator fails")]
  public async Task Handler_DoesNotCallRepository_WhenValidatorFails() {
    // Arrange
    _validator
      .ValidateAsync(Arg.Any<GetMarketProducts.Query>(), Arg.Any<CancellationToken>())
      .Returns(new ValidationResult([new ValidationFailure("field", "error")]));

    // Act
    await _handler.Handle(
      new GetMarketProducts.Query(DefaultFilter()),
      TestContext.Current.CancellationToken);

    // Assert
    await _marketRepository.DidNotReceive().GetProductsAsync(
      Arg.Any<GetMarketProductsFilter>(), Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Returns markets from repository")]
  public async Task Handler_ReturnsMarkets_FromRepository() {
    // Arrange
    var markets = new List<Market> {
      new("Mercadona", [
        new MarketProduct("Leche", new ProductBrand("Hacendado"), [new ProductFormat("1L", new Price(0.89f), null)]),
      ]),
    };
    _marketRepository
      .GetProductsAsync(Arg.Any<GetMarketProductsFilter>(), Arg.Any<CancellationToken>())
      .Returns(new PagedResult<Market>(markets, 1));

    // Act
    Result<PagedResult<Market>> result = await _handler.Handle(
      new GetMarketProducts.Query(DefaultFilter()),
      TestContext.Current.CancellationToken);

    // Assert
    Assert.Single(result.Value.Values);
  }

  [Fact(DisplayName = "Returns total count from repository")]
  public async Task Handler_ReturnsTotalCount_FromRepository() {
    // Arrange
    _marketRepository
      .GetProductsAsync(Arg.Any<GetMarketProductsFilter>(), Arg.Any<CancellationToken>())
      .Returns(new PagedResult<Market>([], 42));

    // Act
    Result<PagedResult<Market>> result = await _handler.Handle(
      new GetMarketProducts.Query(DefaultFilter()),
      TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(42, result.Value.TotalCount);
  }

  [Fact(DisplayName = "Returns empty values when repository returns nothing")]
  public async Task Handler_ReturnsEmptyValues_WhenRepositoryReturnsNothing() {
    // Arrange
    _marketRepository
      .GetProductsAsync(Arg.Any<GetMarketProductsFilter>(), Arg.Any<CancellationToken>())
      .Returns(new PagedResult<Market>([], 0));

    // Act
    Result<PagedResult<Market>> result = await _handler.Handle(
      new GetMarketProducts.Query(DefaultFilter()),
      TestContext.Current.CancellationToken);

    // Assert
    Assert.Empty(result.Value.Values);
  }

  [Fact(DisplayName = "Returns zero total when repository returns nothing")]
  public async Task Handler_ReturnsZeroTotal_WhenRepositoryReturnsNothing() {
    // Arrange
    _marketRepository
      .GetProductsAsync(Arg.Any<GetMarketProductsFilter>(), Arg.Any<CancellationToken>())
      .Returns(new PagedResult<Market>([], 0));

    // Act
    Result<PagedResult<Market>> result = await _handler.Handle(
      new GetMarketProducts.Query(DefaultFilter()),
      TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(0, result.Value.TotalCount);
  }
}
