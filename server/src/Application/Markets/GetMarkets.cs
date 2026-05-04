using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Domain.Markets;

namespace Metaspesa.Application.Markets;

public static class GetMarkets {
  public record Query() : IQuery<IReadOnlyCollection<MarketSummary>>;

  internal class Handler(IMarketRepository marketRepository)
    : IQueryHandler<Query, IReadOnlyCollection<MarketSummary>> {
    public async Task<Result<IReadOnlyCollection<MarketSummary>>> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      return await marketRepository.GetMarketSummariesAsync(cancellationToken);
    }
  }
}
