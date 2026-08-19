using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Application.Identity;

public static class RegisterUser {
  public record Command(string Username, string Password);

  public class Handler(
    IHasher hasher,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
  ) {
    public async Task Handle(
      Command command, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(command);

      PasswordPolicy.EnsureIsValid(command.Password);

      var username = new Username(command.Username);
      if (await userRepository.CheckUsernameExistsAsync(username, cancellationToken)) {
        throw new UsernameAlreadyExistsException(command.Username);
      }

      UserId id = new(Guid.CreateVersion7());
      string hashedPassword = hasher.Hash(command.Password);
      var user = User.CreateShopper(id, username, new PasswordHash(hashedPassword));

      userRepository.SaveUser(user);
      await unitOfWork.SaveChangesAsync(cancellationToken);
    }
  }
}