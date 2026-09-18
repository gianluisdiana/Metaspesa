namespace Metaspesa.Application.Abstractions.Core;

public sealed record Pagination {
  public const int MaximumSize = 100;

  public Pagination(int index, int size) {
    if (index < 1) {
      throw new ArgumentOutOfRangeException(nameof(index), index,
        "Page index must be greater than 0.");
    }
    if (size < 1 || size > MaximumSize) {
      throw new ArgumentOutOfRangeException(nameof(size), size,
        $"Page size must be between 1 and {MaximumSize}.");
    }
    if ((long)(index - 1) * size > int.MaxValue) {
      throw new ArgumentOutOfRangeException(nameof(index), index,
        "Page is too large.");
    }

    Index = index;
    Size = size;
  }

  public int Index { get; }
  public int Size { get; }
  public int Skip => (Index - 1) * Size;
}