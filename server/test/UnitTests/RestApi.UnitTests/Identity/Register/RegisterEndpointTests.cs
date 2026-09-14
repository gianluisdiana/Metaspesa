using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Application.Identity;
using Metaspesa.Domain.Identity;
using Metaspesa.RestApi.Identity;
using Metaspesa.RestApi.Identity.Register;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Metaspesa.RestApi.UnitTests.Identity.Register;

public static class RegisterEndpointTests {
  [Fact(DisplayName = "Registration returns 201 Created")]
  public static async Task Handle_ReturnsCreated_WhenUserIsRegistered() {
    IHasher hasher = Substitute.For<IHasher>();
    IUserRepository repository = Substitute.For<IUserRepository>();
    IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    hasher.Hash("SecurePass1!").Returns("hashed");
    var handler = new RegisterUser.Handler(hasher, repository, unitOfWork);

    IResult result = await RegisterEndpoint.HandleAsync(
      new CredentialsRequest("estela", "SecurePass1!"),
      handler,
      TestContext.Current.CancellationToken);

    Assert.Equal(
      StatusCodes.Status201Created,
      Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false).StatusCode);
  }

  [Fact(DisplayName = "Registration preserves username diacritics")]
  public static async Task Handle_PreservesUsernameDiacritics() {
    IHasher hasher = Substitute.For<IHasher>();
    IUserRepository repository = Substitute.For<IUserRepository>();
    IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    hasher.Hash("SecurePass1!").Returns("hashed");
    var handler = new RegisterUser.Handler(hasher, repository, unitOfWork);

    await RegisterEndpoint.HandleAsync(
      new CredentialsRequest("Café", "SecurePass1!"),
      handler,
      TestContext.Current.CancellationToken);

    repository.Received(1).SaveUser(Arg.Is<User>(user =>
      user.Username.Value == "Café"));
  }
}