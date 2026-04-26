using Metaspesa.Domain.Markets;

namespace Metaspesa.Application.Abstractions.Markets;

public interface IMarketRepository {
  Task<List<Market>> GetMarketsAsync(CancellationToken cancellationToken);
  Task AddMarketsAsync(IReadOnlyCollection<Market> markets, CancellationToken cancellationToken);
  Task<List<ProductBrand>> GetBrandsAsync(CancellationToken cancellationToken);
  Task AddBrandsAsync(IReadOnlyCollection<ProductBrand> brands, CancellationToken cancellationToken);
  Task AddMarketProductsAsync(Market market, DateOnly registeredAt, CancellationToken cancellationToken);
}
