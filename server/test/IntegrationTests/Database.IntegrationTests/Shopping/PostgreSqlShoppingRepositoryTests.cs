using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Database.Entities;
using Metaspesa.Database.Repositories;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Users;
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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

    private async Task<int> SeedProductHistoryAsync(string productName) {
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
      var history = new ProductsHistoryDbEntity {
        Product = product,
        ProductFormat = format,
        Price = 1.25m,
        CreatedAt = DateTime.UtcNow,
      };

      _context.ProductsHistory.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return history.Id;
    }

    [Fact(
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      int milkReferenceUid = await SeedProductHistoryAsync("Milk");
      int breadReferenceUid = await SeedProductHistoryAsync("Bread");
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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

    private async Task<int> SeedProductHistoryAsync(
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
      var history = new ProductsHistoryDbEntity {
        Product = product,
        ProductFormat = format,
        Price = price,
        CreatedAt = DateTime.UtcNow,
      };

      _context.ProductsHistory.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return history.Id;
    }

    [Fact(
      Explicit = true,
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
      int milkReferenceUid = await SeedProductHistoryAsync("Milk");
      int breadReferenceUid = await SeedProductHistoryAsync("Bread");

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
      Explicit = true,
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
      int butterReferenceUid = await SeedProductHistoryAsync("Butter");

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
      Explicit = true,
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
      int milkReferenceUid = await SeedProductHistoryAsync("Milk");

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
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");

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
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");

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

    private async Task<int> SeedProductHistoryAsync(string productName) {
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
      var history = new ProductsHistoryDbEntity {
        Product = product,
        ProductFormat = format,
        Price = 1.25m,
        CreatedAt = DateTime.UtcNow,
      };

      _context.ProductsHistory.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return history.Id;
    }

    [Fact(
      Explicit = true,
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
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");
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
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");
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
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");
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

    private async Task<int> SeedProductHistoryAsync(string productName) {
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
      var history = new ProductsHistoryDbEntity {
        Product = product,
        ProductFormat = format,
        Price = 1.25m,
        CreatedAt = DateTime.UtcNow,
      };

      _context.ProductsHistory.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return history.Id;
    }

    [Fact(
      Explicit = true,
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
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");
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
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");
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
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");
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

    [Fact(
      Explicit = true,
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
      Explicit = true,
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
      Explicit = true,
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
      _repository.AddItemsToList(
        userUid, null, [new AShoppingItem(1, 1, false)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.UpdateShoppingListName(userUid, null, "Groceries");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      AShoppingList? list = await _repository.GetShoppingListAsync(
        userUid, "Groceries", TestContext.Current.CancellationToken);
      Assert.Equal(1, list!.Items.Single().ReferenceUid);
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

    private async Task<int> SeedProductHistoryAsync(string productName) {
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
      var history = new ProductsHistoryDbEntity {
        Product = product,
        ProductFormat = format,
        Price = 1.25m,
        CreatedAt = DateTime.UtcNow,
      };

      _context.ProductsHistory.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return history.Id;
    }

    [Fact(
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");
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
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");
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
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");
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
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");
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

    private async Task<int> SeedProductHistoryAsync(string productName) {
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
      var history = new ProductsHistoryDbEntity {
        Product = product,
        ProductFormat = format,
        Price = 1.25m,
        CreatedAt = DateTime.UtcNow,
      };

      _context.ProductsHistory.Add(history);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      return history.Id;
    }

    [Fact(
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");
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
          i => i.Product.Name == "Milk" && i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid),
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
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      int referenceUid = await SeedProductHistoryAsync("Milk");
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
      Explicit = true,
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
      int referenceUid = await SeedProductHistoryAsync("Milk");
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

    [Fact(
      Explicit = true,
      DisplayName = "Soft-deletes checked items after recording")]
    public async Task RecordShoppingList_SoftDeletesCheckedItems_AfterRecording() {
      // Arrange
      var userUid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(userUid, "Weekly", [
        new AShoppingItem(1, 1, true),
        new AShoppingItem(2, 1, false),
      ]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.RecordShoppingList(userUid, new ShoppingList("Weekly", [
        new ShoppingItem("Milk", null, new Price(1.5m), true),
        new ShoppingItem("Bread", null, new Price(2.0m), false),
      ]));

      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      ShoppingItemDbEntity milk = await _context.ShoppingItems
        .AsNoTracking()
        .FirstAsync(
          i => i.Product.Name == "Milk" && i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid),
          TestContext.Current.CancellationToken);
      ShoppingItemDbEntity bread = await _context.ShoppingItems
        .AsNoTracking()
        .FirstAsync(
          i => i.Product.Name == "Bread" && i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid),
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
        Uid = userUid, Username = userUid.ToString(), EncryptedPassword = "x", RoleId = 1
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.CreateShoppingList(userUid, "Weekly");
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      _repository.AddItemsToList(
        userUid, "Weekly", [new AShoppingItem(1, 1, true)]);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      _repository.RecordShoppingList(userUid, new ShoppingList("Weekly", [
        new ShoppingItem("Eggs", null, new Price(3.0m), true),
      ]));

      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      int count = await _context.Purchases
        .AsNoTracking()
        .CountAsync(p => p.UserUid == userUid, TestContext.Current.CancellationToken);
      Assert.Equal(1, count);
    }
  }
}
