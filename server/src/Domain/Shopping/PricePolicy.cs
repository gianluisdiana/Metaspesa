namespace Metaspesa.Domain.Shopping;

public static class PricePolicy {
  public static bool IsValidPrice(decimal price) => price >= 0;
}