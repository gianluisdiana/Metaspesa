using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Domain.Identity;

public readonly record struct Username {
  public string Value { get; }

  public Username(string value) {
    if (string.IsNullOrWhiteSpace(value)) {
      throw new InvalidUsernameException(value);
    }

    Value = value;
  }

  public override string ToString() => Value;
}
