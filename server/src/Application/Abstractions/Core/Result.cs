using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metaspesa.Application.Abstractions.Core;

public class Result {
  public bool IsSuccess => Errors.Count == 0;

  public IReadOnlyCollection<DomainError> Errors { get; private init; }

  protected Result(IReadOnlyCollection<DomainError> errors) => Errors = errors;

  public static implicit operator Result(DomainError error) => new([error]);
  public static implicit operator Result(ImmutableList<DomainError> errors) =>
    new(errors);

  public static Result Success() => new([]);
}

public class Result<TValue> : Result where TValue : notnull {
  [AllowNull]
  public TValue Value => field ??
    throw new InvalidOperationException("The value of a failure result can not be accessed.");

  private Result(TValue? value, IReadOnlyCollection<DomainError> errors) : base(errors) =>
    Value = value;

  public static implicit operator Result<TValue>(TValue value) =>
    new(value, []);
  public static implicit operator Result<TValue>(DomainError error) =>
    new(default, [error]);
}