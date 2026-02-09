namespace Metaspesa.Application.Abstractions.Core;

public record DomainError(
  string Code, string Description, ErrorKind Kind, Exception? Exception = null
);