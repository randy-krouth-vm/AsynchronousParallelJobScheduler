namespace ParallelJobScheduler
{
    internal enum RequestPriority
    {
        Hot,
        Cold,
        Unspecified
    };

    internal class Job
    {
        internal Func<CancellationToken, Task> task { get; set; }
        internal RequestPriority priority { get; set; } = RequestPriority.Unspecified;
        internal CancellationTokenSource cancellationTokenSource { get; set; } = new CancellationTokenSource();

        internal Job() { }

        internal Job(Func<CancellationToken, Task> task, RequestPriority priority)
        {
            if (task != null)
            {
                this.task = task;
                this.priority = priority;
            }
        }
    }

    internal class Job<T> : Job
    {
        internal Func<CancellationToken, Task<T>> Function { get; set; }
        internal TaskCompletionSource<T> Completion { get; set; } = new TaskCompletionSource<T>();

        internal Job(Func<CancellationToken, Task<T>> function, RequestPriority priority): base(null, priority)
        {
            if (function != null)
            {
                Function = function;

                this.task = async (token) =>
                {
                    try
                    {
                        T result = await function(token);
                        Completion.SetResult(result);
                    }

                    catch (Exception ex)
                    {
                        Completion.SetException(ex);
                        throw;
                    }
                };
            }
        }
    }

    internal static class ResourceAllocator
    {
        internal static BufferAllocator bufferOne = new BufferAllocator();
        internal static BufferAllocator bufferTwo = new BufferAllocator();
    }
}

