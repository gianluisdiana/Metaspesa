using System.Globalization;
using System.Text;

namespace Metaspesa.GrpcApi.Extensions;

internal static class GrpcTextSanitizer {
  public static string SanitizeAscii(string value) {
    if (value.All(IsAllowedAscii)) {
      return value;
    }

    string normalized = value.Normalize(NormalizationForm.FormD);
    var builder = new StringBuilder(normalized.Length);

    foreach (char character in normalized) {
      UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
      if (category is UnicodeCategory.NonSpacingMark
        or UnicodeCategory.SpacingCombiningMark
        or UnicodeCategory.EnclosingMark) {
        continue;
      }

      if (IsAllowedAscii(character)) {
        builder.Append(character);
      }
    }

    return builder.ToString();
  }

  private static bool IsAllowedAscii(char character) =>
    character is >= ' ' and <= '~';
}
