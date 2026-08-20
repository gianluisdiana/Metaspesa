using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Domain.Markets;

public sealed class ProductFormat(
  ProductFormatId id, Quantity quantity, ImageUrl? imageUrl
) {
  public ProductFormatId Id { get; } = id;
  public Quantity Quantity { get; private set; } = quantity;
  public ImageUrl? ImageUrl { get; private set; } = imageUrl;

  internal void Update(Quantity quantity, ImageUrl? imageUrl) {
    Quantity = quantity;
    ImageUrl = imageUrl;
  }
}