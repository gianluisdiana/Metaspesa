using Metaspesa.Domain.Purchasing;

namespace Metaspesa.Application.Abstractions.Purchasing;

public interface IPurchaseRepository {
  Task<Guid> AddAsync(Purchase purchase, CancellationToken cancellationToken);
}