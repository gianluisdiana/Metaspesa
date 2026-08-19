using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using static Metaspesa.Application.Markets.AddMarketProducts;

namespace Metaspesa.Application.UnitTests.Markets;

public class AddProductsHandlerTest {
  private readonly IMarketRepository _marketRepository;
  private readonly IProductRepository _productRepository;
  private readonly IPriceSnapshotRepository _snapshotRepository;
  private readonly Handler _handler;

  public AddProductsHandlerTest() {
    _marketRepository = Substitute.For<IMarketRepository>();
    _productRepository = Substitute.For<IProductRepository>();
    _snapshotRepository = Substitute.For<IPriceSnapshotRepository>();

    IServiceScopeFactory scopeFactory = new ServiceCollection()
      .AddSingleton(_marketRepository)
      .AddSingleton(_productRepository)
      .AddSingleton(_snapshotRepository)
      .BuildServiceProvider()
      .GetRequiredService<IServiceScopeFactory>();

    _handler = new Handler(
      _marketRepository,
      _productRepository,
      _snapshotRepository,
      scopeFactory,
      Substitute.For<ILogger<Handler>>());

    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([]);
    _marketRepository
      .CheckUnitOfMeasureIsSupportedAsync(
        Arg.Any<string>(),
        Arg.Any<CancellationToken>())
      .Returns(true);
    _productRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .Returns([]);
    _productRepository
      .ResolveProductsAsync(
        Arg.Any<MarketImport>(),
        Arg.Any<DateTime>(),
        Arg.Any<CancellationToken>())
      .Returns(new ProductImportResult([], [], []));
  }

