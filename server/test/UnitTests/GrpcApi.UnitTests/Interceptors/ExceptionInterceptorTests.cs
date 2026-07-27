using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Metaspesa.Database.Exceptions;
using Metaspesa.Domain.Identity.Errors;
using Metaspesa.GrpcApi.Interceptors;
using Metaspesa.GrpcApi.Protos.Auth;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Metaspesa.GrpcApi.UnitTests.Interceptors;

public class ExceptionInterceptorTests {
  private readonly ExceptionInterceptor _interceptor;

  public ExceptionInterceptorTests() {
    ILogger<ExceptionInterceptor> logger = Substitute.For<ILogger<ExceptionInterceptor>>();
    _interceptor = new ExceptionInterceptor(logger);
  }

  [Fact(DisplayName = "Rethrows RpcException without modification")]
  public async Task UnaryServerHandler_RethrowsRpcException_WithoutModification() {
    // Arrange
    var request = new RegisterRequest {
      Username = "estela",
      Password = "SecurePass1!"
    };
    var rpcException = new RpcException(new Status(StatusCode.InvalidArgument, "Invalid argument"));
    Task<Empty> continuation(RegisterRequest _, ServerCallContext __) => throw rpcException;

    // Act
    RpcException exception = await Assert.ThrowsAsync<RpcException>(() =>
      _interceptor.UnaryServerHandler(request, CreateServerCallContext(), continuation));

    // Assert
    Assert.Equal(rpcException, exception);
  }

  [Fact(DisplayName = "Throws cancelled RpcException when OperationCanceledException is thrown and cancellation is requested")]
  public async Task UnaryServerHandler_ThrowsCancelledRpcException_WhenCancellationIsRequested() {
    // Arrange
    var request = new RegisterRequest {
      Username = "estela",
      Password = "SecurePass1!"
    };
    static Task<Empty> continuation(RegisterRequest _, ServerCallContext __) =>
      throw new OperationCanceledException("Operation was canceled");

    using var cancellationTokenSource = new CancellationTokenSource();
    await cancellationTokenSource.CancelAsync();
    CancellationToken token = cancellationTokenSource.Token;

    // Act
    RpcException exception = await Assert.ThrowsAsync<RpcException>(() =>
      _interceptor.UnaryServerHandler(request, CreateServerCallContext(token), continuation));

    // Assert
    Assert.Equal(StatusCode.Cancelled, exception.StatusCode);
  }

  [Fact(DisplayName = "Throws internal RpcException when db exception is thrown")]
  public async Task UnaryServerHandler_ThrowsInternalRpcException_WhenDatabaseExceptionIsThrown() {
    // Arrange
    var request = new RegisterRequest {
      Username = "estela",
      Password = "SecurePass1!"
    };

    static Task<Empty> continuation(RegisterRequest _, ServerCallContext __) =>
      throw new DatabaseException("Database error");

    // Act
    RpcException exception = await Assert.ThrowsAsync<RpcException>(() =>
      _interceptor.UnaryServerHandler(request, CreateServerCallContext(), continuation));

    // Assert
    Assert.Equal(StatusCode.Internal, exception.StatusCode);
  }

  private sealed class IdentityDomainExceptions() : TheoryData<IdentityDomainException, StatusCode>([
    (new UsernameAlreadyExistsException(), StatusCode.AlreadyExists),
    (new InvalidCredentialsException(), StatusCode.Unauthenticated),
    (new InvalidPasswordHashException(), StatusCode.InvalidArgument),
    (new InvalidRoleException(), StatusCode.InvalidArgument),
    (new InvalidUserIdException(), StatusCode.InvalidArgument),
    (new InvalidUsernameException(), StatusCode.InvalidArgument),
    (new PasswordMissingDigitException(), StatusCode.InvalidArgument),
    (new PasswordMissingLowercaseException(), StatusCode.InvalidArgument),
    (new PasswordMissingSpecialCharacterException(), StatusCode.InvalidArgument),
    (new PasswordMissingUppercaseException(), StatusCode.InvalidArgument),
    (new PasswordTooShortException(), StatusCode.InvalidArgument),
  ]);

  [Theory(DisplayName = "Throws RpcException with correct status code when identity domain exception is thrown")]
  [ClassData<IdentityDomainExceptions>]
  public async Task UnaryServerHandler_ThrowsRpcException_WithCorrectStatusCode_WhenIdentityDomainExceptionIsThrown(
    IdentityDomainException originalException, StatusCode expectedStatusCode
  ) {
    // Arrange
    var request = new RegisterRequest {
      Username = "estela",
      Password = "SecurePass1!"
    };

    Task<Empty> continuation(RegisterRequest _, ServerCallContext __) =>
      throw originalException;

    // Act
    RpcException exception = await Assert.ThrowsAsync<RpcException>(() =>
      _interceptor.UnaryServerHandler(request, CreateServerCallContext(), continuation));

    // Assert
    Assert.Equal(expectedStatusCode, exception.StatusCode);
  }

  [Fact(DisplayName = "Throws internal RpcException when unhandled exception is thrown")]
  public async Task UnaryServerHandler_ThrowsInternalRpcException_WhenUnhandledExceptionIsThrown() {
    // Arrange
    var request = new RegisterRequest {
      Username = "estela",
      Password = "SecurePass1!"
    };
    static Task<Empty> continuation(RegisterRequest _, ServerCallContext __) =>
      throw new NotSupportedException("Unhandled exception");

    // Act
    RpcException exception = await Assert.ThrowsAsync<RpcException>(() =>
      _interceptor.UnaryServerHandler(request, CreateServerCallContext(), continuation));

    // Assert
    Assert.Equal(StatusCode.Internal, exception.StatusCode);
  }

  private static ServerCallContext CreateServerCallContext(
    CancellationToken? cancellationToken = null
  ) => TestServerCallContext.Create(
    method: string.Empty,
    host: string.Empty,
    deadline: DateTime.UtcNow.AddMinutes(1),
    requestHeaders: [],
    cancellationToken: cancellationToken ?? TestContext.Current.CancellationToken,
    peer: string.Empty,
    authContext: null!,
    contextPropagationToken: null!,
    writeHeadersFunc: _ => Task.CompletedTask,
    writeOptionsGetter: () => new WriteOptions(),
    writeOptionsSetter: _ => { });
}
