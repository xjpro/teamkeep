using System.Globalization;
using System.Linq;
using Teamkeep.Models.DataModels;
using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Teamkeep.Models;

namespace Teamkeep.Services
{
    public class EmailService
    {
        private readonly ILog log = LogManager.GetLogger("Log");

        private bool _automaticallySend = true;
        public bool AutomaticallySend
        {
            get { return _automaticallySend; }
            set { _automaticallySend = value; }
        }
        
        private readonly Queue<MailMessage> UnsentMessages = new Queue<MailMessage>();

        public void EmailPasswordReset(string email, string username, string resetToken)
        {
            var body = new StringBuilder();
            body.Append("<p>A password reset was requested for your Teamkeep login. If you did not make this request you may simply ignore this message. ");
            body.Append("Otherwise, please use to the following link to reset your password: </p>");
            body.Append(string.Format("<p><a href='{0}?username={1}&token={2}'>Reset your password</a></p>", "https://teamkeep.com/reset", username, resetToken));
            body.Append("<p>Thank you for using Teamkeep.</p>");

            Enqueue(email, "Password Reset for Teamkeep", body.ToString(), "teamkeep-noreply@teamkeep.com", null);
            if(AutomaticallySend) SendQueuedMessages();
        }

        public void EmailWelcome(string email, string username, string verifyCode = null)
        {
            var body = new StringBuilder();
            body.Append("<h2>Welcome to Teamkeep!</h2>");
            body.Append("<p>We are excited to see that you are trying out Teamkeep as your team management system. ");
            body.Append("We hope you'll find it most useful. If you have any questions or comments please do not ");
            body.Append("hesitate to share your thoughts with us. Just reply to this email.</p>");

            if (!string.IsNullOrEmpty(verifyCode))
            {
                body.Append("<h3>Please verify your email</h3>");
                body.Append("<p>Click or visit the link below to verify your email address:</p>");
                body.Append(string.Format("<p><a href='{0}'>{0}</a></p>", "https://teamkeep.com/users/verify?code=" + verifyCode));
            }

            body.Append("<h3>Your login details:</h3>");
            body.Append("<table>");
            body.Append(string.Format("<tr><td style='width: 100px'>Login at:</td><td><a href='{0}'>{0}</a></tr>", "https://teamkeep.com"));
            body.Append(string.Format("<tr><td>Username:</td><td>{0}</a></tr>", username));
            body.Append("<tr><td>Password:</td><td>(entered when you signed up)</a></tr>");
            body.Append("</table>");

            body.Append("<p>Thanks again for choosing Teamkeep!</p>");

            Enqueue(email, "Welcome to Teamkeep", body.ToString(), "info@teamkeep.com", null);
            if (AutomaticallySend) SendQueuedMessages();
        }

        public void EmailVerification(string email, string verifyCode)
        {
            var body = new StringBuilder();
            body.Append("<h2>Email verification</h2>");
            body.Append("<p>Click or visit the link below to verify your email address:</p>");
            body.Append(string.Format("<p><a href='{0}'>{0}</a></p>", "https://teamkeep.com/users/verify?code=" + verifyCode));

            Enqueue(email, "Teamkeep Email Verification", body.ToString(), "no-reply@teamkeep.com", null);
            if (AutomaticallySend) SendQueuedMessages();
        }

        public Message EmailMessage(Message message)
        {
            var body = new StringBuilder();

            // Start the body off with the 'message'
            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                body.Append(string.Format("<h2>Message from {0}</h2>", message.TeamName));

                var paragraphs = Regex.Split(message.Content, "\n\n");

                foreach (var p in paragraphs)
                {
                    var formattedParagraph = System.Web.HttpUtility.HtmlEncode(p);
                    formattedParagraph = Regex.Replace(formattedParagraph, "\n", "<br/>");
                    body.Append(string.Format("<p>{0}</p>", formattedParagraph));
                }
            }

