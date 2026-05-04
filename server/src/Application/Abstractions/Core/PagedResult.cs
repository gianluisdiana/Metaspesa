namespace Metaspesa.Application.Abstractions.Core;

public record PagedResult<T>(IReadOnlyCollection<T> Values, int TotalCount);
