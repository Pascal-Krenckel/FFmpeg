namespace FFmpeg.Threading;

// Provides a task scheduler that ensures a maximum concurrency level while
// running on top of the thread pool.
public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
{
    // Indicates whether the current thread is processing work items.
    [ThreadStatic]
    private static bool _currentThreadIsProcessingItems;

    // The list of tasks to be executed
    private readonly LinkedList<Task> _tasks = new(); // protected by lock(_tasks)

    // The maximum concurrency level allowed by this scheduler.
    private int _maxDegreeOfParallelism;

    // Indicates whether the scheduler is currently processing work items.
    private int _delegatesQueuedOrRunning = 0;

    // Creates a new instance with the specified degree of parallelism.
    public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
    {
        if (maxDegreeOfParallelism < 1)
            throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
        _maxDegreeOfParallelism = maxDegreeOfParallelism;
    }

    // Queues a task to the scheduler.
    protected sealed override void QueueTask(Task task)
    {
        // Add the task to the list of tasks to be processed.  If there aren't enough
        // delegates currently queued or running to process tasks, schedule another.
        lock (_tasks)
        {
            _ = _tasks.AddLast(task);
            if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
            {
                ++_delegatesQueuedOrRunning;
                NotifyThreadPoolOfPendingWork();
            }
        }
    }

    // Inform the ThreadPool that there's work to be executed for this scheduler.
    private void NotifyThreadPoolOfPendingWork() => _ = ThreadPool.UnsafeQueueUserWorkItem(_ =>
                                                         {
                                                             // Note that the current thread is now processing work items.
                                                             // This is necessary to enable inlining of tasks into this thread.
                                                             _currentThreadIsProcessingItems = true;
                                                             try
                                                             {
                                                                 // Process all available items in the queue.
                                                                 while (true)
                                                                 {
                                                                     Task item;
                                                                     lock (_tasks)
                                                                     {
                                                                         // When there are no more items to be processed,
                                                                         // note that we're done processing, and get out.
                                                                         if (_tasks.Count == 0)
                                                                         {
                                                                             --_delegatesQueuedOrRunning;
                                                                             break;
                                                                         }

                                                                         // Get the next item from the queue
                                                                         item = _tasks.First.Value;
                                                                         _tasks.RemoveFirst();
                                                                     }

                                                                     // Execute the task we pulled out of the queue
                                                                     _ = base.TryExecuteTask(item);
                                                                 }
                                                             }
                                                             // We're done processing items on the current thread
                                                             finally { _currentThreadIsProcessingItems = false; }
                                                         }, null);

    // Attempts to execute the specified task on the current thread.
    protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        // If this thread isn't already processing a task, we don't support inlining
        if (!_currentThreadIsProcessingItems)
            return false;

        // If the task was previously queued, remove it from the queue
        if (taskWasPreviouslyQueued)
            // Try to run the task.
            return TryDequeue(task) && base.TryExecuteTask(task);
        else
            return base.TryExecuteTask(task);
    }

    // Attempt to remove a previously scheduled task from the scheduler.
    protected sealed override bool TryDequeue(Task task)
    {
        lock (_tasks)
            return _tasks.Remove(task);
    }

    // Gets the maximum concurrency level supported by this scheduler.
    public sealed override int MaximumConcurrencyLevel => _maxDegreeOfParallelism;
    public void SetMaxDegreeOfParallelism(int maxDegreeOfParallelism)
    {
        if (maxDegreeOfParallelism < 1)
            throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
        if (Monitor.TryEnter(_tasks))
        {
            try  // try enter _tasks and if successful, adjust the running threads if needed
            {
                _maxDegreeOfParallelism = maxDegreeOfParallelism;
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    int toLaunch = _maxDegreeOfParallelism - _delegatesQueuedOrRunning;
                    _delegatesQueuedOrRunning = _maxDegreeOfParallelism;
                    for (int i = 0; i < toLaunch; i++)
                        NotifyThreadPoolOfPendingWork();
                }
            }
            finally { Monitor.Exit(_tasks); }
        }
        else // tasks is locked, just set the value, it will be used when possible
        {
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }
    }

    // Gets an enumerable of the tasks currently scheduled on this scheduler.
    protected sealed override IEnumerable<Task> GetScheduledTasks()
    {
        bool lockTaken = false;
        try
        {
            Monitor.TryEnter(_tasks, ref lockTaken);
            return lockTaken ? (IEnumerable<Task>)_tasks : throw new NotSupportedException();
        }
        finally
        {
            if (lockTaken)
                Monitor.Exit(_tasks);
        }
    }
}
