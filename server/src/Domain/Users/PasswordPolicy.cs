using System.Diagnostics;

namespace Metaspesa.Domain.Users;

public static class PasswordPolicy {
  public const int MinimumLength = 10;

  public static bool HasMinimumLength(string password) {
    Debug.Assert(password is not null);

    return password.Length >= MinimumLength;
  }

  public static bool HasUppercase(string password) {
    Debug.Assert(password is not null);

    return password.Any(char.IsUpper);
  }

  public static bool HasLowercase(string password) {
    Debug.Assert(password is not null);

    return password.Any(char.IsLower);
  }

  public static bool HasDigit(string password) {
    Debug.Assert(password is not null);

    return password.Any(char.IsDigit);
  }

  public static bool HasSpecialCharacter(string password) {
    Debug.Assert(password is not null);

    return password.Any(c => !char.IsLetterOrDigit(c));
  }
}
