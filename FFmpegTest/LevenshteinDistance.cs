using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpegTest;
/// <summary>
/// Contains approximate string matching
/// </summary>
static class LevenshteinDistance
{
    /// <summary>
    /// Compute the distance between two strings.
    /// </summary>
    public static int Compute<T>(IReadOnlyList<T> s, IReadOnlyList<T> t, IEqualityComparer<T>? comparer = null)
    {
        comparer ??= EqualityComparer<T>.Default;
        int n = s.Count;
        int m = t.Count;
        int[,] d = new int[n + 1, m + 1];

        // Step 1
        if (n == 0)
        {
            return m;
        }

        if (m == 0)
        {
            return n;
        }

        // Step 2
        for (int i = 0; i <= n; d[i, 0] = i++) ;

        for (int j = 0; j <= m; d[0, j] = j++) ;

        // Step 3
        for (int i = 1; i <= n; i++)
        {
            //Step 4
            for (int j = 1; j <= m; j++)
            {
                // Step 5
                int cost = (comparer.Equals(t[j - 1], s[i - 1])) ? 0 : 1;

                // Step 6
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        // Step 7
        return d[n, m];
    }

    /// <summary>
    /// Compute the distance between two strings.
    /// </summary>
    public static int Compute<T>(ReadOnlySpan<T> s, ReadOnlySpan<T> t, IEqualityComparer<T>? comparer = null)
    {
        comparer ??= EqualityComparer<T>.Default;
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // Step 1
        if (n == 0)
        {
            return m;
        }

        if (m == 0)
        {
            return n;
        }

        // Step 2
        for (int i = 0; i <= n; d[i, 0] = i++) ;

        for (int j = 0; j <= m; d[0, j] = j++) ;

        // Step 3
        for (int i = 1; i <= n; i++)
        {
            //Step 4
            for (int j = 1; j <= m; j++)
            {
                // Step 5
                int cost = (comparer.Equals(t[j - 1], s[i - 1])) ? 0 : 1;

                // Step 6
                int deletion = d[i-1,j] + 1;
                int insertion = d[i,j-1]+ 1;
                int replace = d[i-1,j-1] + cost;

                d[i, j] = Math.Min(deletion,Math.Min( insertion, replace));
            }
        }
        // Step 7
        return d[n, m];
    }

}
