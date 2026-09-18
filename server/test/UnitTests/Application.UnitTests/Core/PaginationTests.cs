using Metaspesa.Application.Abstractions.Core;

namespace Metaspesa.Application.UnitTests.Core;

public class PaginationTests {
  [Fact]
  public void Constructor_RejectsZeroPageIndex() {
    const int index = 0;
    const int size = 24;

    Assert.Throws<ArgumentOutOfRangeException>(() => new Pagination(index, size));
  }

  [Fact]
  public void Constructor_RejectsNegativePageIndex() {
    const int index = -1;
    const int size = 24;

    Assert.Throws<ArgumentOutOfRangeException>(() => new Pagination(index, size));
  }

  [Fact]
  public void Constructor_AcceptsFirstPage() {
    var pagination = new Pagination(1, 24);

    Assert.Equal(1, pagination.Index);
  }

  [Fact]
  public void Constructor_RejectsZeroPageSize() {
    const int index = 1;
    const int size = 0;

    Assert.Throws<ArgumentOutOfRangeException>(() => new Pagination(index, size));
  }

  [Fact]
  public void Constructor_RejectsNegativePageSize() {
    const int index = 1;
    const int size = -1;

    Assert.Throws<ArgumentOutOfRangeException>(() => new Pagination(index, size));
  }

  [Fact]
  public void Constructor_AcceptsMaximumPageSize() {
    var pagination = new Pagination(1, Pagination.MaximumSize);

    Assert.Equal(Pagination.MaximumSize, pagination.Size);
  }

  [Fact]
  public void Constructor_RejectsPageSizeAboveMaximum() {
    int size = Pagination.MaximumSize + 1;

    Assert.Throws<ArgumentOutOfRangeException>(() => new Pagination(1, size));
  }

  [Fact]
  public void Constructor_RejectsPageWhoseOffsetExceedsIntMaxValue() {
    const int index = int.MaxValue;
    const int size = 2;

    Assert.Throws<ArgumentOutOfRangeException>(() => new Pagination(index, size));
  }

  [Fact]
  public void Skip_ReturnsZeroForFirstPage() {
    var pagination = new Pagination(1, 10);

    int skip = pagination.Skip;

    Assert.Equal(0, skip);
  }

  [Fact]
  public void Skip_ReturnsNumberOfItemsBeforeRequestedPage() {
    var pagination = new Pagination(3, 10);

    int skip = pagination.Skip;

    Assert.Equal(20, skip);
  }
}
