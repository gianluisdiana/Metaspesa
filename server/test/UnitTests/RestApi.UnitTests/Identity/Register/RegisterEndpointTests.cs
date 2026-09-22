using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Identity;
using Metaspesa.RestApi.Identity;
using Metaspesa.RestApi.Identity.Register;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Metaspesa.RestApi.UnitTests.Identity.Register;

public class RegisterEndpointTests {
  private sealed class FakeRegisterUserHandler : RegisterUser.Handler {
    public FakeRegisterUserHandler(
      IHasher hasher, IUserRepository repository
    ) : base(hasher, repository) {
      hasher.Hash("SecurePass1!").Returns("hashed");
    }
  }

  private readonly RegisterUser.Handler _handler;

  public RegisterEndpointTests() {
    _handler = new FakeRegisterUserHandler(
      Substitute.For<IHasher>(),
      Substitute.For<IUserRepository>());
  }

  [Fact(DisplayName = "Registration returns 201 Created")]
  public async Task Handle_ReturnsCreated_WhenUserIsRegistered() {
    var request = new CredentialsRequest("estela", "SecurePass1!");

    IResult result = await RegisterEndpoint.HandleAsync(
      request,
      _handler,
      TestContext.Current.CancellationToken);

    Assert.Equal(
      StatusCodes.Status201Created,
      Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false).StatusCode);
  }
}