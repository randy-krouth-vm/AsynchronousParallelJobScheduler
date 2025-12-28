using System.Collections.Concurrent;

namespace ParallelJobScheduler
{
    public class BufferAllocator
    {
        private CancellationTokenSource cancellationTokenSource { get; set; } = new CancellationTokenSource();

        private List<Job> coldQueue { get; set; } = new List<Job>();
        private List<Job> hotQueue { get; set; } = new List<Job>();
        private List<Job> activeColdJobs { get; set; } = new List<Job>();

        private object _lock { get; set; } = new object();
        
        private const int maxTaskCount = 5;
        private SemaphoreSlim semaphoreSlim { get; set; } = new SemaphoreSlim(maxTaskCount);

        public async Task StartJobSchedulingService()
        {
            for (int i = 0; i < maxTaskCount; i++)
            {
                _ = Task.Run(ProcessColdQueue);
                _ = Task.Run(ProcessHotQueue);
            }
        }

        public void AddJob(Job job)
        {
            try
            {
                if (job.priority == RequestPriority.Cold)
                {
                    lock (_lock)
                    {
                        coldQueue.Add(job);
                    }
                }

                else
                {
                    lock (_lock)
                    {
                        if (activeColdJobs.Count >= 1)
                        {
                            Job coldJob = activeColdJobs[^1];
                            coldJob.cancellationTokenSource.Cancel();
                            coldJob.cancellationTokenSource = new CancellationTokenSource();
                            coldQueue.Insert(0, coldJob);
                        }

                        hotQueue.Add(job);
                    }
                }

            }
            catch (Exception ex)
            {
                Program.OutputExceptions(ex);
            }
        }

        private async Task ProcessColdQueue()
        {
            while (cancellationTokenSource.Token.IsCancellationRequested == false)
            {
                Job job = null;
                bool acquired = false;

                try
                {
                    while (true)
                    {
                        bool shouldWait = false;

                        lock (_lock)
                        {
                            shouldWait = (coldQueue.Count == 0 && activeColdJobs.Count == 0);
                        }

                        if (shouldWait == false)
                        {
                            break;
                        }

                        await Task.Delay(10);
                    }

                    await semaphoreSlim.WaitAsync();

                    acquired = true;

                    lock (_lock)
                    {
                        if (coldQueue.Count >= 1 && (hotQueue.Count + activeColdJobs.Count) < maxTaskCount)
                        {
                            job = coldQueue[0];
                            activeColdJobs.Add(job);
                            coldQueue.RemoveAt(0);
                        }
                    }

                    if (job != null)
                    {
                        await job.task(job.cancellationTokenSource.Token);
                    }
                }

                catch (Exception ex)
                {
                    Program.OutputExceptions(ex);
                }

                finally
                {
                    if (job != null)
                    {
                        lock (_lock)
                        {
                            activeColdJobs.Remove(job);
                        }
                    }

                    if (acquired == true)
                    {
                        semaphoreSlim.Release();
                    }
                }
            }
        }

        private async Task ProcessHotQueue()
        {
            while (cancellationTokenSource.Token.IsCancellationRequested == false)
            {
                Job job = null;
                bool acquired = false;

                try
                {
                    while (true)
                    {
                        bool shouldWait = false;

                        lock (_lock)
                        {
                            shouldWait = (hotQueue.Count == 0);
                        }

                        if (shouldWait == false)
                        {
                            break;
                        }

                        await Task.Delay(10);
                    }

                    await semaphoreSlim.WaitAsync();

                    acquired = true;

                    lock (_lock)
                    {
                        if (hotQueue.Count >= 1)
                        {
                            job = hotQueue[0];
                            hotQueue.RemoveAt(0);
                        }
                    }

                    if (job != null)
                    {
                        await job.task(job.cancellationTokenSource.Token);
                    }
                }

                catch (Exception ex)
                {
                    Program.OutputExceptions(ex);
                }

                finally
                {
                    if (acquired)
                    {
                        semaphoreSlim.Release();
                    }
                }
            }
        }
    }
}