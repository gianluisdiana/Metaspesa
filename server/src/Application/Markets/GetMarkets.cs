using Metaspesa.Application.Abstractions.Markets;

namespace Metaspesa.Application.Markets;

public static class GetMarkets {
  public record Query();

  public class Handler(IMarketRepository marketRepository) {
    public async Task<IReadOnlyCollection<MarketSummary>> Handle(
      Query query, CancellationToken cancellationToken = default
    ) => await marketRepository.GetMarketSummariesAsync(cancellationToken);
  }
}