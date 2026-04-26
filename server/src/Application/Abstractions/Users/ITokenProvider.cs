using Metaspesa.Domain.Users;

namespace Metaspesa.Application.Abstractions.Users;

public interface ITokenProvider {
  string GenerateToken(User user);
}
