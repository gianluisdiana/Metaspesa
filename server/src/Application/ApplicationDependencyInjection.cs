using System.Diagnostics;
using FluentValidation;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Auth;
using Metaspesa.Application.Markets;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;
using Microsoft.Extensions.DependencyInjection;

namespace Metaspesa.Application;

public static class ApplicationDependencyInjection {
#pragma warning disable CA1034 // Nested types should not be visible
  extension(IServiceCollection services) {
#pragma warning restore CA1034 // Nested types should not be visible
    public IServiceCollection AddApplication() {
      Debug.Assert(services != null);

      services.AddAuthUseCases();
      services.AddMarketUseCases();
      services.AddShoppingUseCases();

      services.AddValidatorsFromAssemblyContaining<Result>(includeInternalTypes: true);

      return services;
    }

    private IServiceCollection AddAuthUseCases() {
      services.AddScoped<
        ICommandHandler<RegisterUser.Command>,
        RegisterUser.Handler>();

      services.AddScoped<
        IQueryHandler<LoginUser.Query, Token>,
        LoginUser.Handler>();

      return services;
    }

    private IServiceCollection AddMarketUseCases() {
      services.AddScoped<
        ICommandHandler<AddMarketProducts.Command>,
        AddMarketProducts.Handler>();

      services.AddScoped<
        IQueryHandler<GetMarketProducts.Query, PagedResult<Market>>,
        GetMarketProducts.Handler>();

      return services;
    }

    private IServiceCollection AddShoppingUseCases() {
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

      return services;
    }
  }
}