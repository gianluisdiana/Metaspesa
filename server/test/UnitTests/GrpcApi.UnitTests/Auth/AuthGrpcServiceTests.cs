using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Auth;
using Metaspesa.GrpcApi.Protos.Auth;
using Metaspesa.GrpcApi.Services;
using NSubstitute;

namespace Metaspesa.GrpcApi.UnitTests.Auth;

public static class AuthGrpcServiceTests {
  public class RegisterRpc {
    private readonly ICommandHandler<RegisterUser.Command> _registerHandler;
    private readonly AuthGrpcService _service;

    public RegisterRpc() {
      _registerHandler = Substitute.For<ICommandHandler<RegisterUser.Command>>();
      _service = new AuthGrpcService(
        _registerHandler,
        Substitute.For<IQueryHandler<LoginUser.Query, Token>>());
    }

    [Fact(DisplayName = "Throws RpcException if the command handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfCommandHandlerFails() {
      // Arrange
      _registerHandler
        .Handle(Arg.Any<RegisterUser.Command>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Validation));

      var request = new RegisterRequest { Username = "estela", Password = "SecurePass1!" };

      // Act
      async Task action() => await _service.Register(request, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns Empty when handler succeeds")]
    public async Task Api_ReturnsEmpty_WhenHandlerSucceeds() {
      // Arrange
      _registerHandler
        .Handle(Arg.Any<RegisterUser.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

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
      _registerHandler
        .Handle(Arg.Any<RegisterUser.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new RegisterRequest { Username = Username, Password = "SecurePass1!" };

      // Act
      await _service.Register(request, CreateServerCallContext());

      // Assert
      await _registerHandler.Received(1).Handle(
        Arg.Is<RegisterUser.Command>(cmd => cmd.Username == Username),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps password from request to command")]
    public async Task Api_MapsPassword_FromRequestToCommand() {
      // Arrange
      const string Password = "SecurePass1!";
      _registerHandler
        .Handle(Arg.Any<RegisterUser.Command>(), TestContext.Current.CancellationToken)
        .Returns(Result.Success());

      var request = new RegisterRequest { Username = "estela", Password = Password };

      // Act
      await _service.Register(request, CreateServerCallContext());

      // Assert
      await _registerHandler.Received(1).Handle(
        Arg.Is<RegisterUser.Command>(cmd => cmd.Password == Password),
        TestContext.Current.CancellationToken);
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

  public class LoginRpc {
    private readonly IQueryHandler<LoginUser.Query, Token> _loginHandler;
    private readonly AuthGrpcService _service;

    public LoginRpc() {
      _loginHandler = Substitute.For<IQueryHandler<LoginUser.Query, Token>>();
      _service = new AuthGrpcService(
        Substitute.For<ICommandHandler<RegisterUser.Command>>(),
        _loginHandler);
    }

    [Fact(DisplayName = "Throws RpcException when credentials are invalid")]
    public async Task Api_ThrowsRpcException_WhenCredentialsInvalid() {
      // Arrange
      _loginHandler
        .Handle(Arg.Any<LoginUser.Query>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Unauthenticated));

      var request = new LoginRequest { Username = "estela", Password = "wrong" };

      // Act
      async Task action() => await _service.Login(request, CreateServerCallContext());

      // Assert
      await Assert.ThrowsAsync<RpcException>(action);
    }

    [Fact(DisplayName = "Returns LoginResponse with token when credentials are valid")]
    public async Task Api_ReturnsLoginResponse_WithToken_WhenCredentialsValid() {
      // Arrange
      var token = new Token("jwt-token-value", DateTime.UtcNow.AddHours(1));
      
      _loginHandler
        .Handle(Arg.Any<LoginUser.Query>(), TestContext.Current.CancellationToken)
        .Returns(token);

      var request = new LoginRequest { Username = "estela", Password = "SecurePass1!" };

      // Act
      LoginResponse response = await _service.Login(request, CreateServerCallContext());

      // Assert
      Assert.Equal(token.Value, response.Token);
    }

    [Fact(DisplayName = "Returns LoginResponse with expiration when credentials are valid")]
    public async Task Api_ReturnsLoginResponse_WithExpiration_WhenCredentialsValid() {
      // Arrange
      var token = new Token("jwt-token-value", DateTime.UtcNow.AddHours(1));

      _loginHandler
        .Handle(Arg.Any<LoginUser.Query>(), TestContext.Current.CancellationToken)
        .Returns(token);

      var request = new LoginRequest { Username = "estela", Password = "SecurePass1!" };

      // Act
      LoginResponse response = await _service.Login(request, CreateServerCallContext());

      // Assert
      Assert.Equal(token.ExpiresAt.ToString("o"), response.ExpirationInUtc);
    }

    [Fact(DisplayName = "Maps username from request to query")]
    public async Task Api_MapsUsername_FromRequestToQuery() {
      // Arrange
      const string Username = "estela";
      _loginHandler
        .Handle(Arg.Any<LoginUser.Query>(), TestContext.Current.CancellationToken)
        .Returns(new Token("token", DateTime.UtcNow.AddHours(1)));

      var request = new LoginRequest { Username = Username, Password = "SecurePass1!" };

      // Act
      await _service.Login(request, CreateServerCallContext());

      // Assert
      await _loginHandler.Received(1).Handle(
        Arg.Is<LoginUser.Query>(q => q.Username == Username),
        TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Maps password from request to query")]
    public async Task Api_MapsPassword_FromRequestToQuery() {
      // Arrange
      const string Password = "SecurePass1!";
      _loginHandler
        .Handle(Arg.Any<LoginUser.Query>(), TestContext.Current.CancellationToken)
        .Returns(new Token("token", DateTime.UtcNow.AddHours(1)));

      var request = new LoginRequest { Username = "estela", Password = Password };

      // Act
      await _service.Login(request, CreateServerCallContext());

      // Assert
      await _loginHandler.Received(1).Handle(
        Arg.Is<LoginUser.Query>(q => q.Password == Password),
        TestContext.Current.CancellationToken);
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
}
