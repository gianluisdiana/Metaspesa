using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.Markets;

public record ProductFormat(Quantity Quantity, Price Price, Uri? ImageUrl);