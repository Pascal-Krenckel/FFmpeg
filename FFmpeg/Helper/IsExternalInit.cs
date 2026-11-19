#if !NET5_0_OR_GREATER
#pragma warning disable IDE0130 // Der Namespace entspricht stimmt nicht der Ordnerstruktur.
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130 // Der Namespace entspricht stimmt nicht der Ordnerstruktur.

/// <summary>
/// Allows to use init in .net standard
/// </summary>
internal static class IsExternalInit { }
#endif