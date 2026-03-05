using System.Diagnostics;
using Metaspesa.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Metaspesa.MigrationService;

internal class Worker(
  IServiceProvider serviceProvider,
  IHostApplicationLifetime hostApplicationLifetime
) : BackgroundService {
  internal const string ActivitySourceName = "MigrationService";
  private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    using Activity? activity = ActivitySource.StartActivity(ActivityKind.Client);

    try {
      using IServiceScope scope = serviceProvider.CreateScope();
      MainContext dbContext = scope.ServiceProvider.GetRequiredService<MainContext>();

      IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();
      await strategy.ExecuteAsync(async () =>
        await dbContext.Database.MigrateAsync(stoppingToken));
    } catch (Exception ex) {
      activity?.AddException(ex);
      throw;
    }

    hostApplicationLifetime.StopApplication();
  }
}