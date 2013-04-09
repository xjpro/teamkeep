using System;
using System.Linq;
using Quartz;
using Quartz.Impl;
using TeamKeep.Models;

namespace TeamKeep.Services
{
    public class AutomatedTasksService
    {
        public class AutomatedEmailsJob : IJob
        {
            private const int MaximumEmailsPerExecution = 50;

            public void Execute(IJobExecutionContext context)
            {
                var emailService = new EmailService { AutomaticallySend = false };

                using (var entities = Database.GetEntities())
                {
                    var teamDatas = entities.TeamSettingsDatas.Where(x => x.AvailabilityEmailMinutes != null).ToList();
                    foreach (var teamData in teamDatas) // For each team that does availability emails
                    {
                        var abWindow = DateTime.Now.AddMinutes((double) teamData.AvailabilityEmailMinutes);

                        var futureGameDatas = entities.GameDatas.Where(x => 
                            x.HomeTeamId == teamData.TeamId && // only games of this team
                            x.Date != null &&  // with a date set
                            x.Date.Value.CompareTo(DateTime.Now) > 0 && // and the date hasn't passed
                            abWindow.CompareTo(x.Date.Value) >= 0) // and now+the ab minutes is later than the date
                                .ToList();

                        foreach (var gameData in futureGameDatas) // For each game of this team which hasn't passed but is within availability email window
                        {
                            var abDatas = entities.AvailabilityDatas.Where(x => 
                                x.EventId == gameData.Id && 
                                x.EmailSent == null).ToList(); // that hasn't already had its email sent

                            foreach (var abData in abDatas) // For each availability data of the future game, which has not had an email sent
                            {
                                var playerEmail = entities.PlayerDatas.Single(x => x.Id == abData.PlayerId).Email;

                                abData.EmailSent = DateTime.Now;
                                abData.Token = AuthToken.Generate(abData.PlayerId, playerEmail).Key;

                                var eventData = entities.GameDatas.Single(x => x.Id == abData.EventId);
                                var eventLocationData = entities.GameLocationDatas.Single(x => x.GameId == abData.EventId);
                                var abEvent = new Game(eventData);
                                abEvent.Location = eventLocationData;

                                var abRequest = new AvailabilityRequest 
                                {
                                    Data = abData,
                                    Event = abEvent,
                                    Email = playerEmail,
                                    TeamName = entities.TeamDatas.Single(x => x.Id == abEvent.HomeTeamId).Name
                                };

                                // Send out the email!
                                emailService.EmailAvailability(abRequest);
                            }

                            entities.SaveChanges();
                        }    
                    }
                }

                emailService.SendQueuedMessages();
            }
        }

        public void ScheduleTasks()
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = schedulerFactory.GetScheduler();

            var automatedEmailsJob = JobBuilder.Create<AutomatedEmailsJob>()
                                                      .WithIdentity("automatedEmailsJob", "group1")
                                                      .Build();

            var automatedEmailsTrigger = TriggerBuilder.Create()
                                                       .WithIdentity("automatedEmailsTrigger", "group1")
                                                       .StartAt(DateBuilder.FutureDate(15, IntervalUnit.Minute)) // start in 15 minutes
                                                       .WithSimpleSchedule(x => x.WithIntervalInHours(2).RepeatForever()) // repeat every 2 hours, forever
                                                       .Build();

            scheduler.ScheduleJob(automatedEmailsJob, automatedEmailsTrigger);
            scheduler.Start();
        }
    }
}