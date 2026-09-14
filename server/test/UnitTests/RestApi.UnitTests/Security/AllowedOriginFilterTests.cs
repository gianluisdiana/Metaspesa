using Metaspesa.RestApi.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace Metaspesa.RestApi.UnitTests.Security;

public static class AllowedOriginFilterTests {
  [Fact(DisplayName = "Configured browser origin continues request")]
  public static async Task Invoke_AllowsConfiguredOrigin() {
    var filter = new AllowedOriginFilter(Options.Create(
      new BrowserSecurityOptions {
        AllowedOrigins = ["http://localhost:3000"],
      }));
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers.Origin = "http://localhost:3000";
    var invocationContext = new DefaultEndpointFilterInvocationContext(
      httpContext);
    static ValueTask<object?> next(EndpointFilterInvocationContext _) =>
      ValueTask.FromResult<object?>(Results.NoContent());

    object? result = await filter.InvokeAsync(invocationContext, next);

    Assert.IsType<NoContent>(result);
  }

  [Fact(DisplayName = "Missing browser origin is forbidden")]
  public static async Task Invoke_RejectsMissingOrigin() {
    var filter = new AllowedOriginFilter(Options.Create(
      new BrowserSecurityOptions {
        AllowedOrigins = ["http://localhost:3000"],
      }));
    var httpContext = new DefaultHttpContext();
    var invocationContext = new DefaultEndpointFilterInvocationContext(
      httpContext);
    static ValueTask<object?> next(EndpointFilterInvocationContext _) =>
      ValueTask.FromResult<object?>(Results.NoContent());

    object? result = await filter.InvokeAsync(invocationContext, next);

    Assert.Equal(
      StatusCodes.Status403Forbidden,
      Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false).StatusCode);
  }

  [Fact(DisplayName = "Unconfigured browser origin is forbidden")]
  public static async Task Invoke_RejectsUnconfiguredOrigin() {
    var filter = new AllowedOriginFilter(Options.Create(
      new BrowserSecurityOptions {
        AllowedOrigins = ["http://localhost:3000"],
      }));
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers.Origin = "http://malicious.example";
    var invocationContext = new DefaultEndpointFilterInvocationContext(
      httpContext);
    static ValueTask<object?> next(EndpointFilterInvocationContext _) =>
      ValueTask.FromResult<object?>(Results.NoContent());

    object? result = await filter.InvokeAsync(invocationContext, next);

    Assert.Equal(
      StatusCodes.Status403Forbidden,
      Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false).StatusCode);
  }
}