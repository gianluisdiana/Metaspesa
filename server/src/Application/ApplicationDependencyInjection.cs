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
      IQueryHandler<GetRegisteredItems.Query, IReadOnlyCollection<Product>>,
      GetRegisteredItems.Handler>();

    services.AddScoped<
      ICommandHandler<CreateShoppingList.Command>,
      CreateShoppingList.Handler>();

    services.AddScoped<
      ICommandHandler<AddItemsToList.Command>,
      AddItemsToList.Handler>();

    services.AddScoped<
      ICommandHandler<UpdateItem.Command>,
      UpdateItem.Handler>();

    services.AddScoped<
      ICommandHandler<RemoveItem.Command>,
      RemoveItem.Handler>();

    services.AddValidatorsFromAssemblyContaining<Result>(includeInternalTypes: true);

    return services;
  }
}