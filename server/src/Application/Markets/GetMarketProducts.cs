using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;

namespace Metaspesa.Application.Markets;

public static class GetMarketProducts {
  public record Query(GetMarketProductsFilter Filter);

  public class Handler(IProductRepository productRepository) {
    public async Task<PagedResult<MarketCatalog>> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      ArgumentNullException.ThrowIfNull(query);

      if (query.Filter.Pagination is null) {
        query = query with { Filter = query.Filter with { Pagination = Pagination.Infinite } };
      }

      return await productRepository.GetProductsAsync(query.Filter, cancellationToken);
    }
  }
}