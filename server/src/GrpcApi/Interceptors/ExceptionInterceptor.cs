using Grpc.Core;
using Grpc.Core.Interceptors;
using Metaspesa.Database.Exceptions;

namespace Metaspesa.GrpcApi.Interceptors;

internal partial class ExceptionInterceptor(
  ILogger<ExceptionInterceptor> logger
) : Interceptor {
  public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
    TRequest request,
    ServerCallContext context,
    UnaryServerMethod<TRequest, TResponse> continuation
  ) {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(continuation);

    try {
      return await continuation(request, context);
    } catch (RpcException) {
      throw;
    } catch (OperationCanceledException ex) when (
      context.CancellationToken.IsCancellationRequested
    ) {
      LogRequestCancelled(context.Method, ex);
      throw new RpcException(new Status(StatusCode.Cancelled, "request cancelled"));
    } catch (DatabaseException ex) {
      LogDatabaseException(context.Method, ex);
      throw new RpcException(new Status(StatusCode.Internal, "database error"));
    } catch (Exception ex) {
      LogUnhandledException(context.Method, ex);
      throw new RpcException(new Status(StatusCode.Internal, "internal server error"));
    }
  }

  [LoggerMessage(LogLevel.Information, "Request cancelled while handling {Method}")]
  private partial void LogRequestCancelled(string method, Exception ex);

  [LoggerMessage(LogLevel.Error, "Database exception while handling {Method}")]
  private partial void LogDatabaseException(string method, Exception ex);

  [LoggerMessage(LogLevel.Error, "Unhandled exception while handling {Method}")]
  private partial void LogUnhandledException(string method, Exception ex);
}
