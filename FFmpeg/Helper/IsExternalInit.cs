#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices;

/// <summary>
/// Allows to use init in .net standard
/// </summary>
internal static class IsExternalInit { }
#endif