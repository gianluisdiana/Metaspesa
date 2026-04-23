using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;

namespace Metaspesa.Application.Extensions;

internal static class ValidationFailureExtensions {
  extension(ValidationFailure failure) {
    public DomainError ToDomainError() => new(
      Code: failure.ErrorCode,
      Description: failure.ErrorMessage,
      Kind: failure.ErrorKind);

    private ErrorKind ErrorKind =>
      failure.CustomState is ErrorKind kind ? kind : ErrorKind.Validation;
  }
}