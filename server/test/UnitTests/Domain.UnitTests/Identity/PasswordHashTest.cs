using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Domain.UnitTests.Identity;

public static class PasswordHashTest {
  [Fact(DisplayName = "Creates password hash from non-empty value")]
  public static void PasswordHash_Created_WhenValueIsNotEmpty() {
    // Act
    var passwordHash = new PasswordHash("hashed");

    // Assert
    Assert.Equal("hashed", passwordHash.Value);
  }

  [Theory(DisplayName = "Throws specific exception when password hash is invalid")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData(" ")]
  public static void PasswordHash_ThrowsInvalidPasswordHashException_WhenValueIsInvalid(
    string? value
  ) {
    // Act & Assert
    Assert.Throws<InvalidPasswordHashException>(() => new PasswordHash(value!));
  }

  [Fact(DisplayName = "Two password hashes with the same value are equal")]
  public static void PasswordHash_Equal_WhenSameValue() {
    // Act & Assert
    Assert.Equal(new PasswordHash("hashed"), new PasswordHash("hashed"));
  }
}