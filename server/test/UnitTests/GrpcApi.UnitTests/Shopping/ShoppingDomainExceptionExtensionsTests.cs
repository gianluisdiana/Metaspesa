using Grpc.Core;
using Metaspesa.Domain.Shopping.Errors;
using Metaspesa.GrpcApi.Extensions;

namespace Metaspesa.GrpcApi.UnitTests.Shopping;

public class ShoppingDomainExceptionExtensionsTests {
  [Fact(DisplayName = "Maps missing Shopping state to NotFound")]
  public void ToRpcException_MapsMissingStateToNotFound() {
    var exception = new ShoppingListNotFoundException().ToRpcException();

    Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    Assert.Equal("ShoppingList.NotFound", exception.Status.Detail);
  }

  [Fact(DisplayName = "Maps Shopping conflict to AlreadyExists")]
  public void ToRpcException_MapsConflictToAlreadyExists() {
    var exception = new ShoppingListAlreadyExistsException().ToRpcException();

    Assert.Equal(StatusCode.AlreadyExists, exception.StatusCode);
  }

  [Fact(DisplayName = "Maps Shopping validation to InvalidArgument")]
  public void ToRpcException_MapsValidationToInvalidArgument() {
    var exception = new EmptyShoppingItemsException().ToRpcException();

    Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
  }
}
