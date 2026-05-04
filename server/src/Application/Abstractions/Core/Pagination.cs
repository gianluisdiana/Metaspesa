namespace Metaspesa.Application.Abstractions.Core;

public record Pagination(int Index, int Size) {
  public static Pagination Infinite => new(1, int.MaxValue);

  public bool IsInfinite => Size == int.MaxValue;

  public int Skip => (Index - 1) * Size;
};
