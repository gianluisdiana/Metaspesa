using System.Threading.RateLimiting;
using Grpc.Core;
using Metaspesa.GrpcApi.Extensions;
using Metaspesa.GrpcApi.Interceptors;
using Metaspesa.GrpcApi.Services;

namespace Metaspesa.GrpcApi;

internal static class ServiceCollectionExtensions {
  public static IServiceCollection AddGrpcApi(this IServiceCollection services) {
    services.AddGrpc(options => options.Interceptors.Add<ExceptionInterceptor>());
    services.AddGrpcReflection();

    services.AddRateLimiter(options => {
      options.OnRejected = (context, _) => {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.GrpcStatus = $"{(int)StatusCode.ResourceExhausted}";
        context.HttpContext.Response.Headers.GrpcMessage = "rate limit exceeded";

        return ValueTask.CompletedTask;
      };

      options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
          context.GetPartitionKey(),
          _ => new FixedWindowRateLimiterOptions {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
          }));

      options.AddPolicy("market", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
          context.GetPartitionKey(),
          _ => new SlidingWindowRateLimiterOptions {
            PermitLimit = 300,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueLimit = 0,
            AutoReplenishment = true
          }));
    });

    return services;
  }
}

internal static class WebApplicationExtensions {
  public static WebApplication AddCustomMiddleware(this WebApplication app) {
    app.UseAuthentication();
    app.UseRateLimiter();
    app.UseAuthorization();

    return app;
  }

  public static WebApplication MapGrpcServices(this WebApplication app) {
    app.MapGrpcService<AuthGrpcService>();
    app.MapGrpcService<ShoppingGrpcService>();
    app.MapGrpcService<MarketGrpcService>().RequireRateLimiting("market");
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    return app;
  }
}