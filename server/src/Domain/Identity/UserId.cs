using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Domain.Identity;

public readonly record struct UserId {
  public Guid Value { get; }

  public UserId(Guid value) {
    if (value == Guid.Empty) {
      throw new InvalidUserIdException();
    }

    Value = value;
  }

  public override string ToString() => Value.ToString();
}