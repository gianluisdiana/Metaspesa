using Metaspesa.Domain.Users;

namespace Metaspesa.Application.Abstractions.Users;

public interface ITokenProvider {
  Token GenerateToken(User user);
}