            // Then add the availability parts
            if (message.RequestAvailability)
            {
                var replyEmail = "https://teamkeep.com/rsvp?token=TEAMKEEPRSVPTOKEN";

                Game.EventType type;
                Enum.TryParse(message.AvailabilityEvent.Type.ToString(CultureInfo.InvariantCulture), out type);

                switch (type)
                {
                    case Game.EventType.Game:
                        if (string.IsNullOrEmpty(message.AvailabilityEvent.OpponentName))
                        {
                            body.Append(string.Format("<h2>{0} vs. [To Be Determined]</h2>", message.TeamName));
                        }
                        else
                        {
                            body.Append(string.Format("<h2>{0} vs. {1}</h2>", message.TeamName, message.AvailabilityEvent.OpponentName));
                        }
                        break;
                    case Game.EventType.Practice:
                        if (string.IsNullOrEmpty(message.AvailabilityEvent.OpponentName))
                        {
                            body.Append("<h2>Practice</h2>");
                        }
                        else
                        {
                            body.Append(string.Format("<h2>Practice &mdash; {0}</h2>", message.AvailabilityEvent.OpponentName));
                        }
                        break;
                    case Game.EventType.Meeting:
                        if (string.IsNullOrEmpty(message.AvailabilityEvent.OpponentName))
                        {
                            body.Append("<h2>Meeting</h2>");
                        }
                        else
                        {
                            body.Append(string.Format("<h2>Meeting &mdash; {0}</h2>", message.AvailabilityEvent.OpponentName));
                        }
                        break;
                    case Game.EventType.Party:
                        if (string.IsNullOrEmpty(message.AvailabilityEvent.OpponentName))
                        {
                            body.Append("<h2>Party</h2>");
                        }
                        else
                        {
                            body.Append(string.Format("<h2>Party &mdash; {0}</h2>", message.AvailabilityEvent.OpponentName));
                        }
                        break;
                    default:
                        if (string.IsNullOrEmpty(message.AvailabilityEvent.OpponentName))
                        {
                            body.Append(string.Format("<h2>{0}</h2>", message.TeamName));
                        }
                        else
                        {
                            body.Append(string.Format("<h2>{0} &mdash; {1}</h2>",  message.TeamName, message.AvailabilityEvent.OpponentName));
                        }

                        break;
                }

                body.Append("<table>");
                if (message.AvailabilityEvent.When != null)
                {
                    body.Append(string.Format("<tr><td style='width: 80px'>When:</td><td>{0}</tr>", message.AvailabilityEvent.When));
                }
                if (message.AvailabilityEvent.Where != null)
                {
                    body.Append(string.Format("<tr><td>Where:</td><td>{0}</td></tr>", message.AvailabilityEvent.Where));
                }
                else
                {
                    body.Append(string.Format("<tr><td>Where:</td><td>{0}</td></tr>", "To Be Determined"));
                }
                if (message.AvailabilityEvent.Location != null && !string.IsNullOrWhiteSpace(message.AvailabilityEvent.Location.InternalLocation))
                {
                    body.Append(string.Format("<tr><td>Arena:</td><td>{0}</tr>", message.AvailabilityEvent.Location.InternalLocation));
                }
                body.Append("</table>");

                body.Append("<h4>Can you make it?</h4>");
                body.Append("<p style='overflow: hidden'>");
                body.Append(string.Format("<a style='float: left; display: block; width: 70px; border: solid #666 1px; padding: 8px 5px; margin-right: 7px; text-align: center;' href='{0}'>Yes</a>", replyEmail + "&reply=1"));
                body.Append(string.Format("<a style='float: left; display: block; width: 70px; border: solid #666 1px; padding: 8px 5px; margin-right: 7px; text-align: center;' href='{0}'>No</a>", replyEmail + "&reply=2"));
                body.Append(string.Format("<a style='float: left; display: block; width: 70px; border: solid #666 1px; padding: 8px 5px; margin-right: 7px; text-align: center;' href='{0}'>Maybe</a>", replyEmail + "&reply=3"));
                body.Append("</p>");
            }

            // Finally, add the footer
            body.Append(string.Format("<hr/><p>This message sent on behalf of {0} by Teamkeep.com</p>", message.TeamName));

