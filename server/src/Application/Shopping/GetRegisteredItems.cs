using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Shopping;

public static class GetRegisteredItems {
  public record Query(Guid UserUid) : IQuery<IReadOnlyCollection<RegisteredItem>>;

  internal class Handler(
    IProductRepository productRepository
  ) : IQueryHandler<Query, IReadOnlyCollection<RegisteredItem>> {
    public async Task<Result<IReadOnlyCollection<RegisteredItem>>> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      return await productRepository.GetRegisteredItemsAsync(
        query.UserUid, cancellationToken);
    }
  }
}