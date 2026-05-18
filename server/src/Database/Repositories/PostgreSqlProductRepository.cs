using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Database.Entities;
using Metaspesa.Domain.Shopping;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.Repositories;

internal partial class PostgreSqlProductRepository(
  MainContext context
) : IProductRepository {
  public async Task<List<Product>> GetRegisteredItemsAsync(
    Guid userUid, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => await context.RegisteredItems
      .Where(ri => ri.UserUid == userUid)
      .Select(ri => new Product(
        Name: ri.Name,
        Quantity: Quantity.FromNullable(ri.Quantity),
        Price: new Price(ri.LastKnownPrice)
      ))
      .ToListAsync(cancellationToken),
    "Couldn't get registered items.");

  public void RegisterItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems
  ) => PostgreSqlExceptionMapper.Map(() => {
    IEnumerable<RegisteredItemDbEntity> registeredItems = shoppingItems
      .Select(i => new RegisteredItemDbEntity {
        UserUid = userUid,
        Name = i.Name,
        Quantity = i.Quantity?.Value,
        LastKnownPrice = i.Price.Value,
      });

    context.RegisteredItems.AddRange(registeredItems);
  }, "Couldn't register items.");

  public void UpdateRegisteredItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems
  ) => PostgreSqlExceptionMapper.Map(() => {
    IEnumerable<string> itemNames = shoppingItems
      .Select(i => i.Name.ToUpperInvariant());

#pragma warning disable CA1304, CA1311
    var registeredItems = context.RegisteredItems.Where(ri =>
        ri.UserUid == userUid && itemNames.Contains(ri.Name.ToUpper()))
      .ToList();
#pragma warning restore CA1304, CA1311

    foreach (RegisteredItemDbEntity registeredItem in registeredItems) {
      ShoppingItem shoppingItem = shoppingItems.Single(i =>
        i.Name.Equals(registeredItem.Name, StringComparison.OrdinalIgnoreCase));

      registeredItem.Quantity = shoppingItem.Quantity?.Value;
      registeredItem.LastKnownPrice = !shoppingItem.Price.IsZero()
        ? shoppingItem.Price.Value
        : registeredItem.LastKnownPrice;
    }
  }, "Couldn't update registered items.");
}
