using Metaspesa.Application.Abstractions.Core;

namespace Metaspesa.Database.Fake;

internal class FakeUnitOfWork : IUnitOfWork {
  public Task<int> SaveChangesAsync(
    CancellationToken cancellationToken) {
    return Task.FromResult(0);
  }
}
