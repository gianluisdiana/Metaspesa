using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Metaspesa.Application.Abstractions.Core;
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
      _service = new AuthGrpcService(_registerHandler);
    }

    [Fact(DisplayName = "Throws RpcException if the command handler returns a failure result")]
    public async Task Api_ThrowsRpcException_IfCommandHandlerFails() {
      // Arrange
      _registerHandler
        .Handle(Arg.Any<RegisterUser.Command>(), TestContext.Current.CancellationToken)
        .Returns(new DomainError(string.Empty, string.Empty, ErrorKind.Validation));

      var request = new RegisterRequest { Username = "alice", Password = "SecurePass1!" };

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

      var request = new RegisterRequest { Username = "alice", Password = "SecurePass1!" };

      // Act
      Empty response = await _service.Register(request, CreateServerCallContext());

      // Assert
      Assert.NotNull(response);
    }

    [Fact(DisplayName = "Maps username from request to command")]
    public async Task Api_MapsUsername_FromRequestToCommand() {
      // Arrange
      const string Username = "alice";
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

      var request = new RegisterRequest { Username = "alice", Password = Password };

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
}
