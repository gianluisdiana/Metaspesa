using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Application.Identity;

public static class LoginUser {
  public record Query(string Username, string Password);

  public class Handler(
    IUserRepository userRepository,
    IHasher hasher,
    ITokenProvider tokenProvider
  ) {
    public async Task<Token> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(query);

      User user = await userRepository.GetUserAsync(
        new Username(query.Username), cancellationToken) ??
        throw new InvalidCredentialsException();

      PasswordHash hashedPassword = hasher.HashPassword(query.Password);

      user.EnsureHasSamePassword(hashedPassword);

      return tokenProvider.GenerateToken(user);
    }
  }
}