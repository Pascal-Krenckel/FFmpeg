using FFmpeg.Codecs;
using FFmpeg.Filters;
using FFmpeg.Utils;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace FFmpeg.Threading;

/// <summary>
/// Executes FFmpeg send and receive operations asynchronously using a limited number
/// of dedicated worker threads.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="FFmpegWorkQueue"/> serializes access to FFmpeg codec and filter
/// operations while allowing multiple independent operations to execute concurrently.
/// The number of active workers is controlled by <see cref="DesiredConcurrency"/>.
/// </para>
/// <para>
/// Operations submitted through the <c>Send*</c> and <c>Receive*</c> methods are queued
/// and executed by the worker threads. The returned <see cref="CancellableWorkItem"/>
/// can be used to observe completion and to cancel an operation that has not yet
/// completed.
/// </para>
/// <para>
/// This class does not take ownership of any objects supplied to its send or receive
/// methods. In particular, supplied <see cref="AVPacket"/> and <see cref="AVFrame"/>
/// instances remain owned by the caller and must be disposed by the caller when they
/// are no longer needed. The caller must also ensure that supplied codec and filter
/// contexts remain valid until the corresponding operation has completed.
/// </para>
/// <para>
/// <see cref="ReceiveHwFrameAsync(CodecContext, AVFrame, CancellationToken)"/> does
/// not transfer the received frame to system memory. The supplied frame may therefore
/// contain hardware-backed data. Callers that require a software frame must explicitly
/// perform the appropriate hardware-to-software transfer.
/// </para>
/// </remarks>
public sealed class FFmpegWorkQueue : IDisposable, IAsyncDisposable
{
    private bool _disposed;
    private readonly object _lock = new();
    private readonly LinkedList<Worker> _threads = [];
    private readonly BlockingCollection<CancellableWorkItem> queue = [];

    /// <summary>
    /// Gets the number of worker threads currently active.
    /// </summary>
    /// <value>
    /// The current number of worker threads.
    /// </value>
    /// <remarks>
    /// This value may temporarily differ from <see cref="DesiredConcurrency"/> while
    /// workers are being created or stopped.
    /// </remarks>
    public int Concurrency
    {
        get
        {
            lock (_lock)
                return _threads.Count;
        }
    }

