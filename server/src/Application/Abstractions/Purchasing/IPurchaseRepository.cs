using Metaspesa.Domain.Purchasing;

namespace Metaspesa.Application.Abstractions.Purchasing;

public interface IPurchaseRepository {
  void Add(Purchase purchase);
}