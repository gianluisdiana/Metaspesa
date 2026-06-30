using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Domain.UnitTests.Identity;

public static class PasswordPolicyTest {
  public class EnsureIsValid {
    [Fact(DisplayName = "Completes when password satisfies every policy rule")]
    public void PasswordPolicy_Completes_WhenPasswordIsValid() {
      // Act
      Exception? exception = Record.Exception(() =>
        PasswordPolicy.EnsureIsValid("Abc!12345X"));

      // Assert
      Assert.Null(exception);
    }

    [Fact(DisplayName = "Throws password-too-short exception when password is null")]
    public void PasswordPolicy_ThrowsPasswordTooShortException_WhenPasswordIsNull() {
      // Act
      Action act = () => PasswordPolicy.EnsureIsValid(null!);

      // Assert
      Assert.Throws<PasswordTooShortException>(act);
    }

    [Fact(DisplayName = "Throws password-too-short exception when password is below minimum length")]
    public void PasswordPolicy_ThrowsPasswordTooShortException_WhenPasswordIsTooShort() {
      // Act
      Action act = () => PasswordPolicy.EnsureIsValid("Abc!12345");

      // Assert
      Assert.Throws<PasswordTooShortException>(act);
    }

    [Fact(DisplayName = "Throws missing-uppercase exception when password has no uppercase letter")]
    public void PasswordPolicy_ThrowsPasswordMissingUppercaseException_WhenNoUppercase() {
      // Act
      Action act = () => PasswordPolicy.EnsureIsValid("alllowercase1!");

      // Assert
      Assert.Throws<PasswordMissingUppercaseException>(act);
    }

    [Fact(DisplayName = "Throws missing-lowercase exception when password has no lowercase letter")]
    public void PasswordPolicy_ThrowsPasswordMissingLowercaseException_WhenNoLowercase() {
      // Act
      Action act = () => PasswordPolicy.EnsureIsValid("UPPERCASE123!");

      // Assert
      Assert.Throws<PasswordMissingLowercaseException>(act);
    }

    [Fact(DisplayName = "Throws missing-digit exception when password has no digit")]
    public void PasswordPolicy_ThrowsPasswordMissingDigitException_WhenNoDigit() {
      // Act
      Action act = () => PasswordPolicy.EnsureIsValid("NoDigits!!AA");

      // Assert
      Assert.Throws<PasswordMissingDigitException>(act);
    }

    [Fact(DisplayName = "Throws missing-special-character exception when password has no special character")]
    public void PasswordPolicy_ThrowsPasswordMissingSpecialCharacterException_WhenNoSpecialCharacter() {
      // Act
      Action act = () => PasswordPolicy.EnsureIsValid("OnlyLetters123");

      // Assert
      Assert.Throws<PasswordMissingSpecialCharacterException>(act);
    }
  }
}
