namespace Metaspesa.Application.Abstractions.Core;

public interface IUnitOfWork {
  Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}