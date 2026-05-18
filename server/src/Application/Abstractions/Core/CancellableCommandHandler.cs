using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metaspesa.Application.Abstractions.Core;

public abstract partial class CancellableCommandHandler<TRequest>(
  IServiceScopeFactory scopeFactory,
  ILogger logger
) : ICommandHandler<TRequest> where TRequest : ICommand {

  public async Task<Result> Handle(
    TRequest command, CancellationToken cancellationToken = default
  ) {
    try {
      Result result = await ExecuteAsync(command, cancellationToken);
      if (!result.IsSuccess) {
        QueueRollback(command);
      }
      return result;
    } catch (OperationCanceledException) {
      QueueRollback(command);
      throw;
    }
  }

  protected abstract Task<Result> ExecuteAsync(
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