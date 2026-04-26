using Metaspesa.Domain.Users;

namespace Metaspesa.Domain.UnitTests.Users;

public static class PasswordPolicyTest {
  public class HasMinimumLength {
    [Theory(DisplayName = "Returns true when password meets minimum length")]
    [InlineData("Abc!12345X")]
    [InlineData("Abc!12345Xextra")]
    public void PasswordPolicy_HasMinimumLength_ReturnsTrue_WhenMeetsMinimumLength(string password) {
      // Act
      bool result = PasswordPolicy.HasMinimumLength(password);

      // Assert
      Assert.True(result);
    }

    [Theory(DisplayName = "Returns false when password is shorter than minimum length")]
    [InlineData("")]
    [InlineData("Abc!1234")]
    [InlineData("Abc!12345")]
    public void PasswordPolicy_HasMinimumLength_ReturnsFalse_WhenShorterThanMinimum(string password) {
      // Act
      bool result = PasswordPolicy.HasMinimumLength(password);

      // Assert
      Assert.False(result);
    }
  }

  public class HasUppercase {
    [Theory(DisplayName = "Returns true when password contains uppercase letter")]
    [InlineData("Abc123!@#")]
    [InlineData("abcAbc")]
    public void PasswordPolicy_HasUppercase_ReturnsTrue_WhenContainsUppercase(string password) {
      // Act
      bool result = PasswordPolicy.HasUppercase(password);

      // Assert
      Assert.True(result);
    }

    [Theory(DisplayName = "Returns false when password has no uppercase letter")]
    [InlineData("abc123!@#")]
    [InlineData("alllowercase")]
    public void PasswordPolicy_HasUppercase_ReturnsFalse_WhenNoUppercase(string password) {
      // Act
      bool result = PasswordPolicy.HasUppercase(password);

      // Assert
      Assert.False(result);
    }
  }

  public class HasLowercase {
    [Theory(DisplayName = "Returns true when password contains lowercase letter")]
    [InlineData("ABCabc123")]
    [InlineData("ABCd")]
    public void PasswordPolicy_HasLowercase_ReturnsTrue_WhenContainsLowercase(string password) {
      // Act
      bool result = PasswordPolicy.HasLowercase(password);

      // Assert
      Assert.True(result);
    }

    [Theory(DisplayName = "Returns false when password has no lowercase letter")]
    [InlineData("ABC123!@#")]
    [InlineData("ALLUPPERCASE")]
    public void PasswordPolicy_HasLowercase_ReturnsFalse_WhenNoLowercase(string password) {
      // Act
      bool result = PasswordPolicy.HasLowercase(password);

      // Assert
      Assert.False(result);
    }
  }

  public class HasDigit {
    [Theory(DisplayName = "Returns true when password contains a digit")]
    [InlineData("Abcdef1!")]
    [InlineData("NoDigit9")]
    public void PasswordPolicy_HasDigit_ReturnsTrue_WhenContainsDigit(string password) {
      // Act
      bool result = PasswordPolicy.HasDigit(password);

      // Assert
      Assert.True(result);
    }

    [Theory(DisplayName = "Returns false when password has no digit")]
    [InlineData("Abcdef!@#")]
    [InlineData("NoDigitHere!")]
    public void PasswordPolicy_HasDigit_ReturnsFalse_WhenNoDigit(string password) {
      // Act
      bool result = PasswordPolicy.HasDigit(password);

      // Assert
      Assert.False(result);
    }
  }

  public class HasSpecialCharacter {
    [Theory(DisplayName = "Returns true when password contains a special character")]
    [InlineData("Abc123!")]
    [InlineData("Abc123@")]
    [InlineData("Abc123 ")]
    public void PasswordPolicy_HasSpecialCharacter_ReturnsTrue_WhenContainsSpecialChar(string password) {
      // Act
      bool result = PasswordPolicy.HasSpecialCharacter(password);

      // Assert
      Assert.True(result);
    }

    [Theory(DisplayName = "Returns false when password has no special character")]
    [InlineData("Abc12345")]
    [InlineData("OnlyAlphanumeric99")]
    public void PasswordPolicy_HasSpecialCharacter_ReturnsFalse_WhenNoSpecialChar(string password) {
      // Act
      bool result = PasswordPolicy.HasSpecialCharacter(password);

      // Assert
      Assert.False(result);
    }
  }
}
