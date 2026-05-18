using System.Globalization;

namespace Metaspesa.GrpcApi.Extensions;

internal static class GrpcPriceConverter {
  public static decimal ToDecimal(string price) =>
    decimal.Parse(price, NumberStyles.Number, CultureInfo.InvariantCulture);

  public static string ToProto(decimal price) =>
    price.ToString(CultureInfo.InvariantCulture);
}