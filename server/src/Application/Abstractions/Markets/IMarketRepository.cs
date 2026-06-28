using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Domain.Markets;

namespace Metaspesa.Application.Abstractions.Markets;

public interface IMarketRepository {
  Task<List<MarketSummary>> GetMarketSummariesAsync(CancellationToken cancellationToken);
  Task<PagedResult<Market>> GetProductsAsync(
    GetMarketProductsFilter filter, CancellationToken cancellationToken);
  Task<IReadOnlyDictionary<int, MarketProduct>> GetProductsAsync(
    IReadOnlyCollection<int> referencesId, CancellationToken cancellationToken);
  Task<List<Market>> GetMarketsAsync(CancellationToken cancellationToken);
  Task AddMarketsAsync(
    IReadOnlyCollection<Market> markets, CancellationToken cancellationToken);
  Task<List<ProductBrand>> GetBrandsAsync(CancellationToken cancellationToken);
  Task AddBrandsAsync(
    IReadOnlyCollection<ProductBrand> brands, CancellationToken cancellationToken);
  Task<IReadOnlyCollection<int>> AddMarketProductsAsync(
    Market market, DateOnly registeredAt, CancellationToken cancellationToken);
  Task DeleteMarketsAsync(
    IReadOnlyCollection<string> marketNames, CancellationToken cancellationToken);
  Task DeleteBrandsAsync(
    IReadOnlyCollection<string> brandNames, CancellationToken cancellationToken);
  Task DeleteProductsAsync(
    IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);
  Task DeletePriceSnapshotsForMarketsAsync(
    IReadOnlyCollection<string> marketNames,
    DateOnly registeredAt,
    CancellationToken cancellationToken);
  Task<bool> CheckUnitOfMeasureIsSupportedAsync(
    string unitOfMeasure, CancellationToken cancellationToken);
}