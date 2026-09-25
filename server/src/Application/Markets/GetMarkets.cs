using Metaspesa.Application.Abstractions.Markets;

namespace Metaspesa.Application.Markets;

public static class GetMarkets {
  public class Handler(IMarketRepository marketRepository) {
    public async Task<IReadOnlyCollection<MarketSummary>> Handle(
      CancellationToken cancellationToken = default
    ) => await marketRepository.GetMarketSummariesAsync(cancellationToken);
  }
}