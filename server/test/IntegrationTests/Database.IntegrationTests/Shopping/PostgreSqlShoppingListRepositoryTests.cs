using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Database.Entities;
using Metaspesa.Database.Repositories;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Metaspesa.Database.IntegrationTests.Shopping;

[Collection("Database")]
public class PostgreSqlShoppingListRepositoryTests : IAsyncLifetime {
  private static readonly DateTime RemovedAt =
    new(2026, 8, 18, 10, 0, 0, DateTimeKind.Utc);

  private readonly MainContext _context;
  private readonly PostgreSqlShoppingListRepository _repository;

  public PostgreSqlShoppingListRepositoryTests(DatabaseFixture fixture) {
    _context = fixture.CreateContext();
    IClock clock = Substitute.For<IClock>();
    clock.GetCurrentTime().Returns(RemovedAt);
    _repository = new PostgreSqlShoppingListRepository(_context, clock);
  }

  public async ValueTask InitializeAsync() {
    if (!await _context.UserRoles.AnyAsync(
      role => role.Id == (int)Role.Shopper,
      TestContext.Current.CancellationToken)) {
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = nameof(Role.Shopper),
        Description = "Regular user who manages shopping lists",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
  }

  public async ValueTask DisposeAsync() {
    await _context.DisposeAsync();
    GC.SuppressFinalize(this);
  }

  [Fact(DisplayName = "Persists and reconstructs named aggregate")]
  public async Task AddAndGetAsync_PersistsTypedAggregate() {
    UserId ownerId = await SeedUserAsync();
    ProductFormatId formatId = await SeedProductFormatAsync("Milk");
    var list = ShoppingList.Create(ownerId, new ShoppingListName("Weekly"));
    list.AddItem(formatId, new PositiveAmount(2), true);

    _repository.Add(list);
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    ShoppingList? result = await _repository.GetAsync(
      ownerId,
      new ShoppingListName("weekly"),
      TestContext.Current.CancellationToken);

    Assert.NotNull(result);
    Assert.NotNull(result.Id);
    Assert.Equal(new ShoppingListName("Weekly"), result.Name);
    Assert.False(result.IsTemporary);
    Assert.Equal(ownerId, Assert.Single(result.OwnerIds));
    ShoppingItem item = Assert.Single(result.Items);
    Assert.Equal(formatId, item.ProductFormatId);
    Assert.Equal(2, item.Amount.Value);
    Assert.True(item.IsChecked);
  }

  [Fact(DisplayName = "Persists temporary empty aggregate")]
  public async Task AddAndGetAsync_PersistsTemporaryEmptyList() {
    UserId ownerId = await SeedUserAsync();
    _repository.Add(ShoppingList.Create(ownerId, null));
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

    ShoppingList? result = await _repository.GetAsync(
      ownerId, null, TestContext.Current.CancellationToken);

    Assert.NotNull(result);
    Assert.True(result.IsTemporary);
    Assert.Null(result.Name);
    Assert.Empty(result.Items);
  }

  [Fact(DisplayName = "Returns only owner-accessible lists")]
  public async Task GetByOwnerAsync_IsolatesOwners() {
    UserId ownerId = await SeedUserAsync();
    UserId otherOwnerId = await SeedUserAsync();
    _repository.Add(ShoppingList.Create(ownerId, new ShoppingListName("Mine")));
    _repository.Add(ShoppingList.Create(otherOwnerId, new ShoppingListName("Other")));
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

    IReadOnlyCollection<ShoppingList> result = await _repository.GetByOwnerAsync(
      ownerId, TestContext.Current.CancellationToken);

    ShoppingList list = Assert.Single(result);
    Assert.Equal(new ShoppingListName("Mine"), list.Name);
  }

  [Fact(DisplayName = "Updates aggregate state")]
  public async Task UpdateAsync_PersistsRenameAddAndUpdate() {
    UserId ownerId = await SeedUserAsync();
    ProductFormatId milkId = await SeedProductFormatAsync("Milk");
    ProductFormatId breadId = await SeedProductFormatAsync("Bread");
    var list = ShoppingList.Create(ownerId, null);
    list.AddItem(milkId, new PositiveAmount(1), false);
    _repository.Add(list);
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    ShoppingList persisted = (await _repository.GetAsync(
      ownerId, null, TestContext.Current.CancellationToken))!;

    persisted.Rename(new ShoppingListName("Weekly"));
    persisted.UpdateItem(milkId, new PositiveAmount(3), true);
    persisted.AddItem(breadId, new PositiveAmount(1), false);
    await _repository.UpdateAsync(persisted, TestContext.Current.CancellationToken);
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    ShoppingList result = (await _repository.GetAsync(
      ownerId,
      new ShoppingListName("Weekly"),
      TestContext.Current.CancellationToken))!;

    Assert.False(result.IsTemporary);
    Assert.Equal(2, result.Items.Count);
    ShoppingItem milk = result.Items.Single(item => item.ProductFormatId == milkId);
    Assert.Equal(3, milk.Amount.Value);
    Assert.True(milk.IsChecked);
  }

  [Fact(DisplayName = "Soft deletes removed item")]
  public async Task UpdateAsync_SoftDeletesRemovedItem() {
    UserId ownerId = await SeedUserAsync();
    ProductFormatId formatId = await SeedProductFormatAsync("Milk");
    var list = ShoppingList.Create(ownerId, new ShoppingListName("Weekly"));
    list.AddItem(formatId, new PositiveAmount(1), false);
    _repository.Add(list);
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    ShoppingList persisted = (await _repository.GetAsync(
      ownerId,
      new ShoppingListName("Weekly"),
      TestContext.Current.CancellationToken))!;

    persisted.RemoveItem(formatId);
    await _repository.UpdateAsync(persisted, TestContext.Current.CancellationToken);
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

    ShoppingItemDbEntity row = await _context.ShoppingItems
      .AsNoTracking()
      .SingleAsync(
        item => item.ProductFormatId == formatId.Value &&
          item.ShoppingListId == persisted.Id!.Value.Value,
        TestContext.Current.CancellationToken);
    Assert.Equal(RemovedAt, row.DeletedAt);
    ShoppingList result = (await _repository.GetAsync(
      ownerId,
      new ShoppingListName("Weekly"),
      TestContext.Current.CancellationToken))!;
    Assert.Empty(result.Items);
  }

  [Fact(DisplayName = "Throws exact exception for invalid persisted name")]
  public async Task GetAsync_ThrowsExactException_WhenPersistedNameIsInvalid() {
    UserId ownerId = await SeedUserAsync();
    var entity = new ShoppingListDbEntity {
      Name = "   ",
      IsTemporary = false,
      Ownerships = [new ShoppingListOwnershipDbEntity { UserUid = ownerId.Value }],
    };
    _context.ShoppingLists.Add(entity);
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<InvalidShoppingListNameException>(() =>
      _repository.GetByOwnerAsync(ownerId, TestContext.Current.CancellationToken));
  }

  [Fact(DisplayName = "Propagates cancelled reads")]
  public async Task GetAsync_PropagatesCancellation() {
    UserId ownerId = await SeedUserAsync();
    using var source = new CancellationTokenSource();
    await source.CancelAsync();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
      _repository.GetAsync(ownerId, null, source.Token));
  }

