namespace ParallelJobScheduler
{
    public enum RequestPriority
    {
        Hot,
        Cold,
        Unspecified
    };

    public class Job
    {
        internal Func<CancellationToken, Task> task { get; set; }
        internal RequestPriority priority { get; set; } = RequestPriority.Unspecified;
        internal CancellationTokenSource cancellationTokenSource { get; set; } = new CancellationTokenSource();

        public Job() { }

        public Job(Func<CancellationToken, Task> task, RequestPriority priority)
        {
            this.task = task;
            this.priority = priority;
        }
    }

    public class Job<T> : Job
    {
        public Func<CancellationToken, Task<T>> Function { get; }
        public TaskCompletionSource<T> Completion { get; } = new TaskCompletionSource<T>();

        public Job(Func<CancellationToken, Task<T>> function, RequestPriority priority)
            : base(null, priority)
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

    public static class ResourceAllocator
    {
        public static BufferAllocator bufferOne = new BufferAllocator();
        public static BufferAllocator bufferTwo = new BufferAllocator();
    }
}

