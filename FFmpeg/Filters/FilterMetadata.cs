using System.Globalization;

namespace FFmpeg.Filters;

/// <summary>
/// Parses the numbers filters write into frame metadata and log files.
/// </summary>
internal static class FilterMetadata
{
    /// <summary>
    /// Parses a number printed by C's <c>printf("%f")</c>, which writes <c>inf</c>, <c>-inf</c> and <c>nan</c>
    /// for the special values (e.g. the PSNR of identical frames). <see cref="double.Parse(string)"/> does not
    /// accept those.
    /// </summary>
    public static bool TryParse(string? text, out double value)
    {
        value = double.NaN;
        if (string.IsNullOrWhiteSpace(text))
            return false;
        string s = text!.Trim();
        switch (s.ToLowerInvariant())
        {
            case "inf":
            case "+inf":
            case "infinity":
                value = double.PositiveInfinity;
                return true;
            case "-inf":
            case "-infinity":
                value = double.NegativeInfinity;
                return true;
            case "nan":
            case "-nan":
                return true;
        }
        return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
