namespace Metaspesa.Domain.SharedKernel;

public static class Uid {
  public static Guid Create() => Guid.CreateVersion7();
}