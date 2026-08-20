using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Domain.UnitTests.Identity;

public static class UsernameTest {
  [Fact(DisplayName = "Creates username from non-empty value")]
  public static void Username_Created_WhenValueIsNotEmpty() {
    // Act
    var username = new Username("estela");

    // Assert
    Assert.Equal("estela", username.Value);
  }

  [Theory(DisplayName = "Throws specific exception when username is invalid")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData(" ")]
  public static void Username_ThrowsInvalidUsernameException_WhenValueIsInvalid(
    string? value
  ) {
    // Act & Assert
    Assert.Throws<InvalidUsernameException>(() => new Username(value!));
  }

  [Fact(DisplayName = "Two usernames with the same value are equal")]
  public static void Username_Equal_WhenSameValue() {
    // Act & Assert
    Assert.Equal(new Username("estela"), new Username("estela"));
  }
}