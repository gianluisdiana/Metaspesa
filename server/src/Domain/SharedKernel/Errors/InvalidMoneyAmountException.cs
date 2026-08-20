namespace Metaspesa.Domain.SharedKernel.Errors;

public class InvalidMoneyAmountException : DomainException {
  public InvalidMoneyAmountException() { }

  public InvalidMoneyAmountException(string message) : base(message) { }

  public InvalidMoneyAmountException(string message, Exception innerException)
    : base(message, innerException) { }

  public InvalidMoneyAmountException(decimal amount)
    : base($"Money amount must be non-negative. Amount: {amount}") {
    Amount = amount;
  }

  public decimal Amount { get; }
}