namespace Metaspesa.Domain.Shopping;

public static class PricePolicy {
  public static bool IsValidPrice(float price) => price >= 0;
}