            using (var entities = Database.GetEntities())
            {
                var sentToEmails = new List<string>();
                foreach (var recipentId in message.RecipientPlayerIds)
                {
                    var playerData = entities.PlayerDatas.Single(x => x.Id == recipentId);
                    var teamId = entities.PlayerGroupDatas.Single(x => x.Id == playerData.GroupId).TeamId;

                    if (message.TeamId != teamId) continue;
                    if (string.IsNullOrEmpty(playerData.Email)) continue;
                    if (sentToEmails.Contains(playerData.Email, StringComparer.InvariantCultureIgnoreCase)) continue;

                    var finishedBody = body.ToString(); // ensures a copy is made

                    if (message.RequestAvailability)
                    {
                        var abData = new AvailabilityData 
                        { 
                            EventId = message.AvailabilityEventId, 
                            PlayerId = playerData.Id,
                            Token = AuthToken.GenerateKey(playerData.Email),
                            EmailSent = DateTime.Now
                        };
                        entities.AvailabilityDatas.AddObject(abData);

                        finishedBody = finishedBody.Replace("TEAMKEEPRSVPTOKEN", abData.Token); // generate and replace TEAMKEEPRSVPTOKEN
                    }

                    Enqueue(playerData.Email, "[" + message.TeamName + "] " + message.Subject, finishedBody, "teamkeep-noreply@teamkeep.com", message.From);
                    sentToEmails.Add(playerData.Email);
                }

                // Store sent email in db as MessageData
                string to = sentToEmails.Aggregate("", (current, sentTo) => current + (sentTo + "; ")).Trim();

                var messageData = new MessageData
                {
                    TeamId = message.TeamId,
                    Date = DateTime.Now,
                    To = to,
                    Subject = message.Subject,
                    Content = new Regex(@"<hr/><p>This message sent on behalf of.*$").Replace(body.ToString(), string.Empty) // TODO seems sloppy
                };
                entities.MessageDatas.AddObject(messageData);
                entities.SaveChanges();

                message.Id = messageData.Id;
                message.To = messageData.To;
                message.Date = messageData.Date;
                message.Subject = messageData.Subject;
                message.Content = messageData.Content;
            }

            if (AutomaticallySend) SendQueuedMessages();

            return message;
        }

        public void SendQueuedMessages()
        {
            new Thread(ProcessQueue).Start();
        }

        private void Enqueue(string to, string subject, string body, string from, string replyTo)
        {
            if (IsValidEmail(to))
            {
                MailMessage message = BaseMessage(subject, from, replyTo);
                message.To.Add(to);
                message.Body = body;
                UnsentMessages.Enqueue(message);
            }
        }

        private void ProcessQueue()
        {
           using (var emailServer = new SmtpClient("smtp.gmail.com", 587))
           {
               emailServer.Credentials = new NetworkCredential("teamkeep.info@gmail.com", "ThisIsBatCountry");
               emailServer.EnableSsl = true;

               lock (UnsentMessages)
               {
                   foreach (var message in UnsentMessages)
                   {
                       try
                       {
                           emailServer.Send(message);
                           log.Info("Sent '" + message.Subject + "' to " + message.To);
                       }
                       catch (Exception ex)
                       {
                           log.Error("Failed to send '" + message.Subject + "' to " + message.To + " with exception: " +  ex.Message);
                       }
                   }
                   UnsentMessages.Clear();
               }
           }
        }

        public static string GetAvailabilitySubject(Game.EventType type, string teamName, string title)
        {
            switch (type)
            {
                case Game.EventType.Game:
                    return "vs. " + (title ?? "TBD");
                case Game.EventType.Practice:
                    return "Practice" + (!string.IsNullOrEmpty(title) ? " — " + title : string.Empty);
                case Game.EventType.Meeting:
                    return "Meeting" + (!string.IsNullOrEmpty(title) ? " — " + title : string.Empty);
                case Game.EventType.Party:
                    return "Party" + (!string.IsNullOrEmpty(title) ? " — " + title : string.Empty);
                default:
                    return title ?? "Untitled";
            }
        }

        private MailMessage BaseMessage(string subject, string sender, string replyTo = null)
        {
            var message = new MailMessage
            {
                Subject = subject,
                BodyEncoding = Encoding.ASCII,
                IsBodyHtml = true,
                From = new MailAddress(sender, "Teamkeep"),
                Sender = new MailAddress(sender, "Teamkeep")
            };

            if (!string.IsNullOrEmpty(replyTo))
            {
                message.ReplyTo = new MailAddress(replyTo);
            }

            return message;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            return new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b", RegexOptions.IgnoreCase).IsMatch(email.Trim());
        }
    }
}