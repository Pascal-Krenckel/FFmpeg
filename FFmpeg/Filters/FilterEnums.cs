namespace FFmpeg.Filters;

/// <summary>When a filter evaluates its expressions (option <c>eval</c> of e.g. <c>scale</c>, <c>pad</c>, <c>overlay</c>, <c>volume</c>).</summary>
public enum ExpressionEvaluation
{
    /// <summary>Once, when the filter is initialized or its input is (re)configured (<c>init</c>/<c>once</c>).</summary>
    Once = 0,

    /// <summary>For every frame (<c>frame</c>).</summary>
    PerFrame = 1,
}

/// <summary>The direction of a fade (<c>fade</c>, <c>afade</c>).</summary>
public enum FadeType
{
    /// <summary>Fade in (<c>in</c>).</summary>
    In = 0,

    /// <summary>Fade out (<c>out</c>).</summary>
    Out = 1,
}
