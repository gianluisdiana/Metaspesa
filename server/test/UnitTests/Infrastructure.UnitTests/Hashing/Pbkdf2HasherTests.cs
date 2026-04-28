using Microsoft.Extensions.Logging.Abstractions;

namespace Metaspesa.Infrastructure.UnitTests.Hashing;

public static class Pbkdf2HasherTests {
  private static Pbkdf2Hasher CreateHasher() =>
    new(NullLogger<Pbkdf2Hasher>.Instance);

  public class Hash {
    private readonly Pbkdf2Hasher _hasher = CreateHasher();

    [Fact(DisplayName = "Returns a value different from the input")]
    public void Hash_ReturnsDifferentValue_ThanInput() {
      // Act
      string result = _hasher.Hash("mypassword");

      // Assert
      Assert.NotEqual("mypassword", result);
    }

    [Fact(DisplayName = "Returns a non-empty string")]
    public void Hash_ReturnsNonEmptyString() {
      // Act
      string result = _hasher.Hash("anything");

      // Assert
      Assert.NotEmpty(result);
    }

    [Fact(DisplayName = "Produces unique hashes for the same input on consecutive calls")]
    public void Hash_ProducesUniqueHashes_ForSameInput() {
      // Act
      string first = _hasher.Hash("password");
      string second = _hasher.Hash("password");

      // Assert
      Assert.NotEqual(first, second);
    }
  }

  public class VerifyHash {
    private readonly Pbkdf2Hasher _hasher = CreateHasher();

    [Fact(DisplayName = "Returns true when plain value matches the hash")]
    public void VerifyHash_ReturnsTrue_WhenHashMatches() {
      // Arrange
      string hash = _hasher.Hash("secret");

      // Act
      bool result = _hasher.VerifyHash("secret", hash);

      // Assert
      Assert.True(result);
    }

    [Fact(DisplayName = "Returns false when plain value does not match the hash")]
    public void VerifyHash_ReturnsFalse_WhenPasswordDiffers() {
      // Arrange
      string hash = _hasher.Hash("correct");

      // Act
      bool result = _hasher.VerifyHash("wrong", hash);

      // Assert
      Assert.False(result);
    }

    [Fact(DisplayName = "Returns false when hash belongs to a different value")]
    public void VerifyHash_ReturnsFalse_WhenHashIsFromDifferentValue() {
      // Arrange
      string hashOfOther = _hasher.Hash("other-value");

      // Act
      bool result = _hasher.VerifyHash("my-value", hashOfOther);

      // Assert
      Assert.False(result);
    }

    [Fact(DisplayName = "Returns true for any value hashed and immediately verified")]
    public void VerifyHash_ReturnsTrue_ForRoundTrip() {
      // Arrange
      const string Plain = "round-trip-test-value";
      string hash = _hasher.Hash(Plain);

      // Act
      bool result = _hasher.VerifyHash(Plain, hash);

      // Assert
      Assert.True(result);
    }
  }
}
