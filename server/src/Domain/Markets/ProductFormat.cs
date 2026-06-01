using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.Markets;

public record ProductFormat(AQuantity Quantity, Price Price, Uri? ImageUrl);