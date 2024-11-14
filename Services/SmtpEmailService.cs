
using System.Net;
using System.Net.Mail;
using System.Reflection;

namespace ProjectManagementSystem.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_configuration["Stmp:Username"]);
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(_configuration["Smtp:Host"],
            int.Parse(_configuration["Smtp:Port"]));
            client.Credentials = new NetworkCredential(
            _configuration["Smtp:Username"],
            _configuration["Smtp:Password"]
        );
            client.EnableSsl = true;

            await client.SendMailAsync(message);
        }
    }
}
