using System.Text.RegularExpressions;
using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Domain.Identity;

public readonly partial record struct Username {
  public string Value { get; }

  public Username(string value) {
    bool isValid = !string.IsNullOrWhiteSpace(value) &&
      ValidUsernameRegex().IsMatch(value.Trim());
    if (!isValid) {
      throw new InvalidUsernameException(value);
    }

    Value = value.Trim();
  }

  public override string ToString() => Value;
  [GeneratedRegex(@"^[a-zA-Z0-9_]+$")]
  private static partial Regex ValidUsernameRegex();
}