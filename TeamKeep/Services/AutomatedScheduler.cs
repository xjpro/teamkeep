using Quartz;
using Quartz.Impl;

namespace TeamKeep.Services
{
    public static class AutomatedScheduler
    {
        private static bool Running;
        private static readonly StdSchedulerFactory _factory = new StdSchedulerFactory();

        public static void Start()
        {
            if (!Running)
            {
                var scheduler = _factory.GetScheduler();

                var automatedEmailsJob = JobBuilder
                    .Create<AutomatedEmailsJob>()
                    .WithIdentity("automatedEmailsJob", "group1")
                    .Build();

                var automatedEmailsTrigger = TriggerBuilder
                    .Create()
                    .WithIdentity("automatedEmailsTrigger", "group1")
                    //.StartAt(DateBuilder.FutureDate(10, IntervalUnit.Minute)) // start in 10 minutes
                    .StartAt(DateBuilder.FutureDate(60, IntervalUnit.Second)) // testing: start in 60 seconds
                    .WithSimpleSchedule(x => x.WithIntervalInHours(2).RepeatForever()) // repeat every 2 hours, forever
                    .Build();

                scheduler.ScheduleJob(automatedEmailsJob, automatedEmailsTrigger);
                scheduler.Start();
                Running = true;
            }
        }

        public static void Stop()
        {
            if (Running)
            {
                foreach (var scheduler in _factory.AllSchedulers)
                {
                    scheduler.Shutdown(true);
                }
                Running = false;
            }
        }
    }
}