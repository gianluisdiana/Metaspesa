using Metaspesa.Domain.Markets.Errors;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Domain.Markets;

public sealed class Product {
  private readonly List<ProductFormat> _formats;

  public ProductId Id { get; }
  public ProductName Name { get; }
  public BrandName Brand { get; }
  public MarketId MarketId { get; }
  public IReadOnlyCollection<ProductFormat> Formats => _formats.AsReadOnly();

  public Product(
    ProductId id,
    ProductName name,
    BrandName brand,
    MarketId marketId,
    IEnumerable<ProductFormat>? formats = null
  ) {
    Id = id;
    Name = name;
    Brand = brand;
    MarketId = marketId;
    _formats = formats?.ToList() ?? [];

    ProductFormatId? duplicateId = _formats
      .GroupBy(format => format.Id)
      .Where(group => group.Count() > 1)
      .Select(group => (ProductFormatId?)group.Key)
      .FirstOrDefault();

    if (duplicateId is not null) {
      throw new DuplicateProductFormatException(duplicateId.Value);
    }
  }

  public void AddFormat(
    ProductFormatId id, Quantity quantity, ImageUrl? imageUrl
  ) {
    if (_formats.Any(format => format.Id == id)) {
      throw new DuplicateProductFormatException(id);
    }

    _formats.Add(new ProductFormat(id, quantity, imageUrl));
  }

  public void UpdateFormat(
    ProductFormatId id, Quantity quantity, ImageUrl? imageUrl
  ) {
    ProductFormat format = _formats.FirstOrDefault(format => format.Id == id) ??
      throw new ProductFormatNotFoundException(id);

    format.Update(quantity, imageUrl);
  }
}