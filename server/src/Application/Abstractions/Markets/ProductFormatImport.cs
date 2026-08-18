using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Application.Abstractions.Markets;

public sealed record ProductFormatImport(
  Quantity Quantity, Money Price, ImageUrl? ImageUrl);
