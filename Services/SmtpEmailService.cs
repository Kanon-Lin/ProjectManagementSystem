using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace ProjectManagementSystem.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(
            IConfiguration configuration,
            ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_configuration["Smtp:Username"]); // 修正拼寫錯誤
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var client = new SmtpClient(
                    _configuration["Smtp:Host"],
                    int.Parse(_configuration["Smtp:Port"]));

                client.UseDefaultCredentials = false; // 加入這行
                client.Credentials = new NetworkCredential(
                    _configuration["Smtp:Username"],
                    _configuration["Smtp:Password"]
                );
                client.EnableSsl = true;

                _logger.LogInformation($"正在嘗試發送郵件到 {to}");
                await client.SendMailAsync(message);
                _logger.LogInformation($"郵件成功發送到 {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"發送郵件到 {to} 時發生錯誤");
                throw;
            }
        }
    }
}