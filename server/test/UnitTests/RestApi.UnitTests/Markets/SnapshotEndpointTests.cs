using Metaspesa.Application.Markets;
using Metaspesa.RestApi.Markets;
using Microsoft.AspNetCore.Http;

namespace Metaspesa.RestApi.UnitTests.Markets;

public static class SnapshotEndpointTests {
  [Fact]
  public static void ToCommand_PreservesExistingFieldsAndSanitizesText() {
    var date = new DateOnly(2026, 9, 15);
    var request = new SnapshotRequest([
      new SnapshotItem("Milk\0", 1.04m, 1, "l\0", "Brand\0", "https://example.com/milk.jpg"),
      new SnapshotItem("Rice", 2.10m, 500, "g", "Brand", ""),
    ]);

    var command = SnapshotEndpoint.ToCommand("Market\0", date, request);

    Assert.Equal([
      new AddMarketProducts.CommandProduct("Milk", 1.04m, 1, "l", "Market", "Brand",
        new Uri("https://example.com/milk.jpg")),
      new AddMarketProducts.CommandProduct("Rice", 2.10m, 500, "g", "Market", "Brand", null),
    ], command.Products);
  }

  [Fact]
  public static void ToCommand_PreservesSnapshotDate() {
    var date = new DateOnly(2026, 9, 15);

    var command = SnapshotEndpoint.ToCommand("Market", date, new SnapshotRequest([]));

    Assert.Equal(date, command.RegisteredAt);
  }

  [Fact]
  public static void ToCommand_RejectsMissingItems() =>
    Assert.Throws<BadHttpRequestException>(() => SnapshotEndpoint.ToCommand(
      "Market", new DateOnly(2026, 9, 15), new SnapshotRequest(null)));

  [Fact]
  public static void ToCommand_RejectsNullItem() =>
    Assert.Throws<BadHttpRequestException>(() => SnapshotEndpoint.ToCommand(
      "Market", new DateOnly(2026, 9, 15), new SnapshotRequest([null])));

  [Fact]
  public static void ToCommand_RejectsInvalidImageUrl() =>
    Assert.Throws<BadHttpRequestException>(() => SnapshotEndpoint.ToCommand(
      "Market", new DateOnly(2026, 9, 15), new SnapshotRequest([
        new SnapshotItem("Milk", 1.04m, 1, "l", "Brand", "not a URL"),
      ])));
}