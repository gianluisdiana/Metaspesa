namespace Metaspesa.Domain.SharedKernel.Errors;

public class InvalidPositiveAmountException : DomainException {
  public InvalidPositiveAmountException() { }

  public InvalidPositiveAmountException(int value)
    : base($"Amount must be greater than zero. Received '{value}'.") { }

  public InvalidPositiveAmountException(string message) : base(message) { }

  public InvalidPositiveAmountException(string message, Exception innerException)
    : base(message, innerException) { }
}
