using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Domain.Users;

namespace Metaspesa.Application.Auth;

public static class LoginUser {
  public record Query(string Username, string Password) : IQuery<Token>;

  internal class Handler(
    IUserRepository userRepository,
    IHasher hasher,
    ITokenProvider tokenProvider
  ) : IQueryHandler<Query, Token> {
    public async Task<Result<Token>> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      User? user = await userRepository.GetUserByUsernameAsync(query.Username, cancellationToken);

      if (user is null || !hasher.VerifyHash(query.Password, user.HashedPassword)) {
        return new DomainError(
          "User.InvalidCredentials",
          "Invalid username or password.",
          ErrorKind.Unauthenticated);
      }

      return tokenProvider.GenerateToken(user);
    }
  }
}
