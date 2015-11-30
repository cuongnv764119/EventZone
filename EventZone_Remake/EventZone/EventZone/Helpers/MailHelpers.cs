using System;
using System.Net;
using System.Net.Mail;

namespace EventZone.Helpers
{
    public class MailHelpers : SingletonBase<MailHelpers>
    {
        private const string SenderId = "eventzone.system@gmail.com";
        private const string SenderPassword = "mothaiba7601";
        private readonly MailMessage _mail;
        private readonly SmtpClient _smtp;

        private MailHelpers()
        {
            _mail = new MailMessage();
            _smtp = new SmtpClient
            {
                Host = "smtp.gmail.com", // smtp server address here…
                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(SenderId, SenderPassword),
                Timeout = 30000
            };
            _mail.From = new MailAddress(SenderId);
            _mail.IsBodyHtml = true;
        }

        public void SendMailWelcome(string email, string firstName, string lastName)
        {
            try
            {
                _mail.To.Add(new MailAddress(email));
                _mail.Subject = "An account has been created for you";
                _mail.Body = "Welcome " + firstName + " " + lastName + " to EventZone. " +
                             "<br/> You'll be excited to know a user account was just created for you at <a href='http://EventZone.com/'>EventZone website</a>." +
                             "<br/> <br/> Get Started Now" +
                             "<br/> <br/> If you have any technical difficulties,  just click the Contact Us link at the bottom of the Help page to contact your Program Administrator to get additional support." +
                             "<br/> If you have any questions, please mail us at: <a href='mailto:eventzone.system@gmail.com' target='_top'>eventzone.system@gmail.com</a> " +
                             "<br/> <br/> Thank you, <br/> <a href='http://EventZone.com'>EZ - EventZone</a>";
                _smtp.Send(_mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SendMailResetPassword(string email, string newPassword)
        {
            try
            {
                var message = new MailMessage(SenderId, email, "Reset password EventZone",
                    "Your new password: " + newPassword);
                _smtp.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}