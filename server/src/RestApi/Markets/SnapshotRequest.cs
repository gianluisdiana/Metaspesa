namespace Metaspesa.RestApi.Markets;

internal sealed record SnapshotRequest(IReadOnlyList<SnapshotItem?>? Items);

internal sealed record SnapshotItem(
  string? Name,
  decimal Price,
  float Quantity,
  string? UnitOfMeasure,
  string? BrandName,
  string? ImageUrl);