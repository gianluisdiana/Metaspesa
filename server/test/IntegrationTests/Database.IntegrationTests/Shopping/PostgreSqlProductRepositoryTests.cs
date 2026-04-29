using Metaspesa.Database.Entities;
using Metaspesa.Database.Repositories;
using Metaspesa.Domain.Shopping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Metaspesa.Database.IntegrationTests.Shopping;

public static class PostgreSqlProductRepositoryTests {
  private static readonly UserRoleDbEntity TestRole = new() {
    Name = "TestRole",
    Description = "Role for testing",
  };

  [Collection("Database")]
  public class GetRegisteredItemsAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlProductRepository _repository;

    public GetRegisteredItemsAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlProductRepository(
        _context, NullLogger<PostgreSqlProductRepository>.Instance);
    }

    public async ValueTask InitializeAsync() {
      await _context.Users.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.UserRoles.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns empty list when user has no registered items")]
    public async Task Repository_ReturnsEmptyList_WhenUserHasNoRegisteredItems() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid,
        Username = userUid.ToString(),
        EncryptedPassword = "x",
        Role = TestRole
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<Product> result = await _repository.GetRegisteredItemsAsync(
        userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns registered items for user")]
    public async Task Repository_ReturnsItems_ForUser() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid,
        Username = userUid.ToString(),
        EncryptedPassword = "x",
        Role = TestRole
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RegisterItems(userUid, [
        new ShoppingItem("Milk", "1L", new Price(1.5f), false),
        new ShoppingItem("Bread", null, new Price(2.0f), false),
      ]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<Product> result = await _repository.GetRegisteredItemsAsync(
        userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(2, result.Count);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Maps name, quantity and price correctly")]
    public async Task Repository_MapsFields_Correctly() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid,
        Username = userUid.ToString(),
        EncryptedPassword = "x",
        Role = TestRole,
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RegisterItems(
        userUid, [new ShoppingItem("Milk", "2L", new Price(3.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<Product> result = await _repository.GetRegisteredItemsAsync(
        userUid, TestContext.Current.CancellationToken);

      // Assert
      Product item = result.Single();
      Assert.Equal("Milk", item.Name);
      Assert.Equal("2L", item.Quantity);
      Assert.Equal(3.5f, item.Price.Value, precision: 2);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Does not return items from different user")]
    public async Task Repository_DoesNotReturnItems_FromDifferentUser() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      var otherUid = Guid.CreateVersion7();
      _context.Users.AddRange(
        new UserDbEntity {
          Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", Role = TestRole
        },
        new UserDbEntity {
          Uid = otherUid, Username = otherUid.ToString(), EncryptedPassword = "x", Role = TestRole
        }
      );
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RegisterItems(
        otherUid, [new ShoppingItem("Milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<Product> result = await _repository.GetRegisteredItemsAsync(
        userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result);
    }
  }

  [Collection("Database")]
  public class RegisterItems : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlProductRepository _repository;

    public RegisterItems(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlProductRepository(
        _context, NullLogger<PostgreSqlProductRepository>.Instance);
    }

    public async ValueTask InitializeAsync() {
      await _context.Users.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.UserRoles.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Persists items to database")]
    public async Task RegisterItems_PersistsItems_ToDatabase() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid,
        Username = userUid.ToString(),
        EncryptedPassword = "x",
        Role = TestRole,
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.RegisterItems(
        userUid, [new ShoppingItem("Milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      List<Product> result = await _repository.GetRegisteredItemsAsync(
        userUid, TestContext.Current.CancellationToken);
      Assert.Single(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Persists price as LastKnownPrice")]
    public async Task RegisterItems_PersistsPriceAsLastKnownPrice() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid,
        Username = userUid.ToString(),
        EncryptedPassword = "x",
        Role = TestRole,
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.RegisterItems(
        userUid, [new ShoppingItem("Coffee", null, new Price(7.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      RegisteredItemDbEntity dbEntity = await _context.RegisteredItems
        .AsNoTracking()
        .FirstAsync(
          ri => ri.UserUid == userUid && ri.Name == "Coffee",
          TestContext.Current.CancellationToken);
      Assert.Equal(7.5f, dbEntity.LastKnownPrice, precision: 2);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Persists multiple items in single call")]
    public async Task RegisterItems_PersistsMultipleItems_InSingleCall() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid,
        Username = userUid.ToString(),
        EncryptedPassword = "x",
        Role = TestRole,
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.RegisterItems(userUid, [
        new ShoppingItem("ItemA", null, new Price(1.0f), false),
        new ShoppingItem("ItemB", null, new Price(2.0f), false),
        new ShoppingItem("ItemC", null, new Price(3.0f), false),
      ]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      List<Product> result = await _repository.GetRegisteredItemsAsync(
        userUid, TestContext.Current.CancellationToken);
      Assert.Equal(3, result.Count);
    }
  }

  [Collection("Database")]
  public class UpdateRegisteredItems : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlProductRepository _repository;

    public UpdateRegisteredItems(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlProductRepository(
        _context, NullLogger<PostgreSqlProductRepository>.Instance);
    }

    public async ValueTask InitializeAsync() {
      await _context.Users.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
      await _context.UserRoles.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Updates quantity of existing registered item")]
    public async Task UpdateRegisteredItems_UpdatesQuantity_OfExistingItem() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid,
        Username = userUid.ToString(),
        EncryptedPassword = "x",
        Role = TestRole,
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RegisterItems(
        userUid, [new ShoppingItem("Milk", "1L", new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateRegisteredItems(
        userUid, [new ShoppingItem("Milk", "2L", new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      List<Product> result = await _repository.GetRegisteredItemsAsync(
        userUid, TestContext.Current.CancellationToken);
      Assert.Equal("2L", result.Single().Quantity);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Updates LastKnownPrice when new price is non-zero")]
    public async Task UpdateRegisteredItems_UpdatesPrice_WhenNewPriceIsNonZero() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid,
        Username = userUid.ToString(),
        EncryptedPassword = "x",
        Role = TestRole,
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RegisterItems(
        userUid, [new ShoppingItem("Milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateRegisteredItems(
        userUid, [new ShoppingItem("Milk", null, new Price(3.0f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      List<Product> result = await _repository.GetRegisteredItemsAsync(
        userUid, TestContext.Current.CancellationToken);
      Assert.Equal(3.0f, result.Single().Price.Value, precision: 2);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Preserves original price when new price is zero")]
    public async Task UpdateRegisteredItems_PreservesOriginalPrice_WhenNewPriceIsZero() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid,
        Username = userUid.ToString(),
        EncryptedPassword = "x",
        Role = TestRole,
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RegisterItems(
        userUid, [new ShoppingItem("Milk", null, new Price(1.5f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateRegisteredItems(
        userUid, [new ShoppingItem("Milk", null, Price.Empty, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      List<Product> result = await _repository.GetRegisteredItemsAsync(
        userUid, TestContext.Current.CancellationToken);
      Assert.Equal(1.5f, result.Single().Price.Value, precision: 2);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Matches items case-insensitively")]
    public async Task UpdateRegisteredItems_MatchesItems_CaseInsensitively() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", Role = TestRole
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RegisterItems(
        userUid, [new ShoppingItem("milk", null, new Price(1.0f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateRegisteredItems(
        userUid, [new ShoppingItem("MILK", null, new Price(2.0f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      List<Product> result = await _repository.GetRegisteredItemsAsync(
        userUid, TestContext.Current.CancellationToken);
      Assert.Equal(2.0f, result.Single().Price.Value, precision: 2);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Does not update items from different user")]
    public async Task UpdateRegisteredItems_DoesNotUpdate_ItemsFromDifferentUser() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      var otherUid = Guid.CreateVersion7();
      _context.Users.AddRange(
        new UserDbEntity {
          Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", Role = TestRole
        },
        new UserDbEntity {
          Uid = otherUid, Username = otherUid.ToString(), EncryptedPassword = "x", Role = TestRole
        });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RegisterItems(
        otherUid, [new ShoppingItem("Milk", null, new Price(1.0f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateRegisteredItems(
        userUid, [new ShoppingItem("Milk", null, new Price(5.0f), false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      List<Product> otherResult = await _repository.GetRegisteredItemsAsync(
        otherUid, TestContext.Current.CancellationToken);
      Assert.Equal(1.0f, otherResult.Single().Price.Value, precision: 2);
    }
  }
}