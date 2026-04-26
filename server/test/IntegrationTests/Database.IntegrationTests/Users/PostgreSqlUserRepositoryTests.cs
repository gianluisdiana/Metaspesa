using Metaspesa.Database.Entities;
using Metaspesa.Database.Repositories;
using Metaspesa.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

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
        _context, NullLogger<PostgreSqlUserRepository>.Instance);
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
      DisplayName = "Returns false when username does not exist")]
    public async Task CheckUsernameExistsAsync_ReturnsFalse_WhenUsernameDoesNotExist() {
      // Act
      bool result = await _repository.CheckUsernameExistsAsync(
        "nonexistent", TestContext.Current.CancellationToken);

      // Assert
      Assert.False(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns true when username exists")]
    public async Task CheckUsernameExistsAsync_ReturnsTrue_WhenUsernameExists() {
      // Arrange
      var uid = Guid.CreateVersion7();
      _context.Users.Add(new UserDbEntity {
        Uid = uid, Username = "alice", EncryptedPassword = "x", Role = TestRole
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      bool result = await _repository.CheckUsernameExistsAsync(
        "alice", TestContext.Current.CancellationToken);

      // Assert
      Assert.True(result);
    }

    [Fact(
      Explicit = true,
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
        "ALICE", TestContext.Current.CancellationToken);

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
        _context, NullLogger<PostgreSqlUserRepository>.Instance);
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
      DisplayName = "Persists user so username can be found afterwards")]
    public async Task SaveUser_PersistsUser_SoUsernameCanBeFound() {
      // Arrange
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = Role.Shopper.ToString(),
        Description = "Shopper role for testing",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
      var user = new User("bob", "hashed_password", Role.Shopper);

      // Act
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      bool exists = await _repository.CheckUsernameExistsAsync(
        "bob", TestContext.Current.CancellationToken);
      Assert.True(exists);
    }

    [Fact(
      Explicit = true,
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
      var user = new User("carol", HashedPassword, Role.Shopper);

      // Act
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      User? retrieved = await _repository.GetUserByUsernameAsync(
        "carol", TestContext.Current.CancellationToken);
      Assert.Equal(HashedPassword, retrieved!.EncryptedPassword);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Persists Shopper role correctly")]
    public async Task SaveUser_PersistsShopperRole_Correctly() {
      // Arrange
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = Role.Shopper.ToString(),
        Description = "Shopper role for testing",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      var user = new User("dave", "hashed_password", Role.Shopper);

      // Act
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Assert
      User? retrieved = await _repository.GetUserByUsernameAsync(
        "dave", TestContext.Current.CancellationToken);
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
        _context, NullLogger<PostgreSqlUserRepository>.Instance);
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
      DisplayName = "Returns null when user does not exist")]
    public async Task GetUserByUsernameAsync_ReturnsNull_WhenUserDoesNotExist() {
      // Act
      User? result = await _repository.GetUserByUsernameAsync(
        "nobody", TestContext.Current.CancellationToken);

      // Assert
      Assert.Null(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns user when username matches")]
    public async Task GetUserByUsernameAsync_ReturnsUser_WhenUsernameMatches() {
      // Arrange
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = Role.Shopper.ToString(),
        Description = "Shopper role for testing",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      var user = new User("eve", "hashed", Role.Shopper);
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      User? result = await _repository.GetUserByUsernameAsync(
        "eve", TestContext.Current.CancellationToken);

      // Assert
      Assert.NotNull(result);
      Assert.Equal("eve", result.Username);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns user case-insensitively")]
    public async Task GetUserByUsernameAsync_ReturnsUser_CaseInsensitively() {
      // Arrange
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = Role.Shopper.ToString(),
        Description = "Shopper role for testing",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      var user = new User("Frank", "hashed", Role.Shopper);
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      User? result = await _repository.GetUserByUsernameAsync(
        "FRANK", TestContext.Current.CancellationToken);

      // Assert
      Assert.NotNull(result);
    }

    [Fact(
      Explicit = true,
      DisplayName = "Returns correct role for user")]
    public async Task GetUserByUsernameAsync_ReturnsCorrectRole_ForUser() {
      // Arrange
      _context.UserRoles.Add(new UserRoleDbEntity {
        Id = (int)Role.Shopper,
        Name = Role.Shopper.ToString(),
        Description = "Shopper role for testing",
      });
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      var user = new User("grace", "hashed", Role.Shopper);
      _repository.SaveUser(user);
      await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

      // Act
      User? result = await _repository.GetUserByUsernameAsync(
        "grace", TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(Role.Shopper, result!.Role);
    }
  }
}
