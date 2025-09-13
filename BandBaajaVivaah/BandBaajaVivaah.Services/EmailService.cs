using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace BandBaajaVivaah.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var email = new MimeMessage();

            // Get the "From" address from config, or use a default
            var fromAddress = _configuration["SmtpSettings:Username"];
            email.From.Add(MailboxAddress.Parse(fromAddress));

            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Password Reset Request";
            email.Body = new TextPart("plain")
            {
                Text = $"Your password reset token is: {resetToken}"
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _configuration["SmtpSettings:Host"],
                int.Parse(_configuration["SmtpSettings:Port"]),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _configuration["SmtpSettings:Username"],
                _configuration["SmtpSettings:Password"]);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
