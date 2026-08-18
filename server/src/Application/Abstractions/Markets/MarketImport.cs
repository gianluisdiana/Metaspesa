using Metaspesa.Domain.Markets;

namespace Metaspesa.Application.Abstractions.Markets;

public sealed record MarketImport(
  MarketName Name, IReadOnlyCollection<ProductImport> Products);