    /// <summary>
    /// Gets or sets the desired number of worker threads.
    /// </summary>
    /// <value>
    /// The desired number of concurrent worker threads. A value less than or equal to zero
    /// selects <see cref="Environment.ProcessorCount"/> automatically.
    /// </value>
    /// <remarks>
    /// Increasing this value causes additional workers to be created as necessary.
    /// Decreasing it causes excess workers to terminate after completing their current
    /// operation.
    /// </remarks>
    public int DesiredConcurrency
    {
        get => Volatile.Read(ref field);
        set
        {
            value = value <= 0 ? Environment.ProcessorCount : value;
            lock (_lock)
            {
                field = value;
                if (field > _threads.Count)
                {
                    if (field <= _threads.Count || _disposed)
                        return;
                    for (int i = _threads.Count; i < field; i++)
                        _ = _threads.AddLast(Worker.StartNew(Work));
                }
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegWorkQueue"/> class.
    /// </summary>
    /// <param name="concurrency">
    /// The desired number of worker threads, or a value less than or equal to zero to use
    /// <see cref="Environment.ProcessorCount"/>.
    /// </param>
    public FFmpegWorkQueue(int concurrency = 0)
    {
        // no need to check for value smaller then 0, just set to processor count, fast reliable, accaptable. No question of I have to pass 0 or -1 for auto-detect.
        if (concurrency <= 0)
            concurrency = Environment.ProcessorCount;
        _threads = new LinkedList<Worker>();
        DesiredConcurrency = concurrency;
    }

    /// <summary>
    /// Queues a packet to be sent to the specified decoder.
    /// </summary>
    /// <param name="decoder">
    /// The decoder to which the packet is sent.
    /// </param>
    /// <param name="packet">
    /// The packet to send, or <see langword="null"/> to signal end-of-input to the decoder.
    /// Ownership of the packet remains with the caller.
    /// </param>
    /// <param name="token">
    /// A token used to cancel the queued operation.
    /// </param>
    /// <returns>
    /// A <see cref="CancellableWorkItem"/> representing the queued operation.
    /// </returns>
    /// <remarks>
    /// The decoder and packet must remain valid until this operation has completed.
    /// This method does not take ownership of either object.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="FFmpegWorkQueue"/> has already been disposed.
    /// </exception>
    public CancellableWorkItem SendPacketAsync(CodecContext decoder, AVPacket? packet, CancellationToken token = default)
    {
        lock (_lock)
        {
            CheckDisposed();
            CancellableWorkItem task = new(() => decoder.SendPacket(packet), token);
            queue.Add(task);
            return task;
        }
    }

    /// <summary>
    /// Queues an operation to receive a decoded frame from the specified decoder.
    /// </summary>
    /// <param name="decoder">
    /// The decoder from which the frame is received.
    /// </param>
    /// <param name="frame">
    /// The frame into which the decoded data is written. Ownership of the frame remains
    /// with the caller.
    /// </param>
    /// <param name="token">
    /// A token used to cancel the queued operation.
    /// </param>
    /// <returns>
    /// A <see cref="CancellableWorkItem"/> representing the queued operation.
    /// </returns>
    /// <remarks>
    /// The decoder and frame must remain valid until this operation has completed.
    /// This method does not take ownership of either object.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="FFmpegWorkQueue"/> has already been disposed.
    /// </exception>
    public CancellableWorkItem ReceiveFrameAsync(CodecContext decoder, AVFrame frame, CancellationToken token = default)
    {
        lock (_lock)
        {
            CheckDisposed();
            CancellableWorkItem task = new(() => decoder.ReceiveFrame(frame), token);
            queue.Add(task);
            return task;
        }
    }

    /// <summary>
    /// Queues an operation to receive a frame from a filter buffer sink.
    /// </summary>
    /// <param name="bufferSink">
    /// The filter buffer sink from which the frame is received.
    /// </param>
    /// <param name="frame">
    /// The frame into which the received data is written. Ownership of the frame remains
    /// with the caller.
    /// </param>
    /// <param name="token">
    /// A token used to cancel the queued operation.
    /// </param>
    /// <returns>
    /// A <see cref="CancellableWorkItem"/> representing the queued operation.
    /// </returns>
    /// <remarks>
    /// The filter context and frame must remain valid until this operation has completed.
    /// This method does not take ownership of either object.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="FFmpegWorkQueue"/> has already been disposed.
    /// </exception>
    public CancellableWorkItem ReceiveFrameAsync(FilterContext bufferSink, AVFrame frame, CancellationToken token = default)
    {
        lock (_lock)
        {
            CheckDisposed();
            CancellableWorkItem task = new(() => ((IBufferSink)bufferSink).ReceiveFrame(frame), token);
            queue.Add(task);
            return task;
        }
    }

    /// <summary>
    /// Queues an operation to receive a decoded frame from the specified decoder without
    /// explicitly transferring hardware frames to system memory.
    /// </summary>
    /// <param name="decoder">
    /// The decoder from which the frame is received.
    /// </param>
    /// <param name="frame">
    /// The frame into which the decoded data is written. The frame may contain
    /// hardware-backed data. Ownership of the frame remains with the caller.
    /// </param>
    /// <param name="token">
    /// A token used to cancel the queued operation.
    /// </param>
    /// <returns>
    /// A <see cref="CancellableWorkItem"/> representing the queued operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Unlike a receive operation that explicitly transfers a hardware frame to system
    /// memory, this method preserves the frame's native representation. Depending on the
    /// decoder and its hardware configuration, the resulting frame may therefore contain
    /// hardware-backed data rather than data directly accessible from system memory.
    /// </para>
    /// <para>
    /// The decoder and frame must remain valid until this operation has completed.
    /// This method does not take ownership of either object.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="FFmpegWorkQueue"/> has already been disposed.
    /// </exception>
    public CancellableWorkItem ReceiveHwFrameAsync(CodecContext decoder, AVFrame frame, CancellationToken token = default)
    {
        lock (_lock)
        {
            CheckDisposed();
            CancellableWorkItem task = new(() => decoder.ReceiveHWFrame(frame), token);
            queue.Add(task);
            return task;
        }
    }

    /// <summary>
    /// Queues an operation to receive an encoded packet from the specified encoder.
    /// </summary>
    /// <param name="encoder">
    /// The encoder from which the packet is received.
    /// </param>
    /// <param name="packet">
    /// The packet into which the encoded data is written. Ownership of the packet remains
    /// with the caller.
    /// </param>
    /// <param name="token">
    /// A token used to cancel the queued operation.
    /// </param>
    /// <returns>
    /// A <see cref="CancellableWorkItem"/> representing the queued operation.
    /// </returns>
    /// <remarks>
    /// The encoder and packet must remain valid until this operation has completed.
    /// This method does not take ownership of either object.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="FFmpegWorkQueue"/> has already been disposed.
    /// </exception>
    public CancellableWorkItem ReceivePacketAsync(CodecContext encoder, AVPacket packet, CancellationToken token = default)
    {
        lock (_lock)
        {
            CheckDisposed();
            CancellableWorkItem task = new(() => encoder.ReceivePacket(packet), token);
            queue.Add(task);
            return task;
        }
    }

    /// <summary>
    /// Queues a frame to be sent to the specified encoder.
    /// </summary>
    /// <param name="encoder">
    /// The encoder to which the frame is sent.
    /// </param>
    /// <param name="frame">
    /// The frame to send, or a frame representing end-of-input as required by FFmpeg.
    /// Ownership of the frame remains with the caller.
    /// </param>
    /// <param name="token">
    /// A token used to cancel the queued operation.
    /// </param>
    /// <returns>
    /// A <see cref="CancellableWorkItem"/> representing the queued operation.
    /// </returns>
    /// <remarks>
    /// The encoder and frame must remain valid until this operation has completed.
    /// This method does not take ownership of either object.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="FFmpegWorkQueue"/> has already been disposed.
    /// </exception>
    public CancellableWorkItem SendFrameAsync(CodecContext encoder, AVFrame frame, CancellationToken token = default)
    {
        lock (_lock)
        {
            CheckDisposed();
            CancellableWorkItem task = new(() => encoder.SendFrame(frame), token);
            queue.Add(task);
            return task;
        }
    }

    /// <summary>
    /// Queues a frame to be sent to a filter buffer source.
    /// </summary>
    /// <param name="bufferSource">
    /// The filter buffer source to which the frame is sent.
    /// </param>
    /// <param name="frame">
    /// The frame to send. Ownership of the frame remains with the caller.
    /// </param>
    /// <param name="token">
    /// A token used to cancel the queued operation.
    /// </param>
    /// <returns>
    /// A <see cref="CancellableWorkItem"/> representing the queued operation.
    /// </returns>
    /// <remarks>
    /// The filter context and frame must remain valid until this operation has completed.
    /// This method does not take ownership of either object.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="FFmpegWorkQueue"/> has already been disposed.
    /// </exception>
    public CancellableWorkItem SendFrameAsync(FilterContext bufferSource, AVFrame frame, CancellationToken token = default)
    {
        lock (_lock)
        {
            CheckDisposed();
            CancellableWorkItem task = new(() => ((IBufferSource)bufferSource).SendFrame(frame), token);
            queue.Add(task);
            return task;
        }
    }

    /// <summary>
    /// Queues an arbitrary FFmpeg operation for execution by the worker threads.
    /// </summary>
    /// <param name="workItem">
    /// The function to execute. The function is executed synchronously by one of the
    /// queue's worker threads and must return the <see cref="AVResult32"/> produced by
    /// the operation.
    /// </param>
    /// <param name="token">
    /// A token used to cancel the operation while it is waiting in the queue.
    /// Cancellation does not interrupt the function once execution has begun.
    /// </param>
    /// <returns>
    /// A <see cref="CancellableWorkItem"/> representing the queued operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method can be used to queue FFmpeg operations that are not covered by the
    /// specialized send and receive methods, such as seeking or other operations that
    /// need to be serialized with the decoder, encoder, or filter operations executed
    /// by this queue.
    /// </para>
    /// <para>
    /// The supplied function is not executed by this method. It is invoked later by
    /// one of the queue's worker threads, subject to the queue's concurrency limit.
    /// </para>
    /// <para>
    /// Cancellation is only effective while the operation is waiting to be executed.
    /// Once the function has started, it cannot be interrupted by the supplied
    /// <paramref name="token"/>.
    /// </para>
    /// <para>
    /// This method does not take ownership of any objects captured by
    /// <paramref name="workItem"/>. Any such objects must remain valid until the
    /// operation has completed and must be disposed by their owner.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="workItem"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// The <see cref="FFmpegWorkQueue"/> has already been disposed.
    /// </exception>
    public CancellableWorkItem QueueFFmpegWorkItem(
        Func<AVResult32> workItem,
        CancellationToken token = default)
    {
        if (workItem == null)
            throw new ArgumentNullException(nameof(workItem));
        lock (_lock)
        {
            CheckDisposed();
            CancellableWorkItem task = new(workItem, token);
            queue.Add(task);
            return task;
        }
    }

    private void CheckDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
    private void DisposeQueue()
    {
        queue.CompleteAdding();
        while (queue.TryTake(out CancellableWorkItem? task))
            _ = task.Cancel();
    }
    private void Work(Worker worker)
    {
        try
        {
            CancellationToken token = worker.Token;
            while (!token.IsCancellationRequested)
            {
                if (Concurrency > DesiredConcurrency)
                {
                    lock (_lock)
                    {
                        if (Concurrency <= DesiredConcurrency)
                            continue;
                        _ = _threads.Remove(worker);
                        return;
                    }
                }
                CancellableWorkItem task = queue.Take(token);
                if (_disposed)
                    _ = task.Cancel();
                else
                    task.Run();

            }
        }
        catch (InvalidOperationException) { }
        catch (OperationCanceledException)
        { }
        catch (Exception ex) // should never happen
        {
            Debug.Fail(ex.Message);
            throw;
        }
        finally
        {
            worker.Dispose();
            lock (_lock)
                _ = _threads.Remove(worker);
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="FFmpegWorkQueue"/> and
    /// waits for all worker threads to terminate.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Queued operations that have not started are cancelled and no new operations may
    /// be submitted after disposal begins.
    /// </para>
    /// <para>
    /// This method blocks until all currently executing operations and worker threads have
    /// terminated.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        Task tasksFinished;
        lock (_lock)
        {
            if (_disposed)
                return;
            _disposed = true;
            DisposeQueue();
            tasksFinished = Task.WhenAll(_threads.Select(worker => worker.WorkerThread));
            foreach (Worker worker in _threads)
                worker.Cancel();
        }
        tasksFinished.Wait();


    }

    /// <summary>
    /// Asynchronously releases all resources used by the
    /// <see cref="FFmpegWorkQueue"/> and waits for all worker threads to
    /// terminate.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous disposal operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Queued operations that have not started are cancelled and no new operations may
    /// be submitted after disposal begins.
    /// </para>
    /// <para>
    /// Unlike <see cref="Dispose"/>, this method does not synchronously block the calling
    /// thread while waiting for the worker threads to terminate.
    /// </para>
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        Task tasksFinished;
        lock (_lock)
        {
            if (_disposed)
                return;
            _disposed = true;
            DisposeQueue();
            tasksFinished = Task.WhenAll(_threads.Select(worker => worker.WorkerThread));
            foreach (Worker worker in _threads)
                worker.Cancel();
        }
        await tasksFinished.ConfigureAwait(false);
    }

    private sealed class Worker
    {
        private CancellationTokenSource? cts = new();
        public CancellationToken Token { get; }
        public Task WorkerThread { get; private set; } = Task.CompletedTask;

        private Worker() => Token = cts.Token;

        public void Cancel()
        {
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException) { }
        }

        public void Dispose()
        {
            cts?.Dispose();
            cts = null;
        }

        public static Worker StartNew(Action<Worker> action)
        {
            Worker worker = new();
            Task task = Task.Factory.StartNew(() => action(worker), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            worker.WorkerThread = task;
            return worker;
        }
    }
}
