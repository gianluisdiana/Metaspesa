namespace Metaspesa.Application.Abstractions.Shopping;

public interface IProductRepository {
  Task<bool> CheckProductExistsAsync(
    long referenceUid, CancellationToken cancellationToken);
}