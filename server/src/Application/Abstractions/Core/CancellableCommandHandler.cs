using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metaspesa.Application.Abstractions.Core;

public abstract partial class CancellableCommandHandler<TRequest>(
  IServiceScopeFactory scopeFactory,
  ILogger logger
) {

  public async Task Handle(
    TRequest command, CancellationToken cancellationToken = default
  ) {
    try {
      await ExecuteAsync(command, cancellationToken);
    } catch {
      QueueRollback(command);
      throw;
    }
  }

  protected abstract Task ExecuteAsync(
    TRequest command, CancellationToken cancellationToken);

  protected abstract Task RollbackAsync(
    TRequest command, IServiceProvider services, CancellationToken cancellationToken
  );

  protected virtual bool HasRollbackWork => true;

  private void QueueRollback(TRequest command) {
    if (!HasRollbackWork) {
      return;
    }

    _ = Task.Run(async () => {
      try {
        using IServiceScope scope = scopeFactory.CreateScope();
        await RollbackAsync(command, scope.ServiceProvider, CancellationToken.None);
      } catch (Exception ex) when (
        ex is InvalidOperationException ||
        ex is OperationCanceledException ||
        ex is TimeoutException
      ) {
        LogBackgroundRollbackFailed(typeof(TRequest).Name, ex);
      }
    });
  }

  [LoggerMessage(
    LogLevel.Error,
    "Background rollback failed for command {CommandType}")]
  private partial void LogBackgroundRollbackFailed(
    string commandType, Exception ex);
}