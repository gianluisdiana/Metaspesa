using Grpc.Core;
using Grpc.Core.Interceptors;
using Metaspesa.Database.Exceptions;
using Metaspesa.Domain.Identity.Errors;
using Metaspesa.Domain.Markets.Errors;
using Metaspesa.Domain.Purchasing.Errors;
using Metaspesa.Domain.Shopping.Errors;
using Metaspesa.GrpcApi.Extensions;

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
    } catch (ArgumentOutOfRangeException ex) {
      LogInvalidArgument(context.Method, ex);
      throw new RpcException(new Status(
        StatusCode.InvalidArgument,
        "invalid argument"));
    } catch (IdentityDomainException ex) {
      LogIdentityDomainException(context.Method, ex);
      throw ex.ToRpcException();
    } catch (MarketDomainException ex) {
      LogMarketDomainException(context.Method, ex);
      throw ex.ToRpcException();
    } catch (ShoppingDomainException ex) {
      LogShoppingDomainException(context.Method);
      throw ex.ToRpcException();
    } catch (PurchaseDomainException ex) {
      LogPurchaseDomainException(context.Method);
      throw ex.ToRpcException();
    } catch (Exception ex) {
      LogUnhandledException(context.Method, ex);
      throw new RpcException(new Status(StatusCode.Internal, "internal server error"));
    }
  }

  [LoggerMessage(LogLevel.Information, "Request cancelled while handling {Method}")]
  private partial void LogRequestCancelled(string method, Exception ex);

  [LoggerMessage(LogLevel.Error, "Database exception while handling {Method}")]
  private partial void LogDatabaseException(string method, Exception ex);

  [LoggerMessage(LogLevel.Information, "Invalid argument while handling {Method}")]
  private partial void LogInvalidArgument(string method, Exception ex);

  [LoggerMessage(LogLevel.Error, "Identity domain exception while handling {Method}")]
  private partial void LogIdentityDomainException(string method, IdentityDomainException ex);

  [LoggerMessage(LogLevel.Error, "Market domain exception while handling {Method}")]
  private partial void LogMarketDomainException(string method, MarketDomainException ex);

  [LoggerMessage(LogLevel.Error, "Shopping domain exception while handling {Method}")]
  private partial void LogShoppingDomainException(string method);

  [LoggerMessage(LogLevel.Error, "Purchasing domain exception while handling {Method}")]
  private partial void LogPurchaseDomainException(string method);

  [LoggerMessage(LogLevel.Error, "Unhandled exception while handling {Method}")]
  private partial void LogUnhandledException(string method, Exception ex);
}