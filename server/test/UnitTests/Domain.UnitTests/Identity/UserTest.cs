using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Domain.UnitTests.Identity;

public static class UserTest {
  [Fact(DisplayName = "Creates shopper user")]
  public static void User_CreatedAsShopper_WhenValuesAreValid() {
    // Act
    var user = User.CreateShopper(
      new UserId(Guid.CreateVersion7()),
      new Username("estela"),
      new PasswordHash("hashed"));

    // Assert
    Assert.Equal(Role.Shopper, user.Role);
  }

  [Fact(DisplayName = "Throws an exception when it doesn't have the same password")]
  public static void EnsureHasSamePassword_ThrowsException_WhenPasswordsAreDifferent() {
    // Arrange
    var user = User.CreateShopper(
      new UserId(Guid.CreateVersion7()),
      new Username("estela"),
      new PasswordHash("hashed"));

    // Act & Assert
    void act() => user.EnsureHasSamePassword(new PasswordHash("different"));

    Assert.Throws<InvalidCredentialsException>(act);
  }

  [Fact(DisplayName = "Does not throw an exception when it has the same password")]
  public static void EnsureHasSamePassword_DoesNotThrowException_WhenPasswordsAreTheSame() {
    // Arrange
    var user = User.CreateShopper(
      new UserId(Guid.CreateVersion7()),
      new Username("estela"),
      new PasswordHash("hashed"));

    // Act
    Exception? exception = Record.Exception(() =>
      user.EnsureHasSamePassword(new PasswordHash("hashed")));

    // Assert
    Assert.Null(exception);
  }
}