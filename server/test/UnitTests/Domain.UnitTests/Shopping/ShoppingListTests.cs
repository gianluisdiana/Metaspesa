using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Identity.Errors;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Domain.UnitTests.Shopping;

public class ShoppingListTests {
  private static readonly UserId OwnerId = new(Guid.Parse("11111111-1111-1111-1111-111111111111"));

  [Fact(DisplayName = "Creates list with version 7 id")]
  public void Create_CreatesList_WithV7Id() {
    var list = ShoppingList.Create(OwnerId.Value, "Weekly");

    Assert.Equal(7, list.Id.Value.Version);
  }

  [Fact(DisplayName = "Creates list with given name")]
  public void Create_CreatesList_WithGivenName() {
    var list = ShoppingList.Create(OwnerId.Value, "Weekly");

    Assert.Equal(new ShoppingListName("Weekly"), list.Name);
  }

  [Fact(DisplayName = "Creates list with single owner")]
  public void Create_CreatesList_WithSingleOwner() {
    var list = ShoppingList.Create(OwnerId.Value, "Weekly");

    Assert.Equal(OwnerId, Assert.Single(list.OwnerIds));
  }

  [Fact(DisplayName = "Creates list without items")]
  public void Create_CreatesList_WithoutItems() {
    var list = ShoppingList.Create(OwnerId.Value, "Weekly");

    Assert.Empty(list.Items);
  }

  [Fact(DisplayName = "Creates list without deleted timestamp")]
  public void Create_CreatesList_WithoutDeletedTimestamp() {
    var list = ShoppingList.Create(OwnerId.Value, "Weekly");

    Assert.Null(list.DeletedAt);
  }

  [Fact(DisplayName = "Rejects invalid primitive owner ID")]
  public void Create_ThrowsExactException_WhenPrimitiveOwnerIdIsInvalid() {
    Assert.Throws<InvalidUserIdException>(() =>
      ShoppingList.Create(Guid.Empty, "Weekly"));
  }

