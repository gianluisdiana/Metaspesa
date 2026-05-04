using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.Markets;

public record ProductFormat(string Quantity, Price Price, Uri? ImageUrl);