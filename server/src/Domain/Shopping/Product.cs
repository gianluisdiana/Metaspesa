using System.Text.RegularExpressions;

namespace Metaspesa.Domain.Shopping;

public partial record Product(string Name, string? Quantity, Price Price) {
  public string NormalizedName => MoreThanOneSpaceRegex()
    .Replace(Name.Trim(), " ")
    .ToUpperInvariant()
    .Replace(' ', '-');

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