using Metaspesa.Domain.Purchasing.Errors;

namespace Metaspesa.Domain.UnitTests.Purchasing;

public class PurchaseDomainExceptionTests {
  [Theory(DisplayName = "Purchasing exceptions share domain base")]
  [ClassData<PurchasingExceptions>]
  public void Exception_InheritsSharedDomainException(Exception exception) {
    Assert.IsType<PurchaseDomainException>(exception, exactMatch: false);
  }

  private sealed class PurchasingExceptions() : TheoryData<Exception>(
    (Exception)new DuplicatePurchaseItemException(),
    (Exception)new EmptyPurchaseItemsException(),
    (Exception)new InvalidPurchasedAtException(),
    (Exception)new InvalidPurchaseIdException(),
    (Exception)new PurchasePriceSnapshotNotFoundException()
  );
}
