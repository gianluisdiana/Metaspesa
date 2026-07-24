using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Database.Entities;
using Metaspesa.Database.Repositories;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Metaspesa.Database.IntegrationTests.Shopping;

public static class PostgreSqlShoppingRepositoryTests {
  private static async ValueTask EnsureShopperRoleAsync(
    MainContext context, CancellationToken ct
  ) {
    if (!await context.UserRoles.AnyAsync(r => r.Id == (int)Role.Shopper, ct)) {
      context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper, Name = nameof(Role.Shopper), Description = "Regular user who manages shopping lists"
      });
      await context.SaveChangesAsync(ct);
    }
  }

  [Collection("Database")]
  public class GetShoppingListSummariesAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public GetShoppingListSummariesAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      IClock _clock = Substitute.For<IClock>();
      _repository = new PostgreSqlShoppingRepository(
        _context,
        _clock);
      _clock.GetCurrentTime().Returns(DateTime.UtcNow);
    }

    public ValueTask InitializeAsync() =>
      EnsureShopperRoleAsync(_context, TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      DisplayName = "Returns empty summaries when user has no lists")]
    public async Task GetShoppingListSummariesAsync_ReturnsEmpty_WhenUserHasNoLists() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<AShoppingList> result =
        await _repository.GetShoppingListSummariesAsync(
          userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result);
    }

    [Fact(
      DisplayName = "Returns two summaries when user owns named and temporary lists")]
    public async Task GetShoppingListSummariesAsync_ReturnsTwoSummaries_WhenUserOwnsNamedAndTemporaryLists() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<AShoppingList> result =
        await _repository.GetShoppingListSummariesAsync(
          userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(2, result.Count);
    }

    [Fact(
      DisplayName = "Returns named list summary before temporary list summary")]
    public async Task GetShoppingListSummariesAsync_ReturnsNamedListSummaryBeforeTemporaryListSummary() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<AShoppingList> result =
        await _repository.GetShoppingListSummariesAsync(
          userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal("Groceries", result[0].Name);
    }

    [Fact(
      DisplayName = "Returns temporary list summary with null name")]
    public async Task GetShoppingListSummariesAsync_ReturnsTemporaryListSummaryWithNullName() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<AShoppingList> result =
        await _repository.GetShoppingListSummariesAsync(
          userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result[1].Name);
    }

    [Fact(
      DisplayName = "Returns named list summary without items")]
    public async Task GetShoppingListSummariesAsync_ReturnsNamedListSummaryWithoutItems() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<AShoppingList> result =
        await _repository.GetShoppingListSummariesAsync(
          userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result[0].Items);
    }

    [Fact(
      DisplayName = "Returns temporary list summary without items")]
    public async Task GetShoppingListSummariesAsync_ReturnsTemporaryListSummaryWithoutItems() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<AShoppingList> result =
        await _repository.GetShoppingListSummariesAsync(
          userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result[1].Items);
    }

    [Fact(
      DisplayName = "Does not return summaries owned by another user")]
    public async Task GetShoppingListSummariesAsync_DoesNotReturnOtherUserLists() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      var otherUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      _context.Users.Add(new UserDbEntity {
        Uid = otherUid, Username = otherUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      _repository.CreateShoppingList(otherUid, "Other List");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<AShoppingList> result =
        await _repository.GetShoppingListSummariesAsync(
          userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Single(result);
    }

    [Fact(
      DisplayName = "Returns owned list summary name when another user owns a list")]
    public async Task GetShoppingListSummariesAsync_ReturnsOwnedListSummaryName_WhenAnotherUserOwnsAList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      var otherUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      _context.Users.Add(new UserDbEntity {
        Uid = otherUid, Username = otherUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      _repository.CreateShoppingList(otherUid, "Other List");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<AShoppingList> result =
        await _repository.GetShoppingListSummariesAsync(
          userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal("Groceries", result.Single().Name);
    }

    [Fact(
      DisplayName = "Returns owned list summary without items when another user owns a list")]
    public async Task GetShoppingListSummariesAsync_ReturnsOwnedListSummaryWithoutItems_WhenAnotherUserOwnsAList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      var otherUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      _context.Users.Add(new UserDbEntity {
        Uid = otherUid, Username = otherUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      _repository.CreateShoppingList(otherUid, "Other List");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      List<AShoppingList> result =
        await _repository.GetShoppingListSummariesAsync(
          userUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result.Single().Items);
    }
  }

  [Collection("Database")]
  public class GetShoppingListAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public GetShoppingListAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      IClock _clock = Substitute.For<IClock>();
      _repository = new PostgreSqlShoppingRepository(
        _context,
        _clock);
      _clock.GetCurrentTime().Returns(DateTime.UtcNow);
    }

    public ValueTask InitializeAsync() =>
      EnsureShopperRoleAsync(_context, TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task<int> SeedPriceSnapshotAsync(string productName) {
      var market = new SuperMarketDbEntity { Name = $"Test market {Guid.CreateVersion7()}" };
      var brand = new ProductBrandDbEntity { Name = $"Test brand {Guid.CreateVersion7()}" };
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
      var history = new PriceSnapshotDbEntity {
        ProductFormat = format,
        PriceAmount = 1.25m,
        ObservedAt = DateTime.UtcNow,
      };

      _context.PriceSnapshots.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return format.Id;
    }

    [Fact(
      DisplayName = "Returns null when user has no list")]
    public async Task GetShoppingListAsync_ReturnsNull_WhenUserHasNoList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      AShoppingList? result = await _repository.GetShoppingListAsync(
        userUid, null, TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result);
    }

    [Fact(
      DisplayName = "Returns list when user has named list")]
    public async Task GetShoppingListAsync_ReturnsList_WhenUserHasNamedList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      AShoppingList? result = await _repository.GetShoppingListAsync(
        userUid, "Groceries", TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal("Groceries", result!.Name);
    }

    [Fact(
      DisplayName = "Returns list when user has temporary (null name) list")]
    public async Task GetShoppingListAsync_ReturnsList_WhenUserHasTemporaryList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      AShoppingList? result = await _repository.GetShoppingListAsync(
        userUid, null, TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result!.Name);
    }

    [Fact(
      DisplayName = "Returns empty items when list has no items")]
    public async Task GetShoppingListAsync_ReturnsEmptyItems_WhenListHasNoItems() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      AShoppingList? result = await _repository.GetShoppingListAsync(
        userUid, "Groceries", TestContext.Current.CancellationToken);

      // Assert
      Assert.Empty(result!.Items);
    }

    [Fact(
      DisplayName = "Returns items excluding soft-deleted ones")]
    public async Task GetShoppingListAsync_ReturnsItems_ExcludingSoftDeleted() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int milkReferenceUid = await SeedPriceSnapshotAsync("Milk");
      int breadReferenceUid = await SeedPriceSnapshotAsync("Bread");
      _repository.AddItemsToList(userUid, "Groceries", [
        new AShoppingItem(milkReferenceUid, 1, false),
        new AShoppingItem(breadReferenceUid, 1, false),
      ]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RemoveItem(userUid, "Groceries", breadReferenceUid);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      AShoppingList? result = await _repository.GetShoppingListAsync(
        userUid, "Groceries", TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(milkReferenceUid, result!.Items.Single().ReferenceUid);
    }

    [Fact(
      DisplayName = "Returns null when list belongs to different user")]
    public async Task GetShoppingListAsync_ReturnsNull_WhenListBelongsToDifferentUser() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      var otherUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      _context.Users.Add(new UserDbEntity {
        Uid = otherUid, Username = otherUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(otherUid, "Other List");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      AShoppingList? result = await _repository.GetShoppingListAsync(
        userUid, "Other List", TestContext.Current.CancellationToken);

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
      IClock _clock = Substitute.For<IClock>();
      _repository = new PostgreSqlShoppingRepository(
        _context,
        _clock);
      _clock.GetCurrentTime().Returns(DateTime.UtcNow);
    }

    public ValueTask InitializeAsync() =>
      EnsureShopperRoleAsync(_context, TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      DisplayName = "Returns false when user has no lists")]
    public async Task CheckShoppingListExistAsync_ReturnsFalse_WhenUserHasNoLists() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckShoppingListExistAsync(
        userUid, "Groceries", TestContext.Current.CancellationToken);

      // Assert
      Assert.False(result);
    }

    [Fact(
      DisplayName = "Returns true when named list exists")]
    public async Task CheckShoppingListExistAsync_ReturnsTrue_WhenNamedListExists() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
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
      DisplayName = "Returns true case-insensitively")]
    public async Task CheckShoppingListExistAsync_ReturnsTrue_CaseInsensitively() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
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
      DisplayName = "Returns true when null name matches temporary list")]
    public async Task CheckShoppingListExistAsync_ReturnsTrue_WhenNullNameMatchesTemporaryList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
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
      DisplayName = "Returns false when list belongs to different user")]
    public async Task CheckShoppingListExistAsync_ReturnsFalse_WhenListBelongsToDifferentUser() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      var otherUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      _context.Users.Add(new UserDbEntity {
        Uid = otherUid, Username = otherUid.ToString(), EncryptedPassword = "x", RoleId = 1
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
      IClock _clock = Substitute.For<IClock>();
      _repository = new PostgreSqlShoppingRepository(
        _context,
        _clock);
      _clock.GetCurrentTime().Returns(DateTime.UtcNow);
    }

    public ValueTask InitializeAsync() =>
      EnsureShopperRoleAsync(_context, TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    [Fact(
      DisplayName = "Creates named list owned by user")]
    public async Task CreateShoppingList_CreatesNamedList_OwnedByUser() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
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
      DisplayName = "Creates temporary list when name is null")]
    public async Task CreateShoppingList_CreatesTemporaryList_WhenNameIsNull() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
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
      IClock _clock = Substitute.For<IClock>();
      _repository = new PostgreSqlShoppingRepository(
        _context,
        _clock);
      _clock.GetCurrentTime().Returns(DateTime.UtcNow);
    }

    public ValueTask InitializeAsync() =>
      EnsureShopperRoleAsync(_context, TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task<int> SeedPriceSnapshotAsync(
      string productName,
      decimal price = 2.5m,
      decimal quantity = 2m
    ) {
      var market = new SuperMarketDbEntity { Name = $"Test market {Guid.CreateVersion7()}" };
      var brand = new ProductBrandDbEntity { Name = $"Test brand {Guid.CreateVersion7()}" };
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
        Quantity = quantity,
        UnitOfMeasure = unit,
        ImageUrl = "https://example.test/product.png",
      };
      var history = new PriceSnapshotDbEntity {
        ProductFormat = format,
        PriceAmount = price,
        ObservedAt = DateTime.UtcNow,
      };

      _context.PriceSnapshots.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return format.Id;
    }

    [Fact(
      DisplayName = "Adds items to existing named list")]
    public async Task AddItemsToList_AddsItems_ToExistingNamedList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int milkReferenceUid = await SeedPriceSnapshotAsync("Milk");
      int breadReferenceUid = await SeedPriceSnapshotAsync("Bread");

      // Act
      _repository.AddItemsToList(userUid, "Weekly", [
        new AShoppingItem(milkReferenceUid, 1, false),
        new AShoppingItem(breadReferenceUid, 1, false),
      ]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingList? list = await _repository.GetShoppingListAsync(
        userUid, "Weekly", TestContext.Current.CancellationToken);
      Assert.Equal(2, list!.Items.Count);
    }

    [Fact(
      DisplayName = "Adds items to temporary list")]
    public async Task AddItemsToList_AddsItems_ToTemporaryList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int butterReferenceUid = await SeedPriceSnapshotAsync("Butter");

      // Act
      _repository.AddItemsToList(
        userUid, null, [new AShoppingItem(butterReferenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingList? list = await _repository.GetShoppingListAsync(
        userUid, null, TestContext.Current.CancellationToken);
      Assert.Equal(butterReferenceUid, list!.Items.Single().ReferenceUid);
    }

    [Fact(
      DisplayName = "Persists item name, quantity and price correctly")]
    public async Task AddItemsToList_PersistsItemFields_Correctly() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int milkReferenceUid = await SeedPriceSnapshotAsync("Milk");

      // Act
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(milkReferenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingItem? item = await _repository.GetItemAsync(
        userUid, "Weekly", milkReferenceUid, TestContext.Current.CancellationToken);
      Assert.Equal(milkReferenceUid, item!.ReferenceUid);
    }

    [Fact(
      DisplayName = "Persists requested item amount")]
    public async Task AddItemsToList_PersistsRequestedItemAmount() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");

      // Act
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(referenceUid, 3, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      int amount = await _context.ShoppingItems
        .AsNoTracking()
        .Where(i => i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid))
        .Select(i => i.Amount)
        .SingleAsync(TestContext.Current.CancellationToken);
      Assert.Equal(3, amount);
    }

    [Fact(
      DisplayName = "Persists requested checked state")]
    public async Task AddItemsToList_PersistsRequestedCheckedState() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");

      // Act
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(referenceUid, 1, true)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      bool isChecked = await _context.ShoppingItems
        .AsNoTracking()
        .Where(i => i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid))
        .Select(i => i.IsChecked)
        .SingleAsync(TestContext.Current.CancellationToken);
      Assert.True(isChecked);
    }
  }

  [Collection("Database")]
  public class CheckItemExistsAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public CheckItemExistsAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      IClock _clock = Substitute.For<IClock>();
      _repository = new PostgreSqlShoppingRepository(
        _context,
        _clock);
      _clock.GetCurrentTime().Returns(DateTime.UtcNow);
    }

    public ValueTask InitializeAsync() =>
      EnsureShopperRoleAsync(_context, TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task<int> SeedPriceSnapshotAsync(string productName) {
      var market = new SuperMarketDbEntity { Name = $"Test market {Guid.CreateVersion7()}" };
      var brand = new ProductBrandDbEntity { Name = $"Test brand {Guid.CreateVersion7()}" };
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
      var history = new PriceSnapshotDbEntity {
        ProductFormat = format,
        PriceAmount = 1.25m,
        ObservedAt = DateTime.UtcNow,
      };

      _context.PriceSnapshots.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return format.Id;
    }

    [Fact(
      DisplayName = "Returns false when item does not exist")]
    public async Task CheckItemExistsAsync_ReturnsFalse_WhenItemDoesNotExist() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckItemExistsAsync(
        userUid, "Weekly", 10, TestContext.Current.CancellationToken);

      // Assert
      Assert.False(result);
    }

    [Fact(
      DisplayName = "Returns true when item exists")]
    public async Task CheckItemExistsAsync_ReturnsTrue_WhenItemExists() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckItemExistsAsync(
        userUid, "Weekly", referenceUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }

    [Fact(
      DisplayName = "Returns true case-insensitively")]
    public async Task CheckItemExistsAsync_ReturnsTrue_CaseInsensitively() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckItemExistsAsync(
        userUid, "WEEKLY", referenceUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }

    [Fact(
      DisplayName = "Returns false for soft-deleted item")]
    public async Task CheckItemExistsAsync_ReturnsFalse_ForSoftDeletedItem() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RemoveItem(userUid, "Weekly", referenceUid);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckItemExistsAsync(
        userUid, "Weekly", referenceUid, TestContext.Current.CancellationToken);

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
      IClock _clock = Substitute.For<IClock>();
      _repository = new PostgreSqlShoppingRepository(
        _context,
        _clock);
      _clock.GetCurrentTime().Returns(DateTime.UtcNow);
    }

    public ValueTask InitializeAsync() =>
      EnsureShopperRoleAsync(_context, TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task<int> SeedPriceSnapshotAsync(string productName) {
      var market = new SuperMarketDbEntity { Name = $"Test market {Guid.CreateVersion7()}" };
      var brand = new ProductBrandDbEntity { Name = $"Test brand {Guid.CreateVersion7()}" };
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
      var history = new PriceSnapshotDbEntity {
        ProductFormat = format,
        PriceAmount = 1.25m,
        ObservedAt = DateTime.UtcNow,
      };

      _context.PriceSnapshots.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return format.Id;
    }

    [Fact(
      DisplayName = "Returns null when item does not exist")]
    public async Task GetItemAsync_ReturnsNull_WhenItemDoesNotExist() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      AShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", 10, TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result);
    }

    [Fact(
      DisplayName = "Returns item when it exists")]
    public async Task GetItemAsync_ReturnsItem_WhenItExists() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      AShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", referenceUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(referenceUid, result.ReferenceUid);
    }

    [Fact(
      DisplayName = "Returns null for soft-deleted item")]
    public async Task GetItemAsync_ReturnsNull_ForSoftDeletedItem() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.RemoveItem(userUid, "Weekly", referenceUid);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      AShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", referenceUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result);
    }

    [Fact(
      DisplayName = "Returns item case-insensitively")]
    public async Task GetItemAsync_ReturnsItem_CaseInsensitively() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(
        userUid, "weekly", [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      AShoppingItem? result = await _repository.GetItemAsync(
        userUid, "WEEKLY", referenceUid, TestContext.Current.CancellationToken);

      // Assert
      Assert.NotNull(result);
    }
  }

  [Collection("Database")]
  public class UpdateShoppingList : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public UpdateShoppingList(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      IClock _clock = Substitute.For<IClock>();
      _repository = new PostgreSqlShoppingRepository(
        _context,
        _clock);
      _clock.GetCurrentTime().Returns(DateTime.UtcNow);
    }

    public ValueTask InitializeAsync() =>
      EnsureShopperRoleAsync(_context, TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task<int> SeedPriceSnapshotAsync(string productName) {
      var market = new SuperMarketDbEntity { Name = $"Test market {Guid.CreateVersion7()}" };
      var brand = new ProductBrandDbEntity { Name = $"Test brand {Guid.CreateVersion7()}" };
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
      var history = new PriceSnapshotDbEntity {
        ProductFormat = format,
        PriceAmount = 1.25m,
        ObservedAt = DateTime.UtcNow,
      };

      _context.PriceSnapshots.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return format.Id;
    }

    [Fact(
      DisplayName = "Renames temporary list to requested name")]
    public async Task UpdateShoppingListName_RenamesTemporaryList_ToRequestedName() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateShoppingListName(userUid, null, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingList? list = await _repository.GetShoppingListAsync(
        userUid, "Groceries", TestContext.Current.CancellationToken);
      Assert.NotNull(list);
    }

    [Fact(
      DisplayName = "Removes temporary list identity after renaming")]
    public async Task UpdateShoppingListName_RemovesTemporaryListIdentity_AfterRenaming() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateShoppingListName(userUid, null, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      bool temporaryListExists = await _repository.CheckShoppingListExistAsync(
        userUid, null, TestContext.Current.CancellationToken);
      Assert.False(temporaryListExists);
    }

    [Fact(
      DisplayName = "Preserves items when temporary list is renamed")]
    public async Task UpdateShoppingListName_PreservesItems_WhenTemporaryListIsRenamed() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(
        userUid, null, [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateShoppingListName(userUid, null, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingList? list = await _repository.GetShoppingListAsync(
        userUid, "Groceries", TestContext.Current.CancellationToken);
      Assert.Equal(referenceUid, list!.Items.Single().ReferenceUid);
    }
  }

  [Collection("Database")]
  public class UpdateItem : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public UpdateItem(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      IClock _clock = Substitute.For<IClock>();
      _repository = new PostgreSqlShoppingRepository(
        _context,
        _clock);
      _clock.GetCurrentTime().Returns(DateTime.UtcNow);
    }

    public ValueTask InitializeAsync() =>
      EnsureShopperRoleAsync(_context, TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task<int> SeedPriceSnapshotAsync(string productName) {
      var market = new SuperMarketDbEntity { Name = $"Test market {Guid.CreateVersion7()}" };
      var brand = new ProductBrandDbEntity { Name = $"Test brand {Guid.CreateVersion7()}" };
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
      var history = new PriceSnapshotDbEntity {
        ProductFormat = format,
        PriceAmount = 1.25m,
        ObservedAt = DateTime.UtcNow,
      };

      _context.PriceSnapshots.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return format.Id;
    }

    [Fact(
      DisplayName = "Updates item amount")]
    public async Task UpdateItem_UpdatesAmount_WhenNewAmountProvided() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateItem(
        userUid,
        "Weekly",
        new AShoppingItem(referenceUid, 3, false));
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", referenceUid, TestContext.Current.CancellationToken);
      Assert.NotNull(result);
      Assert.Equal(3, result.Amount);
    }

    [Fact(
      DisplayName = "Updates item checked state to true")]
    public async Task UpdateItem_UpdatesCheckedState_ToTrue() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateItem(
        userUid,
        "Weekly",
        new AShoppingItem(referenceUid, 1, true));
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", referenceUid, TestContext.Current.CancellationToken);
      Assert.NotNull(result);
      Assert.True(result.IsChecked);
    }

    [Fact(
      DisplayName = "Preserves checked state when only amount changes")]
    public async Task UpdateItem_PreservesCheckedState_WhenOnlyAmountChanges() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(referenceUid, 1, true)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateItem(
        userUid,
        "Weekly",
        new AShoppingItem(referenceUid, 2, true));
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingItem? result = await _repository.GetItemAsync(
        userUid, "Weekly", referenceUid, TestContext.Current.CancellationToken);
      Assert.NotNull(result);
      Assert.True(result.IsChecked);
    }

    [Fact(
      DisplayName = "Does not update matching reference from another user's list")]
    public async Task UpdateItem_DoesNotUpdateMatchingReference_FromAnotherUsersList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      var otherUid = Guid.CreateVersion7();
      _context.Users.AddRange(
        new UserDbEntity {
          Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
        },
        new UserDbEntity {
          Uid = otherUid, Username = otherUid.ToString(), EncryptedPassword = "x", RoleId = 1
        });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      _repository.CreateShoppingList(otherUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(userUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      _repository.AddItemsToList(otherUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateItem(
        userUid,
        "Weekly",
        new AShoppingItem(referenceUid, 5, false));
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingItem? result = await _repository.GetItemAsync(
        otherUid, "Weekly", referenceUid, TestContext.Current.CancellationToken);
      Assert.NotNull(result);
      Assert.Equal(1, result.Amount);
    }
  }

  [Collection("Database")]
  public class RemoveItem : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public RemoveItem(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      IClock _clock = Substitute.For<IClock>();
      _repository = new PostgreSqlShoppingRepository(
        _context,
        _clock);
      _clock.GetCurrentTime().Returns(DateTime.UtcNow);
    }

    public ValueTask InitializeAsync() =>
      EnsureShopperRoleAsync(_context, TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task<int> SeedPriceSnapshotAsync(string productName) {
      var market = new SuperMarketDbEntity { Name = $"Test market {Guid.CreateVersion7()}" };
      var brand = new ProductBrandDbEntity { Name = $"Test brand {Guid.CreateVersion7()}" };
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
      var history = new PriceSnapshotDbEntity {
        ProductFormat = format,
        PriceAmount = 1.25m,
        ObservedAt = DateTime.UtcNow,
      };

      _context.PriceSnapshots.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return format.Id;
    }

    [Fact(
      DisplayName = "Soft-deletes item by setting DeletedAt")]
    public async Task RemoveItem_SoftDeletesItem_BySettingDeletedAt() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.RemoveItem(userUid, "Weekly", referenceUid);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      ShoppingItemDbEntity? dbEntity = await _context.ShoppingItems
        .AsNoTracking()
        .FirstOrDefaultAsync(
          i => i.ProductFormat.Product.Name == "Milk" && i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid),
          TestContext.Current.CancellationToken);
      Assert.NotNull(dbEntity);
      Assert.NotNull(dbEntity.DeletedAt);
    }

    [Fact(
      DisplayName = "Item is no longer returned after removal")]
    public async Task RemoveItem_ItemIsNoLongerReturned_AfterRemoval() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.RemoveItem(userUid, "Weekly", referenceUid);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingList? list = await _repository.GetShoppingListAsync(
        userUid, "Weekly", TestContext.Current.CancellationToken);
      Assert.NotNull(list);
      Assert.Empty(list.Items);
    }

    [Fact(
      DisplayName = "Does not remove matching reference from another user's list")]
    public async Task RemoveItem_DoesNotRemoveMatchingReference_FromAnotherUsersList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      var otherUid = Guid.CreateVersion7();
      _context.Users.AddRange(
        new UserDbEntity {
          Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
        },
        new UserDbEntity {
          Uid = otherUid, Username = otherUid.ToString(), EncryptedPassword = "x", RoleId = 1
        });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      _repository.CreateShoppingList(otherUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(userUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      _repository.AddItemsToList(otherUid, "Weekly", [new AShoppingItem(referenceUid, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.RemoveItem(userUid, "Weekly", referenceUid);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingList? otherList = await _repository.GetShoppingListAsync(
        otherUid, "Weekly", TestContext.Current.CancellationToken);
      Assert.Single(otherList!.Items);
    }
  }

  [Collection("Database")]
  public class RecordShoppingList : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public RecordShoppingList(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      IClock _clock = Substitute.For<IClock>();
      _repository = new PostgreSqlShoppingRepository(
        _context,
        _clock);
      _clock.GetCurrentTime().Returns(DateTime.UtcNow);
    }

    public ValueTask InitializeAsync() =>
      EnsureShopperRoleAsync(_context, TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task<int> SeedPriceSnapshotAsync(string productName) {
      var market = new SuperMarketDbEntity { Name = $"Test market {Guid.CreateVersion7()}" };
      var brand = new ProductBrandDbEntity { Name = $"Test brand {Guid.CreateVersion7()}" };
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
      var history = new PriceSnapshotDbEntity {
        ProductFormat = format,
        PriceAmount = 1.25m,
        ObservedAt = DateTime.UtcNow,
      };

      _context.PriceSnapshots.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return format.Id;
    }

    [Fact(
      DisplayName = "Creates purchase item for checked item")]
    public async Task RecordShoppingList_CreatesPurchaseItem_ForCheckedItem() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int milkReferenceUid = await SeedPriceSnapshotAsync("Milk");
      int breadReferenceUid = await SeedPriceSnapshotAsync("Bread");

      // Act
      _repository.RecordShoppingList(userUid, new AShoppingList("Weekly", [
        new AShoppingItem(milkReferenceUid, 2, true),
        new AShoppingItem(breadReferenceUid, 1, false),
      ]));
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      PurchaseItemDbEntity item = await _context.PurchaseItems
        .AsNoTracking()
        .SingleAsync(i => i.Purchase.UserUid == userUid, TestContext.Current.CancellationToken);
      int expectedHistoryId = await _context.PriceSnapshots
        .Where(h => h.ProductFormatId == milkReferenceUid)
        .Select(h => h.Id)
        .SingleAsync(TestContext.Current.CancellationToken);
      Assert.Equal(expectedHistoryId, item.PriceSnapshotId);
      Assert.Equal(2, item.Amount);
    }

    [Fact(
      DisplayName = "Creates purchase records for checked items")]
    public async Task RecordShoppingList_CreatesPurchaseRecords_ForCheckedItems() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Eggs");

      // Act
      _repository.RecordShoppingList(userUid, new AShoppingList("Weekly", [
        new AShoppingItem(referenceUid, 1, true),
      ]));

      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      int count = await _context.Purchases
        .AsNoTracking()
        .CountAsync(p => p.UserUid == userUid, TestContext.Current.CancellationToken);
      Assert.Equal(1, count);
    }
  }

  [Collection("Database")]
  public class ResetShoppingList : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlShoppingRepository _repository;

    public ResetShoppingList(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      IClock _clock = Substitute.For<IClock>();
      _repository = new PostgreSqlShoppingRepository(
        _context,
        _clock);
      _clock.GetCurrentTime().Returns(DateTime.UtcNow);
    }

    public ValueTask InitializeAsync() =>
      EnsureShopperRoleAsync(_context, TestContext.Current.CancellationToken);

    public async ValueTask DisposeAsync() {
      await _context.DisposeAsync();
      GC.SuppressFinalize(this);
    }

    private async Task<int> SeedPriceSnapshotAsync(string productName) {
      var market = new SuperMarketDbEntity { Name = $"Test market {Guid.CreateVersion7()}" };
      var brand = new ProductBrandDbEntity { Name = $"Test brand {Guid.CreateVersion7()}" };
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
      var history = new PriceSnapshotDbEntity {
        ProductFormat = format,
        PriceAmount = 1.25m,
        ObservedAt = DateTime.UtcNow,
      };

      _context.PriceSnapshots.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return format.Id;
    }

    [Fact(
      DisplayName = "Unchecks checked items in target list")]
    public async Task ResetShoppingList_UnchecksCheckedItems_InTargetList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int milkReferenceUid = await SeedPriceSnapshotAsync("Milk");
      int breadReferenceUid = await SeedPriceSnapshotAsync("Bread");
      _repository.AddItemsToList(userUid, "Weekly", [
        new AShoppingItem(milkReferenceUid, 1, true),
        new AShoppingItem(breadReferenceUid, 1, false),
      ]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.ResetShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingList? list = await _repository.GetShoppingListAsync(
        userUid, "Weekly", TestContext.Current.CancellationToken);
      Assert.All(list!.Items, item => Assert.False(item.IsChecked));
    }

    [Fact(
      DisplayName = "Does not uncheck another user's matching list")]
    public async Task ResetShoppingList_DoesNotUncheckAnotherUsersMatchingList() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      var otherUid = Guid.CreateVersion7();
      _context.Users.AddRange(
        new UserDbEntity {
          Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
        },
        new UserDbEntity {
          Uid = otherUid, Username = otherUid.ToString(), EncryptedPassword = "x", RoleId = 1
        });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      _repository.CreateShoppingList(otherUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(userUid, "Weekly", [new AShoppingItem(referenceUid, 1, true)]);
      _repository.AddItemsToList(otherUid, "Weekly", [new AShoppingItem(referenceUid, 1, true)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.ResetShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingList? otherList = await _repository.GetShoppingListAsync(
        otherUid, "Weekly", TestContext.Current.CancellationToken);
      Assert.True(otherList!.Items.Single().IsChecked);
    }

    [Fact(
      DisplayName = "Resets temporary list when name is null")]
    public async Task ResetShoppingList_ResetsTemporaryList_WhenNameIsNull() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedPriceSnapshotAsync("Milk");
      _repository.AddItemsToList(userUid, null, [new AShoppingItem(referenceUid, 1, true)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.ResetShoppingList(userUid, null);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingList? list = await _repository.GetShoppingListAsync(
        userUid, null, TestContext.Current.CancellationToken);
      Assert.False(list!.Items.Single().IsChecked);
    }
  }
}
