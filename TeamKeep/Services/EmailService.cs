using System.Linq;
using TeamKeep.Models.DataModels;
using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TeamKeep.Models;

namespace TeamKeep.Services
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

        public void EmailPassword(PasswordReset passwordReset)
        {
            var body = new StringBuilder();
            body.Append("<p>A password reset was requested for your Team Keep login. If you did not make this request you may simply ignore this message. ");
            body.Append("Otherwise, please use to the following link to reset your password: </p>");
            body.Append(string.Format("<p><a href='{0}?email={1}&token={2}'>Reset your password</a></p>", "https://teamkeep.com/reset", passwordReset.Email, passwordReset.ResetToken));
            body.Append("<p>Thank you for using Team Keep.</p>");

            Enqueue(passwordReset.Email, "Password Reset for Team Keep", body.ToString(), "teamkeep-noreply@teamkeep.com", null);
            if(AutomaticallySend) SendQueuedMessages();
        }

        public void EmailWelcome(string email)
        {
            var body = new StringBuilder();
            body.Append("<h2>Welcome to Team Keep!</h2>");
            body.Append("<p>We are excited to see that you are trying out Team Keep as your team management system. ");
            body.Append("We hope you'll find it most useful. If you have any questions or comments please do not ");
            body.Append("hesitate to share your thoughts with us. Just reply to this e-mail.</p>");

            body.Append("<h3>Your login details:</h3>");
            body.Append("<table>");
            body.Append(string.Format("<tr><td style='width: 100px'>Login at:</td><td><a href='{0}'>{0}</a></tr>", "https://teamkeep.com"));
            body.Append(string.Format("<tr><td>Email:</td><td>{0}</a></tr>", email));
            body.Append("<tr><td>Password:</td><td>(entered when you signed up)</a></tr>");
            body.Append("</table>");

            body.Append("<p>Thanks again for choosing Team Keep!</p>");

            Enqueue(email, "Welcome to Team Keep", body.ToString(), "info@teamkeep.com", null);
            if (AutomaticallySend) SendQueuedMessages();
        }

        public void EmailAvailability(AvailabilityRequest abRequest)
        {
            var replyEmail = "https://teamkeep.com/rsvp?token=" + abRequest.Data.Token;

            var body = new StringBuilder();
            body.Append(string.Format("<h2>{0} vs. {1}</h2>", abRequest.TeamName, abRequest.Event.OpponentName ?? "[To Be Determined]"));
            //body.Append(string.Format("<p>{0}</p>", "The event description would go here, if there was one."));

            body.Append("<table>");
            if (abRequest.Event.When != null)
            {
                body.Append(string.Format("<tr><td style='width: 80px'>When:</td><td>{0}</tr>", abRequest.Event.When));
            }
            if (abRequest.Event.Where != null)
	        {
	            body.Append(string.Format("<tr><td>Where:</td><td>{0}</td></tr>", abRequest.Event.Where));
	        }
            else
            {
                body.Append(string.Format("<tr><td>Where:</td><td>{0}</td></tr>", "To Be Determined"));
            }
            if (abRequest.Event.Location != null && !string.IsNullOrWhiteSpace(abRequest.Event.Location.InternalLocation))
            {
                body.Append(string.Format("<tr><td>Arena:</td><td>{0}</tr>", abRequest.Event.Location.InternalLocation));
            }
            body.Append("</table>");

            body.Append("<h4>Can you make it?</h4>");
            body.Append("<p style='overflow: hidden'>");
            body.Append(string.Format("<a style='float: left; display: block; width: 70px; border: solid #666 1px; padding: 8px 5px; margin-right: 7px; text-align: center;' href='{0}'>Yes</a>", replyEmail + "&reply=1"));
            body.Append(string.Format("<a style='float: left; display: block; width: 70px; border: solid #666 1px; padding: 8px 5px; margin-right: 7px; text-align: center;' href='{0}'>No</a>", replyEmail + "&reply=2"));
            body.Append(string.Format("<a style='float: left; display: block; width: 70px; border: solid #666 1px; padding: 8px 5px; margin-right: 7px; text-align: center;' href='{0}'>Maybe</a>", replyEmail + "&reply=3"));
            body.Append("</p>");

            Enqueue(abRequest.Email, "[" + abRequest.TeamName + "] vs. " + (abRequest.Event.OpponentName ?? "TBD") + " @ " + abRequest.Event.When, 
                body.ToString(), "teamkeep-noreply@teamkeep.com", null);

            if (AutomaticallySend) SendQueuedMessages();
        }

        public Message EmailMessage(Message message)
        {
            var paragraphs = Regex.Split(message.Content, "\n\n");

            var body = new StringBuilder();
            foreach (var p in paragraphs)
            {
                var formattedParagraph = System.Web.HttpUtility.HtmlEncode(p);
                formattedParagraph = Regex.Replace(formattedParagraph, "\n", "<br/>");
                body.Append(string.Format("<p>{0}</p>", formattedParagraph));
            }

            // And now for the TeamKeep parts
            body.Append(string.Format("<hr/><p>This message sent on behalf of {0} by TeamKeep.com</p>", message.TeamName));

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

                    Enqueue(playerData.Email, "[" + message.TeamName + "] " + message.Subject, body.ToString(), "teamkeep-noreply@teamkeep.com", message.From);
                    sentToEmails.Add(playerData.Email);
                }

                // Store sent e-mail in db as MessageData
                string to = sentToEmails.Aggregate("", (current, sentTo) => current + (sentTo + "; ")).Trim();

                var messageData = new MessageData
                {
                    TeamId = message.TeamId,
                    Date = DateTime.Now,
                    To = to,
                    Subject = message.Subject,
                    Content = message.Content
                };
                entities.MessageDatas.AddObject(messageData);
                entities.SaveChanges();

                message.Id = messageData.Id;
                message.To = messageData.To;
                message.Date = messageData.Date;
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

        private MailMessage BaseMessage(string subject, string sender, string replyTo = null)
        {
            var message = new MailMessage
            {
                Subject = subject,
                BodyEncoding = Encoding.ASCII,
                IsBodyHtml = true,
                Sender = new MailAddress(sender, "Team Keep")
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