using System.Diagnostics;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Domain.Identity;
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

  public PasswordHash HashPassword(string password) {
    string hash = _hasher.HashPassword(string.Empty, password);
    return new PasswordHash(hash);
  }

  [LoggerMessage(
    LogLevel.Warning,
    "Password hash needs rehashing.")]
  private partial void LogPasswordRehashNeeded();
}