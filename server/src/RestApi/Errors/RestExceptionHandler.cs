using Metaspesa.Database.Exceptions;
using Metaspesa.Domain.Identity.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Metaspesa.RestApi.Errors;

internal sealed partial class RestExceptionHandler(
  IProblemDetailsService problemDetailsService,
  ILogger<RestExceptionHandler> logger
) : IExceptionHandler {
  public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken
  ) {
    (int status, string code, string title, string problemType) = exception switch {
      UsernameAlreadyExistsException identity => (
        StatusCodes.Status409Conflict,
        identity.Code,
        "Username already exists",
        "username-already-exists"),
      InvalidCredentialsException identity => (
        StatusCodes.Status401Unauthorized,
        identity.Code,
        "Invalid credentials",
        "invalid-credentials"),
      IdentityDomainException identity => (
        StatusCodes.Status400BadRequest,
        identity.Code,
        identity.Message,
        "identity-validation"),
      DatabaseException => (
        StatusCodes.Status500InternalServerError,
        "Server.DatabaseError",
        "Database error",
        "database-error"),
      _ => (
        StatusCodes.Status500InternalServerError,
        "Server.InternalError",
        "Internal server error",
        "internal-error"),
    };

    if (status >= StatusCodes.Status500InternalServerError) {
      LogRequestFailure(logger, exception);
    }

    httpContext.Response.StatusCode = status;
    await problemDetailsService.WriteAsync(new ProblemDetailsContext {
      HttpContext = httpContext,
      Exception = exception,
      ProblemDetails = new ProblemDetails {
        Type = $"https://metaspesa.app/problems/{problemType}",
        Title = title,
        Status = status,
        Extensions = {
          ["code"] = code,
          ["traceId"] = httpContext.TraceIdentifier,
        },
      },
    });
    return true;
  }

  [LoggerMessage(LogLevel.Error, "REST request failed")]
  private static partial void LogRequestFailure(
    ILogger logger, Exception exception);
}