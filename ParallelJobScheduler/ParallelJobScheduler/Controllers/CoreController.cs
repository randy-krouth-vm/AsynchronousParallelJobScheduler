using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ParallelJobScheduler.Controllers
{
    [Route("api/core-controller")]
    [ApiController]
    public class CoreController : ControllerBase
    {
        private readonly ModelService model;

        public CoreController(ModelService model)
        {
            this.model = model;
        }

        [HttpGet("cold-job-request/{numberOfColdJobsToRun}")]
        public async Task<ActionResult> ColdJobRequest(int numberOfColdJobsToRun = 5)
        {
            try
            {
                for (int i = 1; i <= numberOfColdJobsToRun; ++i)
                {
                    int jobIndex = i;

                    Job job = new Job(async (token) =>
                    {
                        await model.ColdJob(jobIndex, token);
                    }, RequestPriority.Cold);

                    ResourceAllocator.bufferOne.AddJob(job);
                }

                return Ok(JsonSerializer.Serialize(new ResponseModel("Cold job succesfully added to queue.")));
            }

            catch (Exception ex)
            {
                Program.OutputExceptions(ex);
            }

            return StatusCode(500, StatusCodes.Status500InternalServerError);
        }

        [HttpGet("hot-job-request")]
        public async Task<ActionResult> HotJobRequest()
        {
            try
            {
                Job job = new Job(async (token) =>
                {
                    await model.HotJob();
                }, RequestPriority.Hot);

                ResourceAllocator.bufferOne.AddJob(job);

                return Ok(JsonSerializer.Serialize(new ResponseModel("Hot job succesfully added to queue.")));
            }

            catch (Exception ex)
            {
                Program.OutputExceptions(ex);
            }

            return StatusCode(500, StatusCodes.Status500InternalServerError);
        }
    }
}
