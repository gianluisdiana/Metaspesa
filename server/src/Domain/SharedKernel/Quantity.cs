using Metaspesa.Domain.SharedKernel.Errors;

namespace Metaspesa.Domain.SharedKernel;

public readonly record struct Quantity {
  public decimal Amount { get; }
  public UnitOfMeasure UnitOfMeasure { get; }

  public Quantity(decimal amount, UnitOfMeasure unitOfMeasure) {
    if (amount <= 0) {
      throw new InvalidQuantityAmountException(amount);
    }

    Amount = amount;
    UnitOfMeasure = unitOfMeasure;
  }
}