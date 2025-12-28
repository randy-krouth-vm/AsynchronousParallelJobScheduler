
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelJobScheduler
{
    /* 
     * Notes: This program demonstrates a Parallel Job Scheduler which requeues cold jobs on cancellation, 
     * and processes hot jobs as soon as a cold job is cancelled.
     * 
     * Please use swagger to initiate cold job requests, and then may proceed by making hot job requests to endpoint.
     */

    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<ModelService>();
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            _ = ResourceAllocator.bufferOne.StartJobSchedulingService();

            app.Run();
        }
    }
}
