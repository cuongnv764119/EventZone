using EventZone.Models;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventZone.Helpers
{

    public class CalculateEventScoreHelpers : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
           using (var db = new EventZoneEntities())
            {
               var listRank=db.EventRanks.ToList();
               foreach (var item in listRank)
                {
                    item.Score = EventDatabaseHelper.Instance.CalculateEventScore(item.EventId);
                }
                db.SaveChanges();
            };
        }
           
    }
    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<CalculateEventScoreHelpers>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0))
                  )
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}