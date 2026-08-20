using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Domain.Identity;

public readonly record struct PasswordHash {
  public string Value { get; }

  public PasswordHash(string value) {
    if (string.IsNullOrWhiteSpace(value)) {
      throw new InvalidPasswordHashException();
    }

    Value = value;
  }

  public override string ToString() => Value;
}