namespace Metaspesa.Domain.Users;

public record User(string Username, string EncryptedPassword, Role Role);
