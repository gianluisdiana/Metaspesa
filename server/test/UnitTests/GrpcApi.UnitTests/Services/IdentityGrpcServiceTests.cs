using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Identity;
using Metaspesa.Domain.Identity;
using Metaspesa.GrpcApi.Protos.Auth;
using Metaspesa.GrpcApi.Services;
using NSubstitute;

namespace Metaspesa.GrpcApi.UnitTests.Services;

public static class IdentityGrpcServiceTests {
  public class RegisterRpc {
    private readonly IHasher _hasher;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IdentityGrpcService _service;

    public RegisterRpc() {
      _hasher = Substitute.For<IHasher>();
      _userRepository = Substitute.For<IUserRepository>();
      _unitOfWork = Substitute.For<IUnitOfWork>();
      _hasher.Hash(Arg.Any<string>()).Returns("hashed");

      _service = new IdentityGrpcService(
        new RegisterUser.Handler(_hasher, _userRepository, _unitOfWork),
        new LoginUser.Handler(
          Substitute.For<IUserRepository>(),
          Substitute.For<IHasher>(),
          Substitute.For<ITokenProvider>()));
    }

    [Fact(DisplayName = "Throws exception if handler throws domain exception")]
    public async Task Api_ThrowsException_IfHandlerThrowsException() {
      // Arrange
      _userRepository
        .CheckUsernameExistsAsync(Arg.Any<Username>(), TestContext.Current.CancellationToken)
        .Returns(true);

      var request = new RegisterRequest { Username = "estela", Password = "SecurePass1!" };

      // Act
      async Task action() => await _service.Register(request, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAnyAsync<Exception>(action);
    }

    [Fact(DisplayName = "Returns Empty when handler succeeds")]
    public async Task Api_ReturnsEmpty_WhenHandlerSucceeds() {
      // Arrange
      var request = new RegisterRequest { Username = "estela", Password = "SecurePass1!" };

      // Act
      Empty response = await _service.Register(request, CreateServerCallContext());

      // Assert
      Assert.NotNull(response);
    }

    [Fact(DisplayName = "Maps username from request to command")]
    public async Task Api_MapsUsername_FromRequestToCommand() {
      // Arrange
      const string Username = "estela";
      var request = new RegisterRequest { Username = Username, Password = "SecurePass1!" };

      // Act
      await _service.Register(request, CreateServerCallContext());

      // Assert
      await _userRepository.Received(1).CheckUsernameExistsAsync(
        Arg.Is<Username>(username => username.Value == Username),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps password from request to handler")]
    public async Task Api_MapsPassword_FromRequestToCommand() {
      // Arrange
      const string Password = "SecurePass1!";
      var request = new RegisterRequest { Username = "estela", Password = Password };

      // Act
      await _service.Register(request, CreateServerCallContext());

      // Assert
      _hasher.Received(1).Hash(Password);
    }

    [Fact(DisplayName = "Sanitizes non-ASCII username before creating command")]
    public async Task Api_SanitizesNonAsciiUsername_BeforeCreatingCommand() {
      // Arrange
      var request = new RegisterRequest { Username = "Est\u00e9la \u2713", Password = "SecurePass1!" };

      // Act
      await _service.Register(request, CreateServerCallContext());

      // Assert
      await _userRepository.Received(1).CheckUsernameExistsAsync(
        Arg.Is<Username>(username => username.Value == "Estela "),
        TestContext.Current.CancellationToken);
    }
  }

  public class LoginRpc {
    private readonly IUserRepository _userRepository;
    private readonly IHasher _hasher;
    private readonly ITokenProvider _tokenProvider;
    private readonly IdentityGrpcService _service;

    public LoginRpc() {
      _userRepository = Substitute.For<IUserRepository>();
      _hasher = Substitute.For<IHasher>();
      _tokenProvider = Substitute.For<ITokenProvider>();
      _service = new IdentityGrpcService(
        new RegisterUser.Handler(
          Substitute.For<IHasher>(),
          Substitute.For<IUserRepository>(),
          Substitute.For<IUnitOfWork>()),
        new LoginUser.Handler(_userRepository, _hasher, _tokenProvider));
    }

    [Fact(DisplayName = "Throws exception when credentials are invalid")]
    public async Task Api_ThrowsException_WhenCredentialsInvalid() {
      // Arrange
      var request = new LoginRequest { Username = "estela", Password = "wrong" };

      // Act
      async Task action() => await _service.Login(request, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAnyAsync<Exception>(action);
    }

    [Fact(DisplayName = "Returns LoginResponse with token when credentials are valid")]
    public async Task Api_ReturnsLoginResponse_WithToken_WhenCredentialsValid() {
      // Arrange
      User user = TestUser();
      var token = new Token("jwt-token-value", DateTime.UtcNow.AddHours(1));
      _userRepository
        .GetUserByUsernameAsync(Arg.Any<Username>(), TestContext.Current.CancellationToken)
        .Returns(user);
      _hasher.VerifyHash("SecurePass1!", user.PasswordHash.Value).Returns(true);
      _tokenProvider.GenerateToken(user).Returns(token);

      var request = new LoginRequest { Username = "estela", Password = "SecurePass1!" };

      // Act
      LoginResponse response = await _service.Login(request, CreateServerCallContext());

      // Assert
      Assert.Equal(token.Value, response.Token);
    }

    [Fact(DisplayName = "Returns LoginResponse with expiration when credentials are valid")]
    public async Task Api_ReturnsLoginResponse_WithExpiration_WhenCredentialsValid() {
      // Arrange
      User user = TestUser();
      var token = new Token("jwt-token-value", DateTime.UtcNow.AddHours(1));
      _userRepository
        .GetUserByUsernameAsync(Arg.Any<Username>(), TestContext.Current.CancellationToken)
        .Returns(user);
      _hasher.VerifyHash("SecurePass1!", user.PasswordHash.Value).Returns(true);
      _tokenProvider.GenerateToken(user).Returns(token);

      var request = new LoginRequest { Username = "estela", Password = "SecurePass1!" };

      // Act
      LoginResponse response = await _service.Login(request, CreateServerCallContext());

      // Assert
      Assert.Equal(token.ExpiresAt.ToString("o"), response.ExpirationInUtc);
    }

    [Fact(DisplayName = "Maps username from request to query")]
    public async Task Api_MapsUsername_FromRequestToQuery() {
      // Arrange
      User user = TestUser();
      _userRepository
        .GetUserByUsernameAsync(Arg.Any<Username>(), TestContext.Current.CancellationToken)
        .Returns(user);
      _hasher.VerifyHash("SecurePass1!", user.PasswordHash.Value).Returns(true);
      _tokenProvider.GenerateToken(user).Returns(new Token("token", DateTime.UtcNow.AddHours(1)));

      const string Username = "estela";
      var request = new LoginRequest { Username = Username, Password = "SecurePass1!" };

      // Act
      await _service.Login(request, CreateServerCallContext());

      // Assert
      await _userRepository.Received(1).GetUserByUsernameAsync(
        Arg.Is<Username>(username => username.Value == Username),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps password from request to query")]
    public async Task Api_MapsPassword_FromRequestToQuery() {
      // Arrange
      User user = TestUser();
      _userRepository
        .GetUserByUsernameAsync(Arg.Any<Username>(), TestContext.Current.CancellationToken)
        .Returns(user);
      _hasher.VerifyHash("SecurePass1!", user.PasswordHash.Value).Returns(true);
      _tokenProvider.GenerateToken(user).Returns(new Token("token", DateTime.UtcNow.AddHours(1)));

      const string Password = "SecurePass1!";
      var request = new LoginRequest { Username = "estela", Password = Password };

      // Act
      await _service.Login(request, CreateServerCallContext());

      // Assert
      _hasher.Received(1).VerifyHash(Password, user.PasswordHash.Value);
    }

    [Fact(DisplayName = "Sanitizes non-ASCII username before creating query")]
    public async Task Api_SanitizesNonAsciiUsername_BeforeCreatingQuery() {
      // Arrange
      User user = TestUser();
      _userRepository
        .GetUserByUsernameAsync(Arg.Any<Username>(), TestContext.Current.CancellationToken)
        .Returns(user);
      _hasher.VerifyHash("SecurePass1!", user.PasswordHash.Value).Returns(true);
      _tokenProvider.GenerateToken(user).Returns(new Token("token", DateTime.UtcNow.AddHours(1)));

      var request = new LoginRequest { Username = "Est\u00e9la \u2713", Password = "SecurePass1!" };

      // Act
      await _service.Login(request, CreateServerCallContext());

      // Assert
      await _userRepository.Received(1).GetUserByUsernameAsync(
        Arg.Is<Username>(username => username.Value == "Estela "),
        TestContext.Current.CancellationToken);
    }

    private static User TestUser() =>
      User.CreateShopper(
        new UserId(Guid.CreateVersion7()),
        new Username("estela"),
        new PasswordHash("hashed"));
  }

  private static ServerCallContext CreateServerCallContext() => TestServerCallContext.Create(
    method: string.Empty,
    host: string.Empty,
    deadline: DateTime.UtcNow.AddMinutes(1),
    requestHeaders: [],
    cancellationToken: TestContext.Current.CancellationToken,
    peer: string.Empty,
    authContext: null!,
    contextPropagationToken: null!,
    writeHeadersFunc: _ => Task.CompletedTask,
    writeOptionsGetter: () => new WriteOptions(),
    writeOptionsSetter: _ => { });
}
