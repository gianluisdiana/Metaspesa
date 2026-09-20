using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Metaspesa.RestApi.UnitTests.Shopping;

internal static class ShoppingEndpointTestData {
  internal static readonly Guid OwnerUid =
    Guid.Parse("e5740719-bf47-4f79-9005-0dd70c522f14");
  internal const int ListId = 17;
  internal const int FormatId = 31;

  internal static DefaultHttpContext ShopperContext() => new() {
    User = new ClaimsPrincipal(new ClaimsIdentity([
      new Claim(JwtRegisteredClaimNames.Sub, OwnerUid.ToString()),
    ])),
  };

  internal static IShoppingListRepository RepositoryWith(ShoppingList list) {
    IShoppingListRepository repository = Substitute.For<IShoppingListRepository>();
    repository.GetAsync(new UserId(OwnerUid), new ShoppingListId(ListId),
      TestContext.Current.CancellationToken).Returns(list);
    return repository;
  }

  internal static ShoppingList PersistedList(
    string? name, params ShoppingItem[] items) => ShoppingList.Rehydrate(
      new ShoppingListId(ListId), [new UserId(OwnerUid)],
      name is null ? null : new ShoppingListName(name), null, items);

  internal static MarketProduct Product() => Product(FormatId);

  internal static MarketProduct Product(int formatId) => new("Milk", "Brand", [
    new MarketProductFormat(new Quantity(1, new UnitOfMeasure("l")),
      new Money(1.25m), new Uri("https://example.test/milk"), formatId),
  ], new MarketSummary(4, "Market", null));
}