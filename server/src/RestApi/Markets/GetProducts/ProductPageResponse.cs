namespace Metaspesa.RestApi.Markets.GetProducts;

/// <summary>One page of matching products.</summary>
/// <param name="Items">Products in this page; empty when none match.</param>
/// <param name="Page">One-based page number requested.</param>
/// <param name="PageSize">Maximum number of products requested for this page.</param>
/// <param name="TotalItems">Number of matching products across all pages.</param>
/// <param name="TotalPages">Number of pages at this page size; zero when none match.</param>
internal sealed record ProductPageResponse(
  IReadOnlyCollection<ProductResponse> Items, int Page, int PageSize,
  int TotalItems, int TotalPages);