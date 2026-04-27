using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Extensions;
using Metaspesa.Domain.Users;

namespace Metaspesa.Application.Auth;

public static class RegisterUser {
  public record Command(string Username, string Password) : ICommand;

  internal class Handler(
    IValidator<Command> validator,
    IHasher hasher,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
  ) : ICommandHandler<Command> {
    public async Task<Result> Handle(
      Command command, CancellationToken cancellationToken = default
    ) {
      ValidationResult validationResult = await validator.ValidateAsync(command, cancellationToken);
      if (!validationResult.IsValid) {
        return validationResult.ToDomainErrors();
      }

      string hashedPassword = hasher.Hash(command.Password);
      User user = new(
        Uid: Guid.CreateVersion7(),
        Username: command.Username,
        HashedPassword: hashedPassword,
        Role: Role.Shopper
      );

      userRepository.SaveUser(user);
      await unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success();
    }
  }

  internal class Validator : AbstractValidator<Command> {
    public Validator(IUserRepository userRepository) {
      RuleFor(x => x.Username)
        .NotEmpty()
        .WithErrorCode("User.UsernameEmpty");

      RuleFor(x => x.Password)
        .Must(PasswordPolicy.HasMinimumLength)
        .WithMessage($"Password must be at least {PasswordPolicy.MinimumLength} characters.")
        .WithErrorCode("User.PasswordTooShort");

      RuleFor(x => x.Password)
        .Must(PasswordPolicy.HasUppercase)
        .WithMessage("Password must contain at least one uppercase letter.")
        .WithErrorCode("User.PasswordMissingUppercase");

      RuleFor(x => x.Password)
        .Must(PasswordPolicy.HasLowercase)
        .WithMessage("Password must contain at least one lowercase letter.")
        .WithErrorCode("User.PasswordMissingLowercase");

      RuleFor(x => x.Password)
        .Must(PasswordPolicy.HasDigit)
        .WithMessage("Password must contain at least one digit.")
        .WithErrorCode("User.PasswordMissingDigit");

      RuleFor(x => x.Password)
        .Must(PasswordPolicy.HasSpecialCharacter)
        .WithMessage("Password must contain at least one special character.")
        .WithErrorCode("User.PasswordMissingSpecialChar");

      RuleFor(x => x.Username)
        .MustAsync(async (username, ct) =>
          !await userRepository.CheckUsernameExistsAsync(username, ct))
        .WithMessage(x => $"Username '{x.Username}' is already taken.")
        .WithErrorCode("User.UsernameAlreadyExists")
        .WithState(_ => ErrorKind.Conflict);
    }
  }
}
