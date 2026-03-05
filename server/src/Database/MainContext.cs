using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database;

internal class MainContext(
  DbContextOptions<MainContext> options
) : DbContext(options), IUnitOfWork {
  public DbSet<UserDbEntity> Users { get; set; } = null!;
  public DbSet<ShoppingListDbEntity> ShoppingLists { get; set; } = null!;
  public DbSet<ShoppingItemDbEntity> ShoppingItems { get; set; } = null!;
  public DbSet<ShoppingListOwnershipDbEntity> ShoppingListOwnerships { get; set; } = null!;
  public DbSet<RegisteredItemDbEntity> RegisteredItems { get; set; } = null!;
  public DbSet<RegisteredItemsHistoryDbEntity> RegisteredItemsHistory { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(MainContext).Assembly);
  }
}