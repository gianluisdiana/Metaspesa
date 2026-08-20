namespace Metaspesa.Domain.SharedKernel.Errors;

public class InvalidUnitOfMeasureException : DomainException {
  public InvalidUnitOfMeasureException() { }

  public InvalidUnitOfMeasureException(string message) : base(message) { }

  public InvalidUnitOfMeasureException(string message, Exception innerException)
    : base(message, innerException) { }

  private InvalidUnitOfMeasureException(string? unit, string message)
    : base(message) {
    Unit = unit;
  }

  public string? Unit { get; }

  public static InvalidUnitOfMeasureException ForUnit(string? unit) =>
    new(unit, $"Unit of measure is invalid. Unit: {unit ?? "<null>"}");
}