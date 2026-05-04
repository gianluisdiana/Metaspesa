using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Metaspesa.Database;

public static class DatabaseDependencyInjection {
  public static IServiceCollection AddDatabase(
    this IServiceCollection services
  ) {
    string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") ??
      throw new InvalidOperationException(
        "Connection string not found in environment variables or configuration.");
    services.AddDbContextPool<MainContext>(options =>
      options.UseNpgsql(connectionString));

    services.AddOpenTelemetry()
      .WithTracing(tracing => tracing.AddNpgsql());

    return services;
  }

  public static IServiceCollection AddPersistence(
    this IServiceCollection services
  ) {
    services.AddDatabase();

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MainContext>());

    services.AddScoped<IUserRepository, PostgreSqlUserRepository>();
    services.AddScoped<IMarketRepository, PostgreSqlMarketRepository>();
    services.AddScoped<IProductRepository, PostgreSqlProductRepository>();
    services.AddScoped<IShoppingRepository, PostgreSqlShoppingRepository>();

    return services;
  }
}