  [Fact(DisplayName = "Throws exact exception for an empty product import")]
  public async Task Handler_ThrowsEmptyMarketProducts_WhenImportIsEmpty() {
    async Task action() => await _handler.Handle(
      new Command([], new DateOnly(2026, 7, 28)),
      TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<EmptyMarketProductsException>(action);
    await _marketRepository.DidNotReceive().AddMarketsAsync(
      Arg.Any<IReadOnlyCollection<MarketName>>(),
      Arg.Any<CancellationToken>());
    await _productRepository.DidNotReceive().ResolveProductsAsync(
      Arg.Any<MarketImport>(),
      Arg.Any<DateTime>(),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Throws exact exception for an old registration date")]
  public async Task Handler_ThrowsInvalidRegisteredAt_WhenDateIsTooOld() {
    Command command = CreateCommand() with {
      RegisteredAt = new DateOnly(2022, 12, 31),
    };

    async Task action() => await _handler.Handle(
      command,
      TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<InvalidMarketProductsRegisteredAtException>(
      action);
  }

  [Fact(DisplayName = "Throws exact exception for a duplicate product identity")]
  public async Task Handler_ThrowsDuplicateMarketProduct_WhenIdentityRepeats() {
    Command command = CreateCommand();
    command = command with {
      Products = [command.Products.Single(), command.Products.Single()],
    };

    async Task action() => await _handler.Handle(
      command,
      TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<DuplicateMarketProductException>(action);
  }

  [Fact(DisplayName = "Throws exact exception for an unsupported unit")]
  public async Task Handler_ThrowsUnsupportedUnit_WhenUnitIsNotSupported() {
    _marketRepository
      .CheckUnitOfMeasureIsSupportedAsync(
        "l",
        TestContext.Current.CancellationToken)
      .Returns(false);

    async Task action() => await _handler.Handle(
      CreateCommand(),
      TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<UnsupportedUnitOfMeasureException>(action);
  }

  [Fact(DisplayName = "Preserves exact domain exception for invalid product name")]
  public async Task Handler_ThrowsInvalidProductName_WhenNameIsInvalid() {
    Command command = CreateCommand();
    command = command with {
      Products = [command.Products.Single() with { Name = null }],
    };

    async Task action() => await _handler.Handle(
      command,
      TestContext.Current.CancellationToken);

    await Assert.ThrowsAsync<InvalidProductNameException>(action);
  }

  [Fact(DisplayName = "Builds validated imports and appends observations separately")]
  public async Task Handler_ResolvesProducts_AndAppendsSnapshots() {
    Command command = CreateCommand();
    var observation = new PriceObservation(
      new ProductFormatId(7),
      new Domain.SharedKernel.Money(1.99m),
      new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc));
    _productRepository
      .ResolveProductsAsync(
        Arg.Any<MarketImport>(),
        Arg.Any<DateTime>(),
        Arg.Any<CancellationToken>())
      .Returns(new ProductImportResult(
        [new ProductId(3)],
        [new ProductFormatId(7)],
        [observation]));

    await _handler.Handle(command, TestContext.Current.CancellationToken);
    await _marketRepository.Received(1).AddMarketsAsync(
      Arg.Is<IReadOnlyCollection<MarketName>>(names =>
        names.Single().Value == "Market"),
      TestContext.Current.CancellationToken);
    await _productRepository.Received(1).AddBrandsAsync(
      Arg.Is<IReadOnlyCollection<BrandName>>(brands =>
        brands.Single().Value == "Brand"),
      TestContext.Current.CancellationToken);
    await _productRepository.Received(1).ResolveProductsAsync(
      Arg.Is<MarketImport>(market =>
        market.Name.Value == "Market" &&
        market.Products.Single().Name.Value == "Milk" &&
        market.Products.Single().Formats.Single().Quantity.Amount == 1m &&
        market.Products.Single().Formats.Single().Price.Amount == 1.99m),
      Arg.Is<DateTime>(value =>
        value == new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc)),
      TestContext.Current.CancellationToken);
    await _snapshotRepository.Received(1).AppendAsync(
      Arg.Is<IReadOnlyCollection<PriceObservation>>(values =>
        values.Single() == observation),
      TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Does not add existing market or brand")]
  public async Task Handler_DoesNotAddExistingMarketOrBrand() {
    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([
        new Market(new MarketId(1), new MarketName("MARKET")),
      ]);
    _productRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .Returns([new BrandName("BRAND")]);

    await _handler.Handle(
      CreateCommand(), TestContext.Current.CancellationToken);

    await _marketRepository.DidNotReceive().AddMarketsAsync(
      Arg.Any<IReadOnlyCollection<MarketName>>(),
      Arg.Any<CancellationToken>());
    await _productRepository.DidNotReceive().AddBrandsAsync(
      Arg.Any<IReadOnlyCollection<BrandName>>(),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Rolls back records created before cancellation")]
  public async Task Handler_RollsBack_WhenSnapshotAppendIsCancelled() {
    var result = new ProductImportResult(
      [new ProductId(3)],
      [new ProductFormatId(7)],
      [new PriceObservation(
        new ProductFormatId(7),
        new Domain.SharedKernel.Money(1.99m),
        new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc))]);
    _productRepository
      .ResolveProductsAsync(
        Arg.Any<MarketImport>(),
        Arg.Any<DateTime>(),
        Arg.Any<CancellationToken>())
      .Returns(result);
    _snapshotRepository
      .AppendAsync(
        Arg.Any<IReadOnlyCollection<PriceObservation>>(),
        Arg.Any<CancellationToken>())
      .ThrowsAsync<OperationCanceledException>();
    var rollbackSignal = new TaskCompletionSource(
      TaskCreationOptions.RunContinuationsAsynchronously);
    _marketRepository
      .DeleteMarketsAsync(
        Arg.Any<IReadOnlyCollection<MarketName>>(),
        Arg.Any<CancellationToken>())
      .Returns(_ => {
        rollbackSignal.SetResult();
        return Task.CompletedTask;
      });

    await Assert.ThrowsAsync<OperationCanceledException>(
      () => _handler.Handle(
        CreateCommand(), TestContext.Current.CancellationToken));
    await rollbackSignal.Task.WaitAsync(
      TimeSpan.FromSeconds(1),
      TestContext.Current.CancellationToken);

    await _snapshotRepository.Received(1).DeleteForMarketsAsync(
      Arg.Is<IReadOnlyCollection<MarketName>>(markets =>
        markets.Single().Value == "Market"),
      Arg.Any<DateTime>(),
      Arg.Any<CancellationToken>());
    await _productRepository.Received(1).DeleteProductsAsync(
      Arg.Is<IReadOnlyCollection<ProductId>>(ids =>
        ids.Single() == new ProductId(3)),
      Arg.Any<CancellationToken>());
    await _productRepository.Received(1).DeleteProductFormatsAsync(
      Arg.Is<IReadOnlyCollection<ProductFormatId>>(ids =>
        ids.Single() == new ProductFormatId(7)),
      Arg.Any<CancellationToken>());
    await _productRepository.Received(1).DeleteBrandsAsync(
      Arg.Any<IReadOnlyCollection<BrandName>>(),
      Arg.Any<CancellationToken>());
    await _marketRepository.Received(1).DeleteMarketsAsync(
      Arg.Any<IReadOnlyCollection<MarketName>>(),
      Arg.Any<CancellationToken>());
  }

  [Fact(DisplayName = "Checks equivalent unit codes only once")]
  public async Task Handler_ChecksUnitSupportOnce_IgnoringCase() {
    Command command = new([
      new CommandProduct(
        "Milk", 1.99m, 1, "l", "Market", "Brand", null),
      new CommandProduct(
        "Juice", 2.49m, 1, "L", "Market", "Brand", null),
    ], new DateOnly(2026, 7, 28));

    await _handler.Handle(command, TestContext.Current.CancellationToken);

    await _marketRepository.Received(1).CheckUnitOfMeasureIsSupportedAsync(
      Arg.Any<string>(), TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Rollback deletes only market and brand created by import")]
  public async Task Handler_RollbackPreservesExistingMarketAndBrand() {
    _marketRepository.GetMarketsAsync(Arg.Any<CancellationToken>())
      .Returns([new Market(new MarketId(1), new MarketName("Existing"))]);
    _productRepository.GetBrandsAsync(Arg.Any<CancellationToken>())
      .Returns([new BrandName("Existing brand")]);
    _snapshotRepository.AppendAsync(
      Arg.Any<IReadOnlyCollection<PriceObservation>>(),
      Arg.Any<CancellationToken>())
      .ThrowsAsync<OperationCanceledException>();
    var rollbackSignal = new TaskCompletionSource(
      TaskCreationOptions.RunContinuationsAsynchronously);
    _marketRepository.DeleteMarketsAsync(
      Arg.Any<IReadOnlyCollection<MarketName>>(),
      Arg.Any<CancellationToken>())
      .Returns(_ => {
        rollbackSignal.SetResult();
        return Task.CompletedTask;
      });
    Command command = new([
      new CommandProduct(
        "Milk", 1.99m, 1, "l", "Existing", "Existing brand", null),
      new CommandProduct(
        "Bread", 2.49m, 1, "kg", "New market", "New brand", null),
    ], new DateOnly(2026, 7, 28));

    await Assert.ThrowsAsync<OperationCanceledException>(() =>
      _handler.Handle(command, TestContext.Current.CancellationToken));
    await rollbackSignal.Task.WaitAsync(
      TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

    await _marketRepository.Received(1).DeleteMarketsAsync(
      Arg.Is<IReadOnlyCollection<MarketName>>(names =>
        names.Count == 1 && names.Single() == new MarketName("New market")),
      Arg.Any<CancellationToken>());
    await _productRepository.Received(1).DeleteBrandsAsync(
      Arg.Is<IReadOnlyCollection<BrandName>>(names =>
        names.Count == 1 && names.Single() == new BrandName("New brand")),
      Arg.Any<CancellationToken>());
  }

  private static Command CreateCommand() => new(
    [new CommandProduct(
      "Milk",
      1.99m,
      1,
      "l",
      "Market",
      "Brand",
      new Uri("https://example.com/milk.png"))],
    new DateOnly(2026, 7, 28));
}