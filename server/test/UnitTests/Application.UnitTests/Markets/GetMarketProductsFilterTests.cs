using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;

namespace Metaspesa.Application.UnitTests.Markets;

public class GetMarketProductsFilterTests {
  [Theory(DisplayName = "Throws when finite page index is not positive")]
  [InlineData(0)]
  [InlineData(-1)]
  public void Constructor_Throws_WhenPageIndexIsNotPositive(int index) {
    void action() => _ = new GetMarketProductsFilter(
      null, null, null, new Pagination(index, 20));

    ArgumentOutOfRangeException exception =
      Assert.Throws<ArgumentOutOfRangeException>(action);

    Assert.Equal("pagination", exception.ParamName);
    Assert.Equal(index, exception.ActualValue);
  }

  [Theory(DisplayName = "Throws when finite page size is not positive")]
  [InlineData(0)]
  [InlineData(-5)]
  public void Constructor_Throws_WhenPageSizeIsNotPositive(int size) {
    void action() => _ = new GetMarketProductsFilter(
      null, null, null, new Pagination(1, size));

    ArgumentOutOfRangeException exception =
      Assert.Throws<ArgumentOutOfRangeException>(action);

    Assert.Equal("pagination", exception.ParamName);
    Assert.Equal(size, exception.ActualValue);
  }

  [Fact(DisplayName = "Accepts missing pagination")]
  public void Constructor_Accepts_WhenPaginationIsNull() {
    var filter = new GetMarketProductsFilter(null, null, null, null);

    Assert.Null(filter.Pagination);
  }

  [Fact(DisplayName = "Accepts infinite pagination")]
  public void Constructor_Accepts_WhenPaginationIsInfinite() {
    var filter = new GetMarketProductsFilter(
      null, null, null, Pagination.Infinite);

    Assert.NotNull(filter.Pagination);
    Assert.True(filter.Pagination.IsInfinite);
  }

  [Fact(DisplayName = "Accepts positive finite pagination")]
  public void Constructor_Accepts_WhenPaginationIsPositive() {
    var pagination = new Pagination(1, 20);

    var filter = new GetMarketProductsFilter(null, null, null, pagination);

    Assert.Equal(pagination, filter.Pagination);
  }
}
