namespace Metaspesa.Application.Abstractions.Users;

public interface IHasher {
  string Hash(string value);
  bool VerifyHash(string plainValue, string hashedValue);
}
