namespace FFmpeg.Utils;
/// <summary>
/// Represents an interface for managing references to unmanaged objects that must be disposed,
/// such as FFmpeg objects, without requiring the implementing struct to handle disposal directly.
/// </summary>
/// <typeparam name="T">The type of the referenced object, which must implement <see cref="IDisposable"/>.</typeparam>
public interface IReference<T> where T : class, IDisposable
{
    /// <summary>
    /// Gets the object currently being referenced. Or a copy if the type is nor reference counted.
    /// </summary>
    /// <returns>The referenced object of type <typeparamref name="T"/> if available, or <see langword="null"> if not.</returns>
    T? GetReferencedObject();

    /// <summary>
    /// Sets the reference to a new object of type <typeparamref name="T"/> or creates a copy if the type is not reference counted or unlinks the current reference.
    /// </summary>
    /// <param name="obj">The new object to reference, or <see langword="null"> to release the reference.</param>
    void SetReferencedObject(T? obj);
}
