using Metaspesa.Domain.Markets;

namespace Metaspesa.Application.Abstractions.Markets;

public sealed record ProductImportResult {
  public IReadOnlyCollection<ProductId> AddedProductIds { get; }
  public IReadOnlyCollection<ProductFormatId> AddedProductFormatIds { get; }
  public IReadOnlyCollection<PriceObservation> PriceObservations { get; }

  public ProductImportResult(
    IReadOnlyCollection<ProductId> addedProductIds,
    IReadOnlyCollection<PriceObservation> priceObservations
  ) : this(addedProductIds, [], priceObservations) { }

  public ProductImportResult(
    IReadOnlyCollection<ProductId> addedProductIds,
    IReadOnlyCollection<ProductFormatId> addedProductFormatIds,
    IReadOnlyCollection<PriceObservation> priceObservations
  ) {
    AddedProductIds = addedProductIds;
    AddedProductFormatIds = addedProductFormatIds;
    PriceObservations = priceObservations;
  }
}