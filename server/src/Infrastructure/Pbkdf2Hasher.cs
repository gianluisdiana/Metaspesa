using System.Diagnostics;
using Metaspesa.Application.Abstractions.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Metaspesa.Infrastructure;

internal partial class Pbkdf2Hasher(
  ILogger<Pbkdf2Hasher> logger
) : IHasher {
  private readonly PasswordHasher<string> _hasher = new();

  public string Hash(string value) {
    Debug.Assert(value is not null);

    return _hasher.HashPassword(string.Empty, value);
  }

  public bool VerifyHash(string plainValue, string hashedValue) {
    Debug.Assert(plainValue is not null);
    Debug.Assert(hashedValue is not null);

    PasswordVerificationResult result = _hasher.VerifyHashedPassword(
      string.Empty, hashedValue, plainValue);

    if (result is PasswordVerificationResult.SuccessRehashNeeded) {
      LogPasswordRehashNeeded();
    }

    return result is not PasswordVerificationResult.Failed;
  }

  [LoggerMessage(
    LogLevel.Warning,
    "Password hash needs rehashing.")]
  private partial void LogPasswordRehashNeeded();
}
