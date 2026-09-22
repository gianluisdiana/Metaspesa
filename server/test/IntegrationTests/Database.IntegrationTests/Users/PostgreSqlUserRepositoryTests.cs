using Metaspesa.Database.Entities;
using Metaspesa.Database.Repositories;
using Metaspesa.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.IntegrationTests.Users;

public static class PostgreSqlUserRepositoryTests {
  private static readonly UserRoleDbEntity TestRole = new() {
    Name = "TestRole",
    Description = "Role for testing",
  };

  [Collection("Database")]
  public class CheckUsernameExistsAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlUserRepository _repository;

    public CheckUsernameExistsAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlUserRepository(
        _context);
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
      DisplayName = "Returns false when username does not exist")]
    public async Task CheckUsernameExistsAsync_ReturnsFalse_WhenUsernameDoesNotExist() {
      // Act
      bool result = await _repository.CheckUsernameExistsAsync(
        new Username("nonexistent"), TestContext.Current.CancellationToken);

      // Assert
      Assert.False(result);
    }

    [Fact(
      DisplayName = "Returns true when username exists")]
    public async Task CheckUsernameExistsAsync_ReturnsTrue_WhenUsernameExists() {
      // Arrange
      var uid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = uid, Username = "estela", EncryptedPassword = "x", Role = TestRole
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckUsernameExistsAsync(
        new Username("estela"), TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }

    [Fact(
      DisplayName = "Returns true case-insensitively")]
    public async Task CheckUsernameExistsAsync_ReturnsTrue_CaseInsensitively() {
      // Arrange
      var uid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = uid, Username = "Alice", EncryptedPassword = "x", Role = TestRole
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckUsernameExistsAsync(
        new Username("ALICE"), TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }
  }

  [Collection("Database")]
  public class SaveUser : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlUserRepository _repository;

    public SaveUser(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlUserRepository(
        _context);
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
      DisplayName = "Persists user so username can be found afterwards")]
    public async Task SaveUser_PersistsUser_SoUsernameCanBeFound() {
      // Arrange
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = Role.Shopper.ToString(),
        Description = "Shopper role for testing",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      User user = CreateUser("bob", "hashed_password");

      // Act
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      bool exists = await _repository.CheckUsernameExistsAsync(
        new Username("bob"), TestContext.Current.CancellationToken);
      Assert.True(exists);
    }

    [Fact(
      DisplayName = "Persists encrypted password correctly")]
    public async Task SaveUser_PersistsEncryptedPassword_Correctly() {
      // Arrange
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = Role.Shopper.ToString(),
        Description = "Shopper role for testing",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      const string HashedPassword = "hashed_password_value";
      User user = CreateUser("carol", HashedPassword);

      // Act
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      User? retrieved = await _repository.GetUserAsync(
        new Username("carol"), TestContext.Current.CancellationToken);
      Assert.Equal(HashedPassword, retrieved!.PasswordHash.Value);
    }

    [Fact(
      DisplayName = "Persists Shopper role correctly")]
    public async Task SaveUser_PersistsShopperRole_Correctly() {
      // Arrange
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = Role.Shopper.ToString(),
        Description = "Shopper role for testing",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      User user = CreateUser("dave", "hashed_password");

      // Act
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      User? retrieved = await _repository.GetUserAsync(
        new Username("dave"), TestContext.Current.CancellationToken);
      Assert.Equal(Role.Shopper, retrieved!.Role);
    }
  }

  [Collection("Database")]
  public class GetUserByUsernameAsync : IAsyncLifetime {
    private readonly MainContext _context;
    private readonly PostgreSqlUserRepository _repository;

    public GetUserByUsernameAsync(DatabaseFixture fixture) {
      _context = fixture.CreateContext();
      _repository = new PostgreSqlUserRepository(
        _context);
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
      DisplayName = "Returns null when user does not exist")]
    public async Task GetUserByUsernameAsync_ReturnsNull_WhenUserDoesNotExist() {
      // Act
      User? result = await _repository.GetUserAsync(
        new Username("nobody"), TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result);
    }

    [Fact(
      DisplayName = "Returns user when username matches")]
    public async Task GetUserByUsernameAsync_ReturnsUser_WhenUsernameMatches() {
      // Arrange
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = Role.Shopper.ToString(),
        Description = "Shopper role for testing",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      User user = CreateUser("eve", "hashed");
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      User? result = await _repository.GetUserAsync(
        new Username("eve"), TestContext.Current.CancellationToken);

      // Assert
      Assert.NotNull(result);
      Assert.Equal("eve", result.Username.Value);
    }

    [Fact(
      DisplayName = "Returns user case-insensitively")]
    public async Task GetUserByUsernameAsync_ReturnsUser_CaseInsensitively() {
      // Arrange
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = Role.Shopper.ToString(),
        Description = "Shopper role for testing",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      User user = CreateUser("Frank", "hashed");
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      User? result = await _repository.GetUserAsync(
        new Username("FRANK"), TestContext.Current.CancellationToken);

      // Assert
      Assert.NotNull(result);
    }

    [Fact(
      DisplayName = "Returns correct role for user")]
    public async Task GetUserByUsernameAsync_ReturnsCorrectRole_ForUser() {
      // Arrange
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = Role.Shopper.ToString(),
        Description = "Shopper role for testing",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      User user = CreateUser("grace", "hashed");
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      User? result = await _repository.GetUserAsync(
        new Username("grace"), TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(Role.Shopper, result!.Role);
    }

    [Fact(
      DisplayName = "Returns correct Uid for saved user")]
    public async Task GetUserByUsernameAsync_ReturnsCorrectUid_ForSavedUser() {
      // Arrange
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = Role.Shopper.ToString(),
        Description = "Shopper role for testing",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      var expectedUid = Guid.CreateVersion7();
      User user = CreateUser(expectedUid, "henry", "hashed");
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      User? result = await _repository.GetUserAsync(
        new Username("henry"), TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(expectedUid, result!.Id.Value);
    }
  }

  private static User CreateUser(string username, string passwordHash) =>
    CreateUser(Guid.CreateVersion7(), username, passwordHash);

  private static User CreateUser(Guid id, string username, string passwordHash) =>
    User.CreateShopper(
      new UserId(id),
      new Username(username),
      new PasswordHash(passwordHash));
}