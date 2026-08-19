using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Application.Shopping;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using Metaspesa.GrpcApi.Protos.Shopping;
using Metaspesa.GrpcApi.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using ShoppingItem = Metaspesa.Domain.Shopping.ShoppingItem;
using ShoppingList = Metaspesa.Domain.Shopping.ShoppingList;

namespace Metaspesa.GrpcApi.UnitTests.Shopping;

public class ShoppingGrpcServiceTests {
  [Fact(DisplayName = "Maps summary read models to unchanged response")]
  public async Task GetShoppingListSummaries_MapsReadModels() {
    var fixture = new ServiceFixture();
    var ownerId = Guid.CreateVersion7();
    fixture.ShoppingRepository.GetByOwnerAsync(
      Arg.Any<UserId>(), Arg.Any<CancellationToken>())
      .Returns([
        PersistedList(ownerId, "Weekly"),
        PersistedList(ownerId, null),
      ]);

    ShoppingListSummariesResponse response = await fixture.Service
      .GetShoppingListSummaries(new Empty(), CreateServerCallContext(ownerId));

    Assert.Equal(2, response.ShoppingLists.Count);
    Assert.Equal("Weekly", response.ShoppingLists[0].Name);
    Assert.False(response.ShoppingLists[1].HasName);
  }