  [Fact(DisplayName = "Preserves purchase record and reset compatibility")]
  public async Task RecordAndReset_PreservesTransitionalWorkflow() {
    UserId ownerId = await SeedUserAsync();
    ProductFormatId formatId = await SeedProductFormatAsync("Milk");
    var list = ShoppingList.Create(ownerId, new ShoppingListName("Weekly"));
    list.AddItem(formatId, new PositiveAmount(2), true);
    _repository.Add(list);
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    ShoppingList persisted = (await _repository.GetAsync(
      ownerId,
      new ShoppingListName("Weekly"),
      TestContext.Current.CancellationToken))!;

    _repository.Record(ownerId, persisted);
    _repository.Reset(ownerId, persisted.Name);
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

    PurchaseItemDbEntity purchaseItem = await _context.PurchaseItems
      .AsNoTracking()
      .SingleAsync(
        item => item.Purchase.UserUid == ownerId.Value,
        TestContext.Current.CancellationToken);
    Assert.Equal(2, purchaseItem.Amount);
    ShoppingList reset = (await _repository.GetAsync(
      ownerId,
      new ShoppingListName("Weekly"),
      TestContext.Current.CancellationToken))!;
    Assert.False(reset.Items.Single().IsChecked);
  }

  private async Task<UserId> SeedUserAsync() {
    var id = new UserId(Guid.CreateVersion7());
    _context.Users.Add(new UserDbEntity {
      Uid = id.Value,
      Username = id.ToString(),
      EncryptedPassword = "x",
      RoleId = (int)Role.Shopper,
    });
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    return id;
  }

  private async Task<ProductFormatId> SeedProductFormatAsync(string productName) {
    var market = new SuperMarketDbEntity {
      Name = $"Test market {Guid.CreateVersion7()}",
    };
    var brand = new ProductBrandDbEntity {
      Name = $"Test brand {Guid.CreateVersion7()}",
    };
    var unit = new UnitOfMeasureDbEntity {
      Code = $"u{Guid.CreateVersion7():N}"[..16],
      Name = $"Test unit {Guid.CreateVersion7()}",
    };
    var product = new ProductDbEntity {
      Name = productName,
      SuperMarket = market,
      Brand = brand,
    };
    var format = new ProductFormatDbEntity {
      Product = product,
      Quantity = 1,
      UnitOfMeasure = unit,
      ImageUrl = "https://example.test/product.png",
    };
    _context.PriceSnapshots.Add(new PriceSnapshotDbEntity {
      ProductFormat = format,
      PriceAmount = 1.25m,
      ObservedAt = RemovedAt.AddDays(-1),
    });
    await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
    return new ProductFormatId(format.Id);
  }
}
