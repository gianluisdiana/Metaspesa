using System.Globalization;
using System.Text;

namespace Metaspesa.Infrastructure;

public static class TextSanitizer {
  public static string Sanitize(string value) {
    ArgumentNullException.ThrowIfNull(value);

    string normalized = value.Normalize(NormalizationForm.FormC);
    var builder = new StringBuilder(normalized.Length);

    foreach (Rune rune in normalized.EnumerateRunes().Where(IsAllowed)) {
      builder.Append(rune);
    }

    return builder.ToString();
  }

  private static bool IsAllowed(Rune rune) {
    if (rune.IsAscii) {
      return rune.Value is >= ' ' and <= '~';
    }

    UnicodeCategory category = Rune.GetUnicodeCategory(rune);
    return category is UnicodeCategory.UppercaseLetter
      or UnicodeCategory.LowercaseLetter
      or UnicodeCategory.TitlecaseLetter
      or UnicodeCategory.ModifierLetter
      or UnicodeCategory.OtherLetter
      or UnicodeCategory.NonSpacingMark
      or UnicodeCategory.SpacingCombiningMark
      or UnicodeCategory.EnclosingMark
      or UnicodeCategory.DecimalDigitNumber
      or UnicodeCategory.LetterNumber
      or UnicodeCategory.OtherNumber
      or UnicodeCategory.SpaceSeparator
      or UnicodeCategory.ConnectorPunctuation
      or UnicodeCategory.DashPunctuation
      or UnicodeCategory.OpenPunctuation
      or UnicodeCategory.ClosePunctuation
      or UnicodeCategory.InitialQuotePunctuation
      or UnicodeCategory.FinalQuotePunctuation
      or UnicodeCategory.OtherPunctuation;
  }
}