  [Fact(DisplayName = "Maps enriched detail read model to unchanged response")]
  public async Task GetShoppingList_MapsExistingResponseShape() {
    var fixture = new ServiceFixture();
    var ownerId = Guid.CreateVersion7();
    fixture.ShoppingRepository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(PersistedList(
        ownerId,
        "Weekly",
        new ShoppingItem(new ProductFormatId(7), new PositiveAmount(2), true)));
    fixture.ProductRepository.GetProductsAsync(
      Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
      .Returns(new Dictionary<int, MarketProduct> {
        [7] = ProductProjection(),
      });

    ShoppingListResponse response = await fixture.Service.GetShoppingList(
      new GetShoppingListRequest { ShoppingListName = "Weekly" },
      CreateServerCallContext(ownerId));

    Assert.Equal("Weekly", response.ShoppingList.Name);
    Metaspesa.GrpcApi.Protos.Shopping.ShoppingItem item =
      Assert.Single(response.ShoppingList.Items);
    Assert.Equal("Milk", item.Name);
    Assert.Equal("1 l", item.Quantity);
    Assert.Equal("1.25", item.Price);
    Assert.True(item.Checked);
  }

  [Fact(DisplayName = "Maps missing list name and JWT owner to temporary-list query")]
  public async Task GetShoppingList_UsesClaimAndNullName_WhenNameIsMissing() {
    var fixture = new ServiceFixture();
    var ownerId = Guid.CreateVersion7();
    fixture.ShoppingRepository.GetAsync(
      new UserId(ownerId), null, TestContext.Current.CancellationToken)
      .Returns(PersistedList(ownerId, null));

    ShoppingListResponse response = await fixture.Service.GetShoppingList(
      new GetShoppingListRequest(), CreateServerCallContext(ownerId));

    await fixture.ShoppingRepository.Received(1).GetAsync(
      new UserId(ownerId), null, TestContext.Current.CancellationToken);
    Assert.False(response.ShoppingList.HasName);
  }

  [Fact(DisplayName = "Creates list through concrete handler")]
  public async Task CreateShoppingList_UsesConcreteHandler() {
    var fixture = new ServiceFixture();
    var ownerId = Guid.CreateVersion7();

    CreateShoppingListResponse response = await fixture.Service.CreateShoppingList(
      new CreateShoppingListRequest { Name = " Weekly " },
      CreateServerCallContext(ownerId));

    fixture.ShoppingRepository.Received(1).Add(Arg.Is<ShoppingList>(list =>
      list.OwnerIds.Single() == new UserId(ownerId) &&
      list.Name == new ShoppingListName("Weekly")));
    Assert.Equal(" Weekly ", response.Name);
  }

  [Fact(DisplayName = "Creates temporary list when proto name is absent")]
  public async Task CreateShoppingList_MapsAbsentName_ToTemporaryList() {
    var fixture = new ServiceFixture();
    var ownerId = Guid.CreateVersion7();

    CreateShoppingListResponse response = await fixture.Service.CreateShoppingList(
      new CreateShoppingListRequest(), CreateServerCallContext(ownerId));

    fixture.ShoppingRepository.Received(1).Add(Arg.Is<ShoppingList>(list =>
      list.IsTemporary && list.OwnerIds.Single() == new UserId(ownerId)));
    Assert.False(response.HasName);
  }

  [Fact(DisplayName = "Adds items through concrete handler")]
  public async Task AddItemsToList_UsesConcreteHandler() {
    var fixture = new ServiceFixture();
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = PersistedList(ownerId, "Weekly");
    fixture.ShoppingRepository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(list);
    fixture.ProductRepository.GetProductsAsync(
      Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
      .Returns(new Dictionary<int, MarketProduct> { [7] = ProductProjection() });
    var request = new AddItemsToListRequest { ShoppingListName = "Weekly" };
    request.Items.Add(new AShoppingItem {
      ReferenceUid = 7,
      Amount = 3,
      IsChecked = true,
    });

    await fixture.Service.AddItemsToList(request, CreateServerCallContext(ownerId));

    ShoppingItem item = Assert.Single(list.Items);
    Assert.Equal(7, item.ProductFormatId.Value);
    Assert.Equal(3, item.Amount.Value);
    Assert.True(item.IsChecked);
  }

  [Fact(DisplayName = "Sanitizes list name before adding items")]
  public async Task AddItemsToList_SanitizesNonAsciiListName() {
    var fixture = new ServiceFixture();
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = PersistedList(ownerId, "Semanal");
    fixture.ShoppingRepository.GetAsync(
      new UserId(ownerId),
      new ShoppingListName("Semanal"),
      TestContext.Current.CancellationToken)
      .Returns(list);
    fixture.ProductRepository.GetProductsAsync(
      Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
      .Returns(new Dictionary<int, MarketProduct> { [7] = ProductProjection() });
    var request = new AddItemsToListRequest { ShoppingListName = "Semanal \u2713" };
    request.Items.Add(new AShoppingItem { ReferenceUid = 7, Amount = 1 });

    await fixture.Service.AddItemsToList(request, CreateServerCallContext(ownerId));

    await fixture.ShoppingRepository.Received(1).GetAsync(
      new UserId(ownerId),
      new ShoppingListName("Semanal"),
      TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Updates item through concrete handler")]
  public async Task UpdateItem_UsesConcreteHandler() {
    var fixture = new ServiceFixture();
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = PersistedList(
      ownerId,
      "Weekly",
      new ShoppingItem(new ProductFormatId(7), new PositiveAmount(1), false));
    fixture.ShoppingRepository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(list);

    await fixture.Service.UpdateItem(new UpdateItemRequest {
      ShoppingListName = "Weekly",
      ProductReferenceUid = 7,
      Amount = 4,
      IsChecked = true,
    }, CreateServerCallContext(ownerId));

    Assert.Equal(4, list.Items.Single().Amount.Value);
    Assert.True(list.Items.Single().IsChecked);
  }

  [Fact(DisplayName = "Preserves amount when proto amount is absent")]
  public async Task UpdateItem_MapsAbsentAmount_ToNoAmountChange() {
    var fixture = new ServiceFixture();
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = PersistedList(
      ownerId,
      "Weekly",
      new ShoppingItem(new ProductFormatId(7), new PositiveAmount(3), false));
    fixture.ShoppingRepository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(list);

    await fixture.Service.UpdateItem(new UpdateItemRequest {
      ShoppingListName = "Weekly",
      ProductReferenceUid = 7,
      IsChecked = true,
    }, CreateServerCallContext(ownerId));

    Assert.Equal(3, list.Items.Single().Amount.Value);
    Assert.True(list.Items.Single().IsChecked);
  }

  [Fact(DisplayName = "Renames list through concrete handler")]
  public async Task UpdateShoppingList_UsesConcreteHandler() {
    var fixture = new ServiceFixture();
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = PersistedList(ownerId, null);
    fixture.ShoppingRepository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(list);

    await fixture.Service.UpdateShoppingList(new UpdateShoppingListRequest {
      ListName = "Weekly",
    }, CreateServerCallContext(ownerId));

    Assert.Equal(new ShoppingListName("Weekly"), list.Name);
    Assert.False(list.IsTemporary);
  }

  [Fact(DisplayName = "Removes item through concrete handler")]
  public async Task RemoveItem_UsesConcreteHandler() {
    var fixture = new ServiceFixture();
    var ownerId = Guid.CreateVersion7();
    ShoppingList list = PersistedList(
      ownerId,
      "Weekly",
      new ShoppingItem(new ProductFormatId(7), new PositiveAmount(1), false));
    fixture.ShoppingRepository.GetAsync(
      Arg.Any<UserId>(), Arg.Any<ShoppingListName?>(), Arg.Any<CancellationToken>())
      .Returns(list);

    await fixture.Service.RemoveItem(new RemoveItemRequest {
      ShoppingListName = "Weekly",
      ProductReferenceUid = 7,
    }, CreateServerCallContext(ownerId));

    Assert.Empty(list.Items);
  }

  [Fact(DisplayName = "Keeps record workflow on transitional handler")]
  public async Task RecordShoppingList_UsesTransitionalHandler() {
    var fixture = new ServiceFixture();
    var ownerId = Guid.CreateVersion7();
    fixture.RecordHandler.Handle(
      Arg.Any<RecordShoppingList.Command>(), Arg.Any<CancellationToken>())
      .Returns(Result.Success());

    await fixture.Service.RecordShoppingList(
      new RecordShoppingListRequest { ShoppingListName = "Weekly" },
      CreateServerCallContext(ownerId));

    await fixture.RecordHandler.Received(1).Handle(
      Arg.Is<RecordShoppingList.Command>(command =>
        command.UserUid == ownerId && command.ShoppingListName == "Weekly"),
      TestContext.Current.CancellationToken);
  }

  [Fact(DisplayName = "Maps transitional record failure to RPC status")]
  public async Task RecordShoppingList_ThrowsRpcException_WhenHandlerFails() {
    var fixture = new ServiceFixture();
    fixture.RecordHandler.Handle(
      Arg.Any<RecordShoppingList.Command>(), Arg.Any<CancellationToken>())
      .Returns(new DomainError(
        "ShoppingList.MissingCheckedItems",
        "Shopping list must contain a checked item.",
        ErrorKind.Validation));

    RpcException exception = await Assert.ThrowsAsync<RpcException>(() =>
      fixture.Service.RecordShoppingList(
        new RecordShoppingListRequest { ShoppingListName = "Weekly" },
        CreateServerCallContext(Guid.CreateVersion7())));

    Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
  }

  private sealed class ServiceFixture {
    public IShoppingListRepository ShoppingRepository { get; } =
      Substitute.For<IShoppingListRepository>();
    public IProductRepository ProductRepository { get; } =
      Substitute.For<IProductRepository>();
    public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();
    public ICommandHandler<RecordShoppingList.Command> RecordHandler { get; } =
      Substitute.For<ICommandHandler<RecordShoppingList.Command>>();
    public ShoppingGrpcService Service { get; }

    public ServiceFixture() {
      Service = new ShoppingGrpcService(
        new GetShoppingListSummaries.Handler(ShoppingRepository),
        new GetShoppingList.Handler(ShoppingRepository, ProductRepository),
        RecordHandler,
        new CreateShoppingList.Handler(ShoppingRepository, UnitOfWork),
        new AddItemsToList.Handler(
          ShoppingRepository, ProductRepository, UnitOfWork),
        new UpdateItem.Handler(ShoppingRepository, UnitOfWork),
        new RemoveItem.Handler(ShoppingRepository, UnitOfWork),
        new UpdateShoppingList.Handler(ShoppingRepository, UnitOfWork));
    }
  }

  private static ShoppingList PersistedList(
    Guid ownerId, string? name, params ShoppingItem[] items
  ) => ShoppingList.Rehydrate(
    new ShoppingListId(1),
    [new UserId(ownerId)],
    name is null ? null : new ShoppingListName(name),
    null,
    items);

  private static MarketProduct ProductProjection() => new(
    "Milk",
    "Brand",
    [new MarketProductFormat(
      new Quantity(1, new UnitOfMeasure("l")),
      new Money(1.25m),
      new Uri("https://example.test/milk"))]);

  private static ServerCallContext CreateServerCallContext(Guid userId) {
    ServerCallContext context = TestServerCallContext.Create(
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
    var httpContext = new DefaultHttpContext {
      User = new ClaimsPrincipal(new ClaimsIdentity([
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
      ])),
    };
    context.UserState["__HttpContext"] = httpContext;
    return context;
  }
}