using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Domain.UnitTests.Identity;

public static class UserIdTest {
  [Fact(DisplayName = "Creates user id from non-empty GUID")]
  public static void UserId_Created_WhenGuidIsNotEmpty() {
    // Arrange
    var value = Guid.CreateVersion7();

    // Act
    var userId = new UserId(value);

    // Assert
    Assert.Equal(value, userId.Value);
  }

  [Fact(DisplayName = "Throws specific exception when GUID is empty")]
  public static void UserId_ThrowsInvalidUserIdException_WhenGuidIsEmpty() {
    // Act & Assert
    Assert.Throws<InvalidUserIdException>(() => new UserId(Guid.Empty));
  }

  [Fact(DisplayName = "Two user ids with the same value are equal")]
  public static void UserId_Equal_WhenSameValue() {
    // Arrange
    var value = Guid.CreateVersion7();

    // Act & Assert
    Assert.Equal(new UserId(value), new UserId(value));
  }
}
