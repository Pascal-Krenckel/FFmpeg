using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Threading;
public class AutoResetAllEvent : IDisposable
{
    private readonly object _lock = new();
    private TaskCompletionSource<int> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool disposedValue;

    public void Notify()
    {
        if (disposedValue)
            throw new ObjectDisposedException(nameof(AutoResetAllEvent));
        lock (_lock)
        {
            _ = tcs.TrySetResult(0);
            tcs = new TaskCompletionSource<int>();
            if (disposedValue)
                _ = tcs.TrySetCanceled();
        }
    }

    public Task Task => tcs.Task;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _ = tcs.TrySetCanceled();
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            disposedValue = true;
        }
    }

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~AutoResetAllEvent()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }

    void IDisposable.Dispose()
    {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void Cancel()
    {
        if (disposedValue)
            throw new ObjectDisposedException(nameof(AutoResetAllEvent));
        lock (_lock)
        {
            _ = tcs.TrySetCanceled();
            tcs = new TaskCompletionSource<int>();
            if (disposedValue)
                _ = tcs.TrySetCanceled();
        }
    }
}
