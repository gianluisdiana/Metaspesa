namespace Metaspesa.Application.Abstractions.Markets;

public sealed record MarketSummary(Guid Id, string Name, Uri? LogoUrl);