using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quartz;

namespace MapTalkie.Jobs
{
    public abstract class JsonJob<T> : IJob
    {
        private const string JsonField = "__Json";

        public Task Execute(IJobExecutionContext context)
        {
            T data;
            try
            {
                var stringValue = context.JobDetail.JobDataMap.GetString(JsonField)!;
                data = JsonConvert.DeserializeObject<T>(stringValue)!;
            }
            catch (Exception e)
            {
                throw new JobExecutionException(e, false);
            }

            return Execute(data, context);
        }

        protected abstract Task Execute(T data, IJobExecutionContext context);

        public static JobBuilder Builder(T data)
        {
            return JobBuilder
                .Create<JsonJob<T>>()
                .UsingJobData(JsonField, JsonConvert.SerializeObject(data));
        }
    }
}