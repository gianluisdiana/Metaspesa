using Metaspesa.Domain.Markets;

namespace Metaspesa.Application.Abstractions.Markets;

public sealed record ProductImport(
  ProductName Name,
  BrandName Brand,
  IReadOnlyCollection<ProductFormatImport> Formats
);