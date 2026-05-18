using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Metaspesa.Domain.Shopping;

public partial record Product(string Name, Quantity? Quantity, Price Price) {
  public string NormalizedName => NormalizeName(Name);

  private static string NormalizeName(string name) {
    Debug.Assert(name is not null);

    return MoreThanOneSpaceRegex()
      .Replace(name.Trim(), " ")
      .ToUpperInvariant()
      .Replace(' ', '-');
  }

  [GeneratedRegex(@"\s{1,}")]
  private static partial Regex MoreThanOneSpaceRegex();

  public bool HasSamePrice(Product? other) {
    return Price == other?.Price;
  }

  public virtual bool Equals(Product? other) {
    return other is not null &&
      NormalizedName == other.NormalizedName &&
      Quantity == other.Quantity;
  }

  public override int GetHashCode() {
    return HashCode.Combine(NormalizedName, Quantity);
  }
}