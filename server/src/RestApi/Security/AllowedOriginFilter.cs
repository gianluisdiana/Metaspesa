using Microsoft.Extensions.Options;

namespace Metaspesa.RestApi.Security;

internal sealed class AllowedOriginFilter(
  IOptions<BrowserSecurityOptions> options
) : IEndpointFilter {
  public async ValueTask<object?> InvokeAsync(
    EndpointFilterInvocationContext context,
    EndpointFilterDelegate next
  ) {
    string? origin = context.HttpContext.Request.Headers.Origin;
    bool isAllowed = origin is not null && options.Value.AllowedOrigins.Contains(
      origin, StringComparer.OrdinalIgnoreCase);
    if (isAllowed) {
      return await next(context);
    }

    return Results.Problem(
      type: "https://metaspesa.app/problems/origin-not-allowed",
      title: "Request origin is not allowed",
      statusCode: StatusCodes.Status403Forbidden,
      extensions: new Dictionary<string, object?> {
        ["code"] = "Request.OriginNotAllowed",
        ["traceId"] = context.HttpContext.TraceIdentifier,
      });
  }
}