  [Fact(DisplayName = "Rehydrates persisted list state")]
  public void Rehydrate_ExposesPersistedState() {
    var deletedAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
    var list = ShoppingList.Rehydrate(
      new ShoppingListId(Guid.Parse("00000000-0000-7000-8000-000000000004")),
      [OwnerId],
      new ShoppingListName("Weekly"),
      deletedAt,
      [new ShoppingItem(new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000008")), new PositiveAmount(2), true)]);

    Assert.Equal(new ShoppingListId(Guid.Parse("00000000-0000-7000-8000-000000000004")), list.Id);
    Assert.Equal(new ShoppingListName("Weekly"), list.Name);
    Assert.False(list.IsTemporary);
    Assert.Equal(deletedAt, list.DeletedAt);
    Assert.Single(list.Items);
  }

  [Fact(DisplayName = "Rejects missing owner")]
  public void Rehydrate_ThrowsExactException_WhenOwnerIsMissing() {
    Assert.Throws<MissingShoppingListOwnerException>(() => ShoppingList.Rehydrate(
      new ShoppingListId(Guid.Parse("00000000-0000-7000-8000-000000000001")), [], null, null, []));
  }

  [Fact(DisplayName = "Rejects duplicate owners")]
  public void Rehydrate_ThrowsExactException_WhenOwnerIsDuplicated() {
    Assert.Throws<DuplicateShoppingListOwnerException>(() => ShoppingList.Rehydrate(
      new ShoppingListId(Guid.Parse("00000000-0000-7000-8000-000000000001")), [OwnerId, OwnerId], null, null, []));
  }

  [Fact(DisplayName = "Rejects duplicate persisted items")]
  public void Rehydrate_ThrowsExactException_WhenItemIsDuplicated() {
    var duplicateId = new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002"));

    Assert.Throws<DuplicateShoppingItemException>(() => ShoppingList.Rehydrate(
      new ShoppingListId(Guid.Parse("00000000-0000-7000-8000-000000000001")),
      [OwnerId],
      null,
      null,
      [
        new ShoppingItem(duplicateId, new PositiveAmount(1), false),
        new ShoppingItem(duplicateId, new PositiveAmount(2), true),
      ]));
  }

  [Fact(DisplayName = "Allows active replacement for deleted item history")]
  public void AddItems_AddsActiveReplacement_WhenPreviousItemIsDeleted() {
    var formatId = Guid.Parse("00000000-0000-7000-8000-000000000002");
    var deletedItem = ShoppingItem.Rehydrate(
      Guid.Parse("00000000-0000-7000-8000-000000000003"),
      formatId,
      1,
      false,
      new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
    var list = ShoppingList.Rehydrate(
      new ShoppingListId(
        Guid.Parse("00000000-0000-7000-8000-000000000001")),
      [OwnerId],
      new ShoppingListName("Weekly"),
      null,
      [deletedItem]);

    list.AddItems([new AddItemsParams(formatId, 2, true)]);

    Assert.Single(list.Items, item => item.DeletedAt is not null);
    Assert.Single(list.Items, item => item.DeletedAt is null);
  }

  [Fact(DisplayName = "Renames temporary list")]
  public void Rename_SetsNameAndClearsTemporaryState() {
    var list = ShoppingList.Create(OwnerId, null);

    list.Rename(new ShoppingListName("Weekly"));

    Assert.Equal(new ShoppingListName("Weekly"), list.Name);
    Assert.False(list.IsTemporary);
  }

  [Fact(DisplayName = "Adds item")]
  public void AddItem_AddsTypedItem() {
    var list = ShoppingList.Create(OwnerId, null);

    list.AddItem(new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002")), new PositiveAmount(3), true);

    ShoppingItem item = Assert.Single(list.Items);
    Assert.Equal(new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002")), item.ProductFormatId);
    Assert.Equal(3, item.Amount.Value);
    Assert.True(item.IsChecked);
  }

  [Fact(DisplayName = "Adds primitive item batch")]
  public void AddItems_AddsPrimitiveItems() {
    var list = ShoppingList.Create(OwnerId.Value, "Test");
    var firstFormatId = Guid.Parse("00000000-0000-7000-8000-000000000002");
    var secondFormatId = Guid.Parse("00000000-0000-7000-8000-000000000003");

    list.AddItems([
      new AddItemsParams(firstFormatId, 2, true),
      new AddItemsParams(secondFormatId, 3, false),
    ]);

    var actual = list.Items.Select(item => new {
      ProductFormatId = item.ProductFormatId.Value,
      Amount = item.Amount.Value,
      item.IsChecked,
    });
    var expected = new[] {
      new { ProductFormatId = firstFormatId, Amount = 2, IsChecked = true },
      new { ProductFormatId = secondFormatId, Amount = 3, IsChecked = false },
    };
    Assert.Equal(expected, actual);
  }

  [Fact(DisplayName = "Rejects empty primitive item batch")]
  public void AddItems_ThrowsExactException_WhenPrimitiveBatchIsEmpty() {
    var list = ShoppingList.Create(OwnerId.Value, "Test");

    void action() => list.AddItems(Array.Empty<AddItemsParams>());

    Assert.Throws<EmptyShoppingItemsException>(action);
  }

  [Fact(DisplayName = "Rejects duplicate primitive batch atomically")]
  public void AddItems_ThrowsWhenPrimitiveBatchHasDuplicate() {
    var list = ShoppingList.Create(OwnerId, null);
    var duplicateId = Guid.Parse("00000000-0000-7000-8000-000000000002");
    IEnumerable<AddItemsParams> items = [
      new AddItemsParams(duplicateId, 1, false),
      new AddItemsParams(duplicateId, 2, true),
    ];

    void action() => list.AddItems(items);

    Assert.Throws<DuplicateShoppingItemException>(action);
  }

  [Fact(DisplayName = "Rejects existing primitive item")]
  public void AddItems_ThrowsWhenPrimitiveItemExists() {
    var list = ShoppingList.Create(OwnerId, null);
    var existingId = Guid.Parse("00000000-0000-7000-8000-000000000002");
    list.AddItems([new AddItemsParams(existingId, 1, false)]);

    void action() => list.AddItems([new AddItemsParams(existingId, 2, true)]);

    Assert.Throws<DuplicateShoppingItemException>(action);
  }

  [Fact(DisplayName = "Rejects invalid primitive item batch atomically")]
  public void AddItems_ThrowsWhenPrimitiveItemIsInvalid() {
    var list = ShoppingList.Create(OwnerId, null);

    void action() => list.AddItems([new AddItemsParams(Guid.Empty, 1, false)]);

    Assert.Throws<InvalidProductFormatIdException>(action);
  }

  [Fact(DisplayName = "Updates amount and checked state atomically")]
  public void UpdateItem_UpdatesProvidedFields() {
    var list = ShoppingList.Create(OwnerId, null);
    var formatId = new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002"));
    list.AddItem(formatId, new PositiveAmount(1), false);

    list.UpdateItem(formatId, new PositiveAmount(4), true);

    ShoppingItem item = Assert.Single(list.Items);
    Assert.Equal(4, item.Amount.Value);
    Assert.True(item.IsChecked);
  }

  [Fact(DisplayName = "Rejects update without fields")]
  public void UpdateItem_ThrowsExactException_WhenNoFieldsProvided() {
    var list = ShoppingList.Create(OwnerId, null);
    var formatId = new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002"));
    list.AddItem(formatId, new PositiveAmount(1), false);

    Assert.Throws<EmptyShoppingItemUpdateException>(() =>
      list.UpdateItem(formatId, null, null));
    Assert.Equal(1, list.Items.Single().Amount.Value);
  }

  [Fact(DisplayName = "Rejects update for missing item")]
  public void UpdateItem_ThrowsExactException_WhenItemIsMissing() {
    var list = ShoppingList.Create(OwnerId, null);

    Assert.Throws<ShoppingItemNotFoundException>(() =>
      list.UpdateItem(new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002")), new PositiveAmount(1), null));
  }

  [Fact(DisplayName = "Preserves amount when updating checked state")]
  public void UpdateItem_PreservesAmount_WhenAmountIsMissing() {
    var list = ShoppingList.Create(OwnerId, null);
    var formatId = new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002"));
    list.AddItem(formatId, new PositiveAmount(3), false);

    list.UpdateItem(formatId, null, true);

    Assert.Equal(3, list.Items.Single().Amount.Value);
  }

  [Fact(DisplayName = "Preserves checked state when updating amount")]
  public void UpdateItem_PreservesCheckedState_WhenCheckedStateIsMissing() {
    var list = ShoppingList.Create(OwnerId, null);
    var formatId = new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002"));
    list.AddItem(formatId, new PositiveAmount(1), true);

    list.UpdateItem(formatId, new PositiveAmount(3), null);

    Assert.True(list.Items.Single().IsChecked);
  }

  [Fact(DisplayName = "Removes existing item")]
  public void RemoveItem_RemovesItem() {
    var list = ShoppingList.Create(OwnerId, null);
    var formatId = new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002"));
    list.AddItem(formatId, new PositiveAmount(1), false);

    list.RemoveItem(formatId);

    Assert.Empty(list.Items);
  }

  [Fact(DisplayName = "Rejects removal of missing item")]
  public void RemoveItem_ThrowsExactException_WhenItemIsMissing() {
    var list = ShoppingList.Create(OwnerId, null);

    Assert.Throws<ShoppingItemNotFoundException>(() =>
      list.RemoveItem(new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002"))));
  }

  [Fact(DisplayName = "Returns checked items without mutation")]
  public void CheckedItems_ReturnsOnlyCheckedItems() {
    var list = ShoppingList.Create(OwnerId, null);
    list.AddItem(new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002")), new PositiveAmount(1), true);
    list.AddItem(new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000003")), new PositiveAmount(1), false);

    IReadOnlyCollection<ShoppingItem> checkedItems = list.CheckedItems();

    Assert.Single(checkedItems);
    Assert.Equal(2, list.Items.Count);
  }

  [Fact(DisplayName = "Resets checked items without removing them")]
  public void ResetCheckedItems_UnchecksAllItems() {
    var list = ShoppingList.Create(OwnerId, null);
    list.AddItem(new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002")), new PositiveAmount(1), true);

    list.ResetCheckedItems();

    Assert.Single(list.Items);
    Assert.False(list.Items.Single().IsChecked);
  }
}