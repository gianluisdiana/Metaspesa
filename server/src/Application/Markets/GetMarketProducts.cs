using FluentValidation;
using FluentValidation.Results;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Markets;
using Metaspesa.Application.Extensions;
using Metaspesa.Domain.Markets;

namespace Metaspesa.Application.Markets;

public static class GetMarketProducts {
  public record Query(GetMarketProductsFilter Filter) : IQuery<PagedResult<Market>>;

  internal class Handler(
    IValidator<Query> validator,
    IMarketRepository marketRepository
  ) : IQueryHandler<Query, PagedResult<Market>> {
    public async Task<Result<PagedResult<Market>>> Handle(
      Query query, CancellationToken cancellationToken = default
    ) {
      ValidationResult validationResult = await validator.ValidateAsync(
        query, cancellationToken);
      if (!validationResult.IsValid) {
        return validationResult.ToDomainErrors();
      }

      if (query.Filter.Pagination is null) {
        query = query with { Filter = query.Filter with { Pagination = Pagination.Infinite } };
      }

      return await marketRepository.GetProductsAsync(query.Filter, cancellationToken);
    }
  }

  internal class Validator : AbstractValidator<Query> {
    public Validator() {
      When(q => q.Filter.Pagination is not null && !q.Filter.Pagination.IsInfinite, () => {
        RuleFor(q => q.Filter.Pagination!.Index)
          .GreaterThan(0)
          .WithMessage("Page index must be greater than 0.")
          .WithErrorCode("Market.Pagination.Index.Invalid");

        RuleFor(q => q.Filter.Pagination!.Size)
          .GreaterThan(0)
          .WithMessage("Page size must be greater than 0.")
          .WithErrorCode("Market.Pagination.Size.Invalid");
      });
    }
  }
}
