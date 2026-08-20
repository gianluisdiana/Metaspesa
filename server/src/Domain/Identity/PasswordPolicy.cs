using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Domain.Identity;

public static class PasswordPolicy {
  public const int MinimumLength = 10;

  public static void EnsureIsValid(string? password) {
    if (password is null || !HasMinimumLength(password)) {
      throw new PasswordTooShortException();
    }

    if (!HasUppercase(password)) {
      throw new PasswordMissingUppercaseException();
    }

    if (!HasLowercase(password)) {
      throw new PasswordMissingLowercaseException();
    }

    if (!HasDigit(password)) {
      throw new PasswordMissingDigitException();
    }

    if (!HasSpecialCharacter(password)) {
      throw new PasswordMissingSpecialCharacterException();
    }
  }

  private static bool HasMinimumLength(string password) =>
    password.Length >= MinimumLength;

  private static bool HasUppercase(string password) =>
    password.Any(char.IsUpper);

  private static bool HasLowercase(string password) =>
    password.Any(char.IsLower);

  private static bool HasDigit(string password) =>
    password.Any(char.IsDigit);

  private static bool HasSpecialCharacter(string password) =>
    password.Any(c => !char.IsLetterOrDigit(c));
}