using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Application.Abstractions.Markets;

public sealed record MarketProductFormat(
  Quantity Quantity, Money Price, Uri? ImageUrl);
