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

}
