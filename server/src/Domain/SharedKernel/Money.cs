using Metaspesa.Domain.SharedKernel.Errors;

namespace Metaspesa.Domain.SharedKernel;

public readonly record struct Money {
  private const int DecimalPlaces = 2;

  public decimal Amount { get; }

  public Money(decimal amount) {
    if (amount < 0) {
      throw new InvalidMoneyAmountException(amount);
    }

    Amount = Math.Round(amount, DecimalPlaces, MidpointRounding.AwayFromZero);
  }
}