using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;

namespace Diplom.Models.App
{
    public class AlertSheduler
    {
        public static void Start()
        {
            DateTime start = DateTime.Now;
            start = start.AddSeconds(60);
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<AlertSender>().Build();

            ITrigger trigger = TriggerBuilder.Create()  // создаем триггер
                .WithIdentity("trigger1", "group1")     // идентифицируем триггер с именем и группой
                .StartAt(new DateTimeOffset(start)) // запуск по таймеру
                .WithSimpleSchedule(x => x            // настраиваем выполнение действия
                    //.WithIntervalInMinutes(60)          // через 1 минуту
                    .WithIntervalInHours(1)              // через каждый час
                    .RepeatForever())                   // бесконечное повторение
                .Build();                               // создаем триггер

            scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы
        }
    }
}