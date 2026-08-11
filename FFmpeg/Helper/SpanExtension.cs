using System.Text;

namespace FFmpeg.Helper;

internal static class SpanExtension
{
    public static bool Contains<T>(this ReadOnlySpan<T> o, T value, IEqualityComparer<T>? comparer)
    {
        comparer ??= EqualityComparer<T>.Default;
        foreach (T? t in o)
        {
            if (comparer.Equals(t, value))
                return true;
        }

        return false;
    }
    public static bool Contains<T>(this ReadOnlySpan<T> o, T value) => Contains(o, value, null);

    public static T? FirstOrDefault<T>(this ReadOnlySpan<T> o) => o.FirstOrDefault(default);
    public static T? FirstOrDefault<T>(this ReadOnlySpan<T> o, T? @default) => o.IsEmpty ? @default : o[0];

    public static bool Contains<T>(this Span<T> o, T value, IEqualityComparer<T>? comparer)
    {
        comparer ??= EqualityComparer<T>.Default;
        foreach (T? t in o)
        {
            if (comparer.Equals(t, value))
                return true;
        }

        return false;
    }
    public static bool Contains<T>(this Span<T> o, T value) => Contains(o, value, null);

    public static T? FirstOrDefault<T>(this Span<T> o) => o.FirstOrDefault(default);
    public static T? FirstOrDefault<T>(this Span<T> o, T? @default) => o.IsEmpty ? @default : o[0];

    extension(string)
    {
        public static string Join<T>(char separator, ReadOnlySpan<T> span)
        {
            if (span.IsEmpty)
                return string.Empty;
            if (span.Length == 1)
                return span[0]!.ToString();
            StringBuilder sb = new();
            _ = sb.Append(span[0]!.ToString());
            for (int i = 1; i < span.Length; i++)
                _ = sb.Append(separator).Append(span[i]!.ToString());
            return sb.ToString();
        }

        public static string Join<T>(string separator, ReadOnlySpan<T> span)
        {
            if (span.IsEmpty)
                return string.Empty;
            if (span.Length == 1)
                return span[0]!.ToString();
            StringBuilder sb = new();
            _ = sb.Append(span[0]!.ToString());
            for (int i = 1; i < span.Length; i++)
                _ = sb.Append(separator).Append(span[i]!.ToString());
            return sb.ToString();
        }

        public static string Join<T>(char separator, ReadOnlySpan<T> span, Func<T, string> toString)
        {
            if (span.IsEmpty)
                return string.Empty;
            if (span.Length == 1)
                return toString(span[0]);
            StringBuilder sb = new();
            _ = sb.Append(toString(span[0]));
            for (int i = 1; i < span.Length; i++)
                _ = sb.Append(separator).Append(toString(span[i]));
            return sb.ToString();
        }

        public static string Join<T>(string separator, ReadOnlySpan<T> span, Func<T, string> toString)
        {
            if (span.IsEmpty)
                return string.Empty;
            if (span.Length == 1)
                return toString(span[0]);
            StringBuilder sb = new();
            _ = sb.Append(toString(span[0]));
            for (int i = 1; i < span.Length; i++)
                _ = sb.Append(separator).Append(toString(span[i]));
            return sb.ToString();
        }

    }
}
