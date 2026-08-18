using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Application.Shopping;

internal static class ShoppingListRequest {
  public static UserId Owner(Guid userUid) => new(userUid);

  public static ShoppingListName? Name(string? value) =>
    string.IsNullOrWhiteSpace(value) ? null : new ShoppingListName(value);

  public static ShoppingListNotFoundException NotFound() => new();
}
