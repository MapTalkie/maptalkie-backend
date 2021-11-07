using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;

namespace MapTalkie.Controllers
{
    class TestJob : IJob
    {
        private readonly ILogger<TestJob> _logger;

        public TestJob(ILogger<TestJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Hello world!");
            return Task.CompletedTask;
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        [HttpGet]
        public async Task<string> Get([FromServices] ISchedulerFactory factory)
        {
            var scheduler = factory.GetScheduler().Result;
            var scheduler2 = factory.GetScheduler().Result;
            var job = JobBuilder.Create<TestJob>().Build();
            var trigger = TriggerBuilder.Create().StartNow().Build();
            await scheduler.ScheduleJob(job, trigger);

            return "OK";
        }
    }
}