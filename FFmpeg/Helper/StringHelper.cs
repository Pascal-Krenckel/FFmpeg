using System.Text;

namespace FFmpeg.Helper;

internal static unsafe class StringHelper
{

    public static int StrLen(byte* p)
    {
        int i = 0;
        while (*p++ != 0)
            i++;
        return i;
    }

    extension(string)
    {
        public static string Join<T>(char separator, IReadOnlyList<T> span, Func<T, string> toString)
        {
            if (span.Count == 0)
                return string.Empty;
            if (span.Count == 1)
                return toString(span[0]);
            StringBuilder sb = new();
            _ = sb.Append(toString(span[0]));
            for (int i = 1; i < span.Count; i++)
                _ = sb.Append(separator).Append(toString(span[i]));
            return sb.ToString();
        }

        public static string Join<T>(string separator, IReadOnlyList<T> span, Func<T, string> toString)
        {
            if (span.Count == 0)
                return string.Empty;
            if (span.Count == 1)
                return toString(span[0]);
            StringBuilder sb = new();
            _ = sb.Append(toString(span[0]));
            for (int i = 1; i < span.Count; i++)
                _ = sb.Append(separator).Append(toString(span[i]));
            return sb.ToString();
        }

    }

}
