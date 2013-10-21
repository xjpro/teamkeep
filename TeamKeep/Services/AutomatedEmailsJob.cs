using System;
using System.Linq;
using Quartz;
using Teamkeep.Models;
using Teamkeep.Models.DataModels;
using System.Collections.Generic;

namespace Teamkeep.Services
{
    public class AutomatedEmailsJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            /*var emailService = new EmailService { AutomaticallySend = false };

            using (var entities = Database.GetEntities())
            {
                var teamDatas = entities.TeamSettingsDatas.Where(x => x.ConfirmationEmailMinutes != null).ToList();
                foreach (var teamData in teamDatas) // For each team that does availability emails
                {
                    var abWindow = DateTime.Now.AddMinutes((double)teamData.ConfirmationEmailMinutes);

                    var futureGameDatas = entities.GameDatas.Where(x =>
                        x.HomeTeamId == teamData.TeamId && // only games of this team
                        x.Date != null &&  // with a date set
                        x.Date.Value.CompareTo(DateTime.Now) > 0 && // and the date hasn't passed
                        abWindow.CompareTo(x.Date.Value) >= 0) // and now+the ab minutes is later than the date
                            .ToList();

                    foreach (var gameData in futureGameDatas) // For each game of this team which hasn't passed but is within availability email window
                    {
                        // Create ab datas for those players that do not have entries yet
                        foreach (var groupData in entities.PlayerGroupDatas.Where(x => x.TeamId == teamData.Id && x.SendConfirmations).ToList())
                        {
                            foreach (var playerData in entities.PlayerDatas.Where(x => x.GroupId == groupData.Id))
                            {
                                var abData = entities.AvailabilityDatas.SingleOrDefault(x => x.PlayerId == playerData.Id && x.EventId == gameData.Id);
                                if (abData == null)
                                {
                                    abData = new AvailabilityData
                                    {
                                        PlayerId = playerData.Id,
                                        EventId = gameData.Id
                                    };
                                    entities.AvailabilityDatas.AddObject(abData);
                                }
                            }
                        }
                        entities.SaveChanges();

                        var abDatas = entities.AvailabilityDatas.Where(x =>
                            x.EventId == gameData.Id &&
                            x.EmailSent == null).ToList(); // Limit to those that haven't had an email sent yet

                        if (abDatas.Count > 0)
                        {
                            var alreadySentEmails = new List<string>();

                            foreach (var abData in abDatas) // For each availability data of the future game, which has not had an email sent
                            {
                                // Filtering
                                var playerData = entities.PlayerDatas.Single(x => x.Id == abData.PlayerId);
                                if (!EmailService.IsValidEmail(playerData.Email)) continue; // Bad email

                                var groupIsEligible = entities.PlayerGroupDatas.Single(x => x.Id == playerData.GroupId).SendConfirmations;
                                if (!groupIsEligible) continue; // Player group doesn't recieve confirmations

                                if (alreadySentEmails.Contains(playerData.Email.ToLower())) continue; // Already sent this e-mail a confirmation
                                // End filtering

                                abData.EmailSent = DateTime.Now;
                                abData.Token = AuthToken.GenerateKey(playerData.Email);

                                var eventData = entities.GameDatas.Single(x => x.Id == abData.EventId);
                                var eventLocationData = entities.GameLocationDatas.Single(x => x.GameId == abData.EventId);
                                var abEvent = new Game(eventData) { Location = eventLocationData };

                                var abRequest = new AvailabilityRequest
                                {
                                    Data = abData,
                                    Event = abEvent,
                                    Email = playerData.Email,
                                    TeamName = entities.TeamDatas.Single(x => x.Id == abEvent.HomeTeamId).Name
                                };

                                // Send out the email!
                                emailService.EmailAvailability(abRequest);
                                alreadySentEmails.Add(playerData.Email.ToLower());
                            }
                        }

                        entities.SaveChanges();
                    }
                }
            }

            emailService.SendQueuedMessages();*/
        }
    }
}