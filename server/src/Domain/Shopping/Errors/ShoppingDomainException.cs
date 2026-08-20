using Metaspesa.Domain.SharedKernel.Errors;

namespace Metaspesa.Domain.Shopping.Errors;

public abstract class ShoppingDomainException : DomainException {
  protected ShoppingDomainException() { }
  protected ShoppingDomainException(string message) : base(message) { }
  protected ShoppingDomainException(string message, Exception innerException)
    : base(message, innerException) { }
  protected ShoppingDomainException(string code, string message) : base(message) {
    Code = code;
  }
  protected ShoppingDomainException(
    string code, string message, Exception innerException
  ) : base(message, innerException) {
    Code = code;
  }

  public string Code { get; } = "Shopping.Error";
}