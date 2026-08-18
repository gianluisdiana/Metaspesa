using Metaspesa.Domain.SharedKernel.Errors;

namespace Metaspesa.Domain.Markets.Errors;

public abstract class MarketDomainException : DomainException {
  protected MarketDomainException() { }

  protected MarketDomainException(string message) : base(message) { }

  protected MarketDomainException(string message, Exception innerException)
    : base(message, innerException) { }

  protected MarketDomainException(string code, string message) : base(message) {
    Code = code;
  }

  protected MarketDomainException(string code, string message, Exception innerException)
    : base(message, innerException) {
    Code = code;
  }

  public string Code { get; } = "Market.Error";
}
