using System.Diagnostics;
using FluentValidation;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Identity;
using Metaspesa.Application.Markets;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Shopping;
using Microsoft.Extensions.DependencyInjection;

namespace Metaspesa.Application;

public static class ApplicationDependencyInjection {
#pragma warning disable CA1034 // Nested types should not be visible
  extension(IServiceCollection services) {
#pragma warning restore CA1034 // Nested types should not be visible
    public IServiceCollection AddApplication() {
      Debug.Assert(services != null);

      services.AddIdentityUseCases();
      services.AddMarketUseCases();
      services.AddShoppingUseCases();

      services.AddValidatorsFromAssemblyContaining<Result>(includeInternalTypes: true);

      return services;
    }

    private IServiceCollection AddIdentityUseCases() {
      services.AddScoped<RegisterUser.Handler>();
      services.AddScoped<LoginUser.Handler>();

      return services;
    }

    private IServiceCollection AddMarketUseCases() {
      services.AddScoped<AddMarketProducts.Handler>();
      services.AddScoped<GetMarketProducts.Handler>();
      services.AddScoped<GetMarkets.Handler>();

      return services;
    }

    private IServiceCollection AddShoppingUseCases() {
      services.AddScoped<
        IQueryHandler<GetShoppingList.Query, GetShoppingList.Response>,
        GetShoppingList.Handler>();

      services.AddScoped<
        IQueryHandler<GetShoppingListSummaries.Query, List<AShoppingList>>,
        GetShoppingListSummaries.Handler>();

      services.AddScoped<
        ICommandHandler<RecordShoppingList.Command>,
        RecordShoppingList.Handler>();

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
        ICommandHandler<UpdateShoppingList.Command>,
        UpdateShoppingList.Handler>();

      services.AddScoped<
        ICommandHandler<RemoveItem.Command>,
        RemoveItem.Handler>();

      return services;
    }
  }
}
