using Metaspesa.Domain.Purchasing;

namespace Metaspesa.Application.Abstractions.Purchasing;

public interface IPurchaseRepository {
  Task<int> AddAsync(Purchase purchase, CancellationToken cancellationToken);
}