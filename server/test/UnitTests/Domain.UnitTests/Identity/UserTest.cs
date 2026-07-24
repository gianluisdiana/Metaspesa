using Metaspesa.Domain.Identity;

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
}