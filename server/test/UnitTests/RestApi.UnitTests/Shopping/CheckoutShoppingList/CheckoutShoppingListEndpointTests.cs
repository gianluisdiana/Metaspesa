using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Purchasing;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Purchasing;
using Metaspesa.Domain.Purchasing.Errors;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using Metaspesa.RestApi.Shopping.CheckoutShoppingList;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using static Metaspesa.RestApi.UnitTests.Shopping.ShoppingEndpointTestData;
using CheckoutUseCase = Metaspesa.Application.Purchasing.CheckoutShoppingList;

namespace Metaspesa.RestApi.UnitTests.Shopping.CheckoutShoppingList;

public static class CheckoutShoppingListEndpointTests {
  [Fact]
  public static async Task Checkout_ReturnsCreatedPurchaseId() {
    ShoppingList list = PersistedList("Weekly",
      new ShoppingItem(new ProductFormatId(FormatId),
        new PositiveAmount(2), true));
    IShoppingListRepository repository = RepositoryWith(list);
    IPurchasePriceSnapshotReader snapshots =
      Substitute.For<IPurchasePriceSnapshotReader>();
    snapshots.GetLatestAsync(Arg.Any<IReadOnlyCollection<ProductFormatId>>(),
      TestContext.Current.CancellationToken).Returns(
        new Dictionary<ProductFormatId, PriceSnapshotId> {
          [new ProductFormatId(FormatId)] = new PriceSnapshotId(44),
        });
    IPurchaseRepository purchases = Substitute.For<IPurchaseRepository>();
    purchases.AddAsync(Arg.Any<Purchase>(),
      TestContext.Current.CancellationToken).Returns(81);
    IClock clock = Substitute.For<IClock>();
    clock.GetCurrentTime().Returns(
      new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc));

    IResult result = await CheckoutShoppingListEndpoint.CheckoutAsync(ListId,
      ShopperContext(), new CheckoutUseCase.Handler(repository,
        snapshots, purchases, clock), TestContext.Current.CancellationToken);

    Assert.Equal((StatusCodes.Status201Created, 81),
      (Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode,
        Assert.IsAssignableFrom<IValueHttpResult<PurchaseResponse>>(result)
          .Value!.PurchaseId));
  }

  [Fact]
  public static async Task Checkout_RejectsListWithoutCheckedItems() {
    ShoppingList list = PersistedList("Weekly");
    IShoppingListRepository repository = RepositoryWith(list);

    await Assert.ThrowsAsync<EmptyPurchaseItemsException>(() =>
      CheckoutShoppingListEndpoint.CheckoutAsync(ListId,
        ShopperContext(), new CheckoutUseCase.Handler(repository,
          Substitute.For<IPurchasePriceSnapshotReader>(),
          Substitute.For<IPurchaseRepository>(), Substitute.For<IClock>()),
        TestContext.Current.CancellationToken));
  }
}