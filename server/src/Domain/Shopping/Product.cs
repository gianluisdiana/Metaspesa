using System.Text.RegularExpressions;

namespace Metaspesa.Domain.Shopping;

public partial record Product(string Name, string? Quantity) {
  public string NormalizedName => MoreThanOneSpaceRegex()
    .Replace(Name.Trim(), " ")
    .ToUpperInvariant()
    .Replace(' ', '-');

  [GeneratedRegex(@"\s{1,}")]
  private static partial Regex MoreThanOneSpaceRegex();
}
