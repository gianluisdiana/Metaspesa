using Metaspesa.Domain.Identity;

namespace Metaspesa.Application.Abstractions.Users;

public interface ITokenProvider {
  Token GenerateToken(User user);
}