using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Database.Fake;
using Microsoft.Extensions.DependencyInjection;

namespace Metaspesa.Database;

public static class DatabaseDependencyInjection {
  public static IServiceCollection AddDatabase(
    this IServiceCollection services
  ) {
    services.AddScoped<IUnitOfWork, FakeUnitOfWork>();
    services.AddScoped<IProductRepository, FakeProductRepository>();
    services.AddScoped<IShoppingRepository, FakeShoppingRepository>();

    return services;
  }
}