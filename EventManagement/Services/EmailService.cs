using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace EventManagement.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {

            var portString = _configuration["Smtp:Port"];
            if (!int.TryParse(portString, out int port))
            {
                throw new Exception("SMTP port is not configured correctly.");
            }

            var enableSslString = _configuration["Smtp:EnableSsl"];
            if (!bool.TryParse(enableSslString, out bool enableSsl))
            {
                throw new Exception("SMTP SSL setting is not configured correctly.");
            }

            var smtpClient = new SmtpClient(_configuration["Smtp:Host"], port)
            {
                Credentials = new NetworkCredential(
                    _configuration["Smtp:Email"],
                    _configuration["Smtp:Password"]
                ),

                EnableSsl = enableSsl,
            };

            var fromEmail = _configuration["Smtp:Email"];
            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new Exception("Sender email is not configured properly in appsettings.json.");
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            smtpClient.Send(mailMessage);
        }
    }
}
