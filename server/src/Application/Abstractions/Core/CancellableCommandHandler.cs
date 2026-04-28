using Microsoft.Extensions.DependencyInjection;

namespace Metaspesa.Application.Abstractions.Core;

public abstract class CancellableCommandHandler<TRequest>(
  IServiceScopeFactory scopeFactory
) : ICommandHandler<TRequest> where TRequest : ICommand {

  public async Task<Result> Handle(
    TRequest command, CancellationToken cancellationToken = default
  ) {
    try {
      Result result = await ExecuteAsync(command, cancellationToken);
      if (!result.IsSuccess) {
        FireRollback(command);
      }
      return result;
    } catch (OperationCanceledException) {
      FireRollback(command);
      throw;
    }
  }

  protected abstract Task<Result> ExecuteAsync(
    TRequest command, CancellationToken cancellationToken);

  protected abstract Task RollbackAsync(
    TRequest command, IServiceProvider services, CancellationToken cancellationToken
  );

  private void FireRollback(TRequest command) {
    _ = Task.Run(async () => {
      using IServiceScope scope = scopeFactory.CreateScope();
      await RollbackAsync(command, scope.ServiceProvider, CancellationToken.None);
    });
  }
}
