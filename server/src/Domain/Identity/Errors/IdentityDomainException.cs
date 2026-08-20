using Metaspesa.Domain.SharedKernel.Errors;

namespace Metaspesa.Domain.Identity.Errors;

public abstract class IdentityDomainException : DomainException {
  protected IdentityDomainException() { }

  protected IdentityDomainException(string message) : base(message) { }

  protected IdentityDomainException(string message, Exception innerException)
    : base(message, innerException) { }

  protected IdentityDomainException(string code, string message) : base(message) {
    Code = code;
  }

  protected IdentityDomainException(string code, string message, Exception innerException)
    : base(message, innerException) {
    Code = code;
  }

  public string Code { get; } = "Identity.Error";
}