using FluentValidation;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Shopping;
using Microsoft.Extensions.DependencyInjection;

namespace Metaspesa.Application;

public static class ApplicationDependencyInjection {
  public static IServiceCollection AddApplication(
    this IServiceCollection services
  ) {
    services.AddScoped<
      IQueryHandler<GetCurrentShoppingList.Query, ShoppingList>,
      GetCurrentShoppingList.Handler>();

    services.AddScoped<
      ICommandHandler<RecordShoppingList.Command>,
      RecordShoppingList.Handler>();

    services.AddScoped<
      IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<RegisteredItem>>,
      GetRegisteredItems.Handler>();

    services.AddValidatorsFromAssemblyContaining<Result>(includeInternalTypes: true);

    return services;
  }
}