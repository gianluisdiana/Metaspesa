using System.Collections.Immutable;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;

namespace Metaspesa.Application.Extensions;

internal static class ValidationResultExtensions {
  extension(ValidationResult result) {
    public ImmutableList<DomainError> ToDomainErrors() => result.Errors
      .OrderBy(f => f.Severity)
      .Select(f => new DomainError(f.ErrorCode, f.ErrorMessage, ErrorKind.Validation))
      .ToImmutableList();
  }
}