using Metaspesa.Database.Entities;
using Metaspesa.Database.Repositories;
using Metaspesa.Domain.Shopping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging.Abstractions;

namespace Metaspesa.Database.IntegrationTests.Shopping;

public static class PostgreSqlShoppingRepositoryTests {
  [Collection("Database")]
  public class GetCurrentShoppingListAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public GetCurrentShoppingListAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlShoppingRepository(
        _context, NullLogger<PostgreSqlShoppingRepository>.Instance);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns null when user has no list")]
    public async Task GetCurrentShoppingListAsync_ReturnsNull_WhenUserHasNoList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      ShoppingList? result = await _repository.GetCurrentShoppingListAsync(
        userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns list when user has named list")]
    public async Task GetCurrentShoppingListAsync_ReturnsList_WhenUserHasNamedList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      ShoppingList? result = await _repository.GetCurrentShoppingListAsync(
        userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal("Groceries", result!.Name);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns list when user has temporary (null name) list")]
    public async Task GetCurrentShoppingListAsync_ReturnsList_WhenUserHasTemporaryList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      ShoppingList? result = await _repository.GetCurrentShoppingListAsync(
        userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result!.Name);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns empty items when list has no items")]
    public async Task GetCurrentShoppingListAsync_ReturnsEmptyItems_WhenListHasNoItems() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      ShoppingList? result = await _repository.GetCurrentShoppingListAsync(
        userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result!.Items);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns items excluding soft-deleted ones")]
    public async Task GetCurrentShoppingListAsync_ReturnsItems_ExcludingSoftDeleted() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(userUid, "Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), false),
        new ShoppingItem("Bread", null, new Price(2.0f), false),
      ]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RemoveItem(userUid, "Groceries", "Milk");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      ShoppingList? result = await _repository.GetCurrentShoppingListAsync(
        userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal("Bread", result!.Items.Single().Name);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns null when list belongs to different user")]
    public async Task GetCurrentShoppingListAsync_ReturnsNull_WhenListBelongsToDifferentUser() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      var otherUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      _context.Users.Add(new UserDbEntity {
        Uid = otherUid, Username = otherUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(otherUid, "Other List");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      ShoppingList? result = await _repository.GetCurrentShoppingListAsync(
        userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result);
    }
  }

  [Collection("Database")]
  public class CheckShoppingListExistAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public CheckShoppingListExistAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlShoppingRepository(
        _context, NullLogger<PostgreSqlShoppingRepository>.Instance);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns false when user has no lists")]
    public async Task CheckShoppingListExistAsync_ReturnsFalse_WhenUserHasNoLists() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckShoppingListExistAsync(
        userUid, "Groceries", TestContext.Current.CancellationToken);

      // Assert
      Assert.False(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns true when named list exists")]
    public async Task CheckShoppingListExistAsync_ReturnsTrue_WhenNamedListExists() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckShoppingListExistAsync(
        userUid, "Groceries", TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns true case-insensitively")]
    public async Task CheckShoppingListExistAsync_ReturnsTrue_CaseInsensitively() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckShoppingListExistAsync(
        userUid, "GROCERIES", TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns true when null name matches temporary list")]
    public async Task CheckShoppingListExistAsync_ReturnsTrue_WhenNullNameMatchesTemporaryList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckShoppingListExistAsync(
        userUid, null, TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns false when list belongs to different user")]
    public async Task CheckShoppingListExistAsync_ReturnsFalse_WhenListBelongsToDifferentUser() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      var otherUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      _context.Users.Add(new UserDbEntity {
        Uid = otherUid, Username = otherUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(otherUid, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckShoppingListExistAsync(
        userUid, "Groceries", TestContext.Current.CancellationToken);

      // Assert
      Assert.False(result);
    }
  }

  [Collection("Database")]
  public class CreateShoppingList : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public CreateShoppingList(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlShoppingRepository(
        _context, NullLogger<PostgreSqlShoppingRepository>.Instance);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Creates named list owned by user")]
    public async Task CreateShoppingList_CreatesNamedList_OwnedByUser() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.CreateShoppingList(userUid, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      bool exists = await _repository.CheckShoppingListExistAsync(
        userUid, "Groceries", TestContext.Current.CancellationToken);
      Assert.True(exists);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Creates temporary list when name is null")]
    public async Task CreateShoppingList_CreatesTemporaryList_WhenNameIsNull() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      bool exists = await _repository.CheckShoppingListExistAsync(
        userUid, null, TestContext.Current.CancellationToken);
      Assert.True(exists);
    }
  }

  [Collection("Database")]
  public class AddItemsToList : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public AddItemsToList(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlShoppingRepository(
        _context, NullLogger<PostgreSqlShoppingRepository>.Instance);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Adds items to existing named list")]
    public async Task AddItemsToList_AddsItems_ToExistingNamedList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.AddItemsToList(userUid, "Weekly", [
        new ShoppingItem("Milk", "1L", new Price(1.5f), false),
        new ShoppingItem("Eggs", "12", new Price(3.0f), false),
      ]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      ShoppingList? list = await _repository.GetCurrentShoppingListAsync(
        userUid, TestContext.Current.CancellationToken);
      Assert.Equal(2, list!.Items.Count);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Adds items to temporary list")]
    public async Task AddItemsToList_AddsItems_ToTemporaryList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.AddItemsToList(
        userUid, null, [new ShoppingItem("Butter", null, new Price(2.0f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      ShoppingList? list = await _repository.GetCurrentShoppingListAsync(
        userUid, TestContext.Current.CancellationToken);
      Assert.Equal("Butter", list!.Items.Single().Name);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Persists item name, quantity and price correctly")]
    public async Task AddItemsToList_PersistsItemFields_Correctly() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("Milk", "2L", new Price(2.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      ShoppingItem? item = await _repository.GetItemAsync(
        userUid, "Weekly", "Milk", TestContext.Current.CancellationToken);
      Assert.NotNull(item);
      Assert.Equal("Milk", item.Name);
      Assert.Equal("2L", item.Quantity);
      Assert.Equal(2.5f, item.Price.Value, precision: 2);
    }
  }

  [Collection("Database")]
  public class CheckItemExistsAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public CheckItemExistsAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlShoppingRepository(
        _context, NullLogger<PostgreSqlShoppingRepository>.Instance);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns false when item does not exist")]
    public async Task CheckItemExistsAsync_ReturnsFalse_WhenItemDoesNotExist() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckItemExistsAsync(
        userUid, "Weekly", "Milk", TestContext.Current.CancellationToken);

      // Assert
      Assert.False(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns true when item exists")]
    public async Task CheckItemExistsAsync_ReturnsTrue_WhenItemExists() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("Milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckItemExistsAsync(
        userUid, "Weekly", "Milk", TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns true case-insensitively")]
    public async Task CheckItemExistsAsync_ReturnsTrue_CaseInsensitively() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckItemExistsAsync(
        userUid, "Weekly", "MILK", TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns false for soft-deleted item")]
    public async Task CheckItemExistsAsync_ReturnsFalse_ForSoftDeletedItem() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("Milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RemoveItem(userUid, "Weekly", "Milk");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckItemExistsAsync(
        userUid, "Weekly", "Milk", TestContext.Current.CancellationToken);

      // Assert
      Assert.False(result);
    }
  }

  [Collection("Database")]
  public class GetItemAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public GetItemAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlShoppingRepository(
        _context, NullLogger<PostgreSqlShoppingRepository>.Instance);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns null when item does not exist")]
    public async Task GetItemAsync_ReturnsNull_WhenItemDoesNotExist() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      ShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", "Milk", TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns item when it exists")]
    public async Task GetItemAsync_ReturnsItem_WhenItExists() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("Milk", "2L", new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      ShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", "Milk", TestContext.Current.CancellationToken);

      // Assert
      Assert.NotNull(result);
      Assert.Equal("Milk", result.Name);
      Assert.Equal("2L", result.Quantity);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns null for soft-deleted item")]
    public async Task GetItemAsync_ReturnsNull_ForSoftDeletedItem() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("Milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RemoveItem(userUid, "Weekly", "Milk");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      ShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", "Milk", TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns item case-insensitively")]
    public async Task GetItemAsync_ReturnsItem_CaseInsensitively() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      ShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", "MILK", TestContext.Current.CancellationToken);

      // Assert
      Assert.NotNull(result);
    }
  }

  [Collection("Database")]
  public class UpdateItem : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public UpdateItem(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlShoppingRepository(
        _context, NullLogger<PostgreSqlShoppingRepository>.Instance);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Updates item name")]
    public async Task UpdateItem_UpdatesName_WhenNewNameProvided() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("Milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateItem(
        userUid,
        "Weekly",
        "Milk",
        new ShoppingItem("Whole Milk", null, new Price(1.5f), false));
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      ShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", "Whole Milk", TestContext.Current.CancellationToken);
      Assert.NotNull(result);
      Assert.Equal("Whole Milk", result.Name);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Updates item price")]
    public async Task UpdateItem_UpdatesPrice_WhenNewPriceProvided() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("Milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateItem(
        userUid,
        "Weekly",
        "Milk",
        new ShoppingItem("Milk", null, new Price(3.5f), false));
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      ShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", "Milk", TestContext.Current.CancellationToken);
      Assert.NotNull(result);
      Assert.Equal(3.5f, result.Price.Value, precision: 2);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Updates item checked state to true")]
    public async Task UpdateItem_UpdatesCheckedState_ToTrue() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("Milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateItem(
        userUid,
        "Weekly",
        "Milk",
        new ShoppingItem("Milk", null, new Price(1.5f), true));
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      ShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", "Milk", TestContext.Current.CancellationToken);
      Assert.NotNull(result);
      Assert.True(result.IsChecked);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Updates item quantity")]
    public async Task UpdateItem_UpdatesQuantity_WhenNewQuantityProvided() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("Milk", "1L", new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateItem(
        userUid,
        "Weekly",
        "Milk",
        new ShoppingItem("Milk", "2L", new Price(1.5f), false));
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      ShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", "Milk", TestContext.Current.CancellationToken);
      Assert.NotNull(result);
      Assert.Equal("2L", result.Quantity);
    }
  }

  [Collection("Database")]
  public class RemoveItem : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public RemoveItem(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlShoppingRepository(
        _context, NullLogger<PostgreSqlShoppingRepository>.Instance);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Soft-deletes item by setting DeletedAt")]
    public async Task RemoveItem_SoftDeletesItem_BySettingDeletedAt() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("Milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.RemoveItem(userUid, "Weekly", "Milk");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      ShoppingItemDbEntity? dbEntity = await _context.ShoppingItems
        .AsNoTracking()
        .FirstOrDefaultAsync(
          i => i.Name == "Milk" && i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid),
          TestContext.Current.CancellationToken);
      Assert.NotNull(dbEntity);
      Assert.NotNull(dbEntity.DeletedAt);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Item is no longer returned after removal")]
    public async Task RemoveItem_ItemIsNoLongerReturned_AfterRemoval() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("Milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.RemoveItem(userUid, "Weekly", "Milk");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      ShoppingList? list = await _repository.GetCurrentShoppingListAsync(
        userUid, TestContext.Current.CancellationToken);
      Assert.NotNull(list);
      Assert.Empty(list.Items);
    }
  }

  [Collection("Database")]
  public class RecordShoppingList : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public RecordShoppingList(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlShoppingRepository(
        _context, NullLogger<PostgreSqlShoppingRepository>.Instance);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Soft-deletes checked items after recording")]
    public async Task RecordShoppingList_SoftDeletesCheckedItems_AfterRecording() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      var registeredItem = new RegisteredItemDbEntity {
        UserUid = userUid, Name = "Milk", LastKnownPrice = 1.5f
      };
      _context.RegisteredItems.Add(registeredItem);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(userUid, "Weekly", [
        new ShoppingItem("Milk", null, new Price(1.5f), true),
        new ShoppingItem("Bread", null, new Price(2.0f), false),
      ]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.RecordShoppingList(userUid, new ShoppingList("Weekly", [
        new ShoppingItem("Milk", null, new Price(1.5f), true),
        new ShoppingItem("Bread", null, new Price(2.0f), false),
      ]));
      foreach (EntityEntry<PurchaseDbEntity> entry in _context.ChangeTracker.Entries<PurchaseDbEntity>()) {
        entry.Entity.RegisteredItemId = registeredItem.Id;
      }

      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      ShoppingItemDbEntity milk = await _context.ShoppingItems
        .AsNoTracking()
        .FirstAsync(
          i => i.Name == "Milk" && i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid),
          TestContext.Current.CancellationToken);
      ShoppingItemDbEntity bread = await _context.ShoppingItems
        .AsNoTracking()
        .FirstAsync(
          i => i.Name == "Bread" && i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid),
          TestContext.Current.CancellationToken);
      Assert.NotNull(milk.DeletedAt);
      Assert.Null(bread.DeletedAt);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Creates purchase records for checked items")]
    public async Task RecordShoppingList_CreatesPurchaseRecords_ForCheckedItems() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x"
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      var registeredItem = new RegisteredItemDbEntity {
        UserUid = userUid, Name = "Eggs", LastKnownPrice = 3.0f
      };
      _context.RegisteredItems.Add(registeredItem);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new ShoppingItem("Eggs", null, new Price(3.0f), true)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.RecordShoppingList(userUid, new ShoppingList("Weekly", [
        new ShoppingItem("Eggs", null, new Price(3.0f), true),
      ]));
      foreach (EntityEntry<PurchaseDbEntity> entry in _context.ChangeTracker.Entries<PurchaseDbEntity>()) {
        entry.Entity.RegisteredItemId = registeredItem.Id;
      }

      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      int count = await _context.Purchases
        .AsNoTracking()
        .CountAsync(p => p.UserUid == userUid, TestContext.Current.CancellationToken);
      Assert.Equal(1, count);
    }
  }
}