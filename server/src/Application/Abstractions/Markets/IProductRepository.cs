using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Domain.Markets;

namespace Metaspesa.Application.Abstractions.Markets;

public interface IProductRepository {
  Task<Product?> GetByIdAsync(
    ProductId productId, CancellationToken cancellationToken);
  Task<PagedResult<MarketCatalog>> GetProductsAsync(
    GetMarketProductsFilter filter, CancellationToken cancellationToken);
  Task<IReadOnlyDictionary<int, MarketProduct>> GetProductsAsync(
    IReadOnlyCollection<int> productFormatIds, CancellationToken cancellationToken);
  Task<IReadOnlyCollection<BrandName>> GetBrandsAsync(
    CancellationToken cancellationToken);
  Task AddBrandsAsync(
    IReadOnlyCollection<BrandName> brands, CancellationToken cancellationToken);
  Task<ProductImportResult> ResolveProductsAsync(
    MarketImport market, DateTime observedAt, CancellationToken cancellationToken);
  Task DeleteBrandsAsync(
    IReadOnlyCollection<BrandName> brandNames, CancellationToken cancellationToken);
  Task DeleteProductsAsync(
    IReadOnlyCollection<ProductId> productIds, CancellationToken cancellationToken);
  Task DeleteProductFormatsAsync(
    IReadOnlyCollection<ProductFormatId> productFormatIds,
    CancellationToken cancellationToken);
}