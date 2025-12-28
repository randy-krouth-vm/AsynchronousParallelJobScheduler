namespace ParallelJobScheduler
{
    public partial class Program
    {
#if DEBUG
        internal static void OutputExceptions(Exception ex)
        {
            Console.WriteLine(ex);
        }
#endif
    }
}
