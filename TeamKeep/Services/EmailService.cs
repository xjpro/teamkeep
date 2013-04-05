using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using TeamKeep.Models;
using TeamKeep.Models.DataModels;

namespace TeamKeep.Services
{
    public class EmailService
    {
        private Queue<MailMessage> UnsentMessages = new Queue<MailMessage>();

        public void EmailPassword(PasswordReset passwordReset)
        {
            var body = new StringBuilder();
            body.Append("<p>A password reset was requested for your Team Keep login. If you did not make this request you may simply ignore this message. ");
            body.Append("Otherwise, please use to the following link to reset your password: </p>");
            body.Append(string.Format("<p><a href='{0}?email={1}&token={2}'>Reset your password</a></p>", "https://teamkeep.com/reset", passwordReset.Email, passwordReset.ResetToken));
            body.Append("<p>Thank you for using Team Keep.</p>");

            Enqueue(passwordReset.Email, "teamkeep-noreply@teamkeep.com", "Password Reset for Team Keep", body.ToString());
            SendQueuedMessages();
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

            Enqueue(email, "info@teamkeep.com", "Welcome to Team Keep", body.ToString());
            SendQueuedMessages();
        }

        public void EmailAvailability(string email)
        {
        }

        public void SendQueuedMessages()
        {
            new Thread(ProcessQueue).Start();
        }

        private void Enqueue(string to, string from, string subject, string body)
        {
            /*using (var entities = Database.GetEntities())
            {
                entities.EmailQueueDatas.AddObject(new EmailQueueData
                {
                    Recipient = to,
                    From = from,
                    Subject = subject,
                    Message = body
                });
                entities.SaveChanges();
            }*/
            MailMessage message = BaseMessage(subject, from);
            message.To.Add(to);
            message.Body = body;
            UnsentMessages.Enqueue(message);
        }

        private void ProcessQueue()
        {
            /*using (var entities = Database.GetEntities())
            using (var emailServer = new SmtpClient("smtp.gmail.com", 587))
            {
                emailServer.Credentials = new NetworkCredential("teamkeep.info@gmail.com", "wtfMate?1");
                emailServer.EnableSsl = true;

                var emailDatas = entities.EmailQueueDatas.Select(x => x).ToList();

                foreach (var emailData in emailDatas)
                {
                    entities.DeleteObject(emailData);
                    entities.SaveChanges();

                    MailMessage message = BaseMessage(emailData.Subject, emailData.From);
                    message.To.Add(emailData.Recipient);
                    message.Body = emailData.Message;
                    emailServer.Send(message);
                }
            }*/

           using (var emailServer = new SmtpClient("smtp.gmail.com", 587))
           {
               emailServer.Credentials = new NetworkCredential("teamkeep.info@gmail.com", "wtfMate?1");
               emailServer.EnableSsl = true;

               lock (UnsentMessages)
               {
                   foreach (var message in UnsentMessages)
                   {
                       emailServer.Send(message);
                   }
                   UnsentMessages.Clear();
               }
           }
        }

        private MailMessage BaseMessage(string subject, string from)
        {
            return new MailMessage
            {
                Subject = subject,
                BodyEncoding = Encoding.ASCII,
                IsBodyHtml = true,
                From = new MailAddress(from, "Team Keep")
            };
        }
    }
}