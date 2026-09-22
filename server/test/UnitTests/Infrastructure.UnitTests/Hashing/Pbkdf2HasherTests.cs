using Microsoft.Extensions.Logging.Abstractions;

namespace Metaspesa.Infrastructure.UnitTests.Hashing;

public static class Pbkdf2HasherTests {
  public class Hash {
    private readonly Pbkdf2Hasher _hasher = new(NullLogger<Pbkdf2Hasher>.Instance);

    [Fact(DisplayName = "Returns a value different from the input")]
    public void Hash_ReturnsDifferentValue_ThanInput() {
      // Act
      string result = _hasher.HashPassword("mypassword").Value;

      // Assert
      Assert.NotEqual("mypassword", result);
    }

    [Fact(DisplayName = "Returns a non-empty string")]
    public void Hash_ReturnsNonEmptyString() {
      // Act
      string result = _hasher.HashPassword("anything").Value;

      // Assert
      Assert.NotEmpty(result);
    }

    [Fact(DisplayName = "Produces unique hashes for the same input on consecutive calls")]
    public void Hash_ProducesUniqueHashes_ForSameInput() {
      // Act
      string first = _hasher.HashPassword("password").Value;
      string second = _hasher.HashPassword("password").Value;

      // Assert
      Assert.NotEqual(first, second);
    }
